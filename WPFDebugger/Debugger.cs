using BbcMicro.Cpu;
using BbcMicro.Cpu.Diagnostics;
using BbcMicro.Cpu.Exceptions;
using BbcMicro.Memory.Extensions;
using BbcMicro.SystemConstants;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = BbcMicro.Cpu.CPU;
using Decoder = BbcMicro.Cpu.Decoder;

namespace BbcMicro.WPFDebugger
{
    // TODO - Many possible threading issues!
    public sealed class Debugger
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly CPU _cpu;
        private readonly Disassembler _dis = new Disassembler();
        private readonly Decoder _decoder = new Decoder();
        private readonly DebuggerDisplay _display;

        private string[] _symbols = new string[0xFFFF];

        private volatile bool _tracing = false;
        private volatile bool _break;

        public Debugger(DebuggerDisplay display, CPU cpu)
        {
            _display = display;

            _cpu = cpu;

            LoadSymbols();

            UpdateCPU();
            UpdateDis();

            _display.SetHideCallback(HideCallback);
            _display.SetShowCallback(ShowCallback);
            _display.SetCommandCallback(ProcessCommandLine);
        }

        private void LoadSymbols(Type theType)
        {
            var enumValues = theType.GetEnumValues();

            foreach (var enumValue in enumValues)
            {
                _symbols[(ushort)enumValue] = enumValue.ToString();
            }
        }

        private void LoadSymbols()
        {
            LoadSymbols(typeof(VDU));
            LoadSymbols(typeof(VIA));
            LoadSymbols(typeof(IRQ));
            LoadSymbols(typeof(SystemConstants.CPU));
        }

        private void UpdateCPU()
        {
            var stack = new byte[0XFF - _cpu.S];

            for (var offset = 0; offset < stack.Length; offset++)
            {
                stack[offset] = _cpu.Memory.GetByte((ushort)(0X1FF - offset));
            }

            _display.UpdateCpu(new ProcessorState
            {
                PC = _cpu.PC,
                S = _cpu.S,
                A = _cpu.A,
                X = _cpu.X,
                Y = _cpu.Y,
                P = _cpu.P
            }, stack.Reverse().ToArray());
        }

        private (byte[] memory, string dis) GetDis(ushort address)
        {
            (var opCode, var addressingMode) = _decoder.Decode(_cpu.Memory.GetByte(address));

            var memory = new byte[_decoder.GetAddressingModePCDelta(addressingMode) + 1];
            memory[0] = _cpu.Memory.GetByte(_cpu.PC);

            for (ushort pcOffset = 1; pcOffset < memory.Length; pcOffset++)
            {
                memory[pcOffset] = _cpu.Memory.GetByte((ushort)(_cpu.PC + pcOffset));
            }
            return (memory, _dis.Disassemble(address, _cpu.Memory));
        }

        private void Trace(ushort address)
        {
            (var opCode, var addressingMode) = _decoder.Decode(_cpu.Memory.GetByte(address));

            var memory = new byte[_decoder.GetAddressingModePCDelta(addressingMode) + 1];
            memory[0] = _cpu.Memory.GetByte(_cpu.PC);

            for (ushort pcOffset = 1; pcOffset < memory.Length; pcOffset++)
            {
                memory[pcOffset] = _cpu.Memory.GetByte((ushort)(_cpu.PC + pcOffset));
            }

            var disString = _dis.Disassemble(address, _cpu.Memory);
            var addrString = $"${address:X4}";
            var memString = string.Join(" ", memory.Select(b => $"${b:X2}"));
            var cpuString = _cpu.ToString();

            ushort operandAddress = (ushort)(address + 1);
            var operandValueString = addressingMode switch
            {
                AddressingMode.Accumulator => "",
                AddressingMode.Immediate => "",
                AddressingMode.Implied => "",
                AddressingMode.Relative => $"(${_cpu.PC + 2 + (sbyte)_cpu.Memory.GetByte(operandAddress):X4})",
                AddressingMode.Absolute => $"(${_cpu.Memory.GetByte(_cpu.Memory.GetNativeWord(operandAddress)):X2})",
                AddressingMode.ZeroPage => $"(${_cpu.Memory.GetByte(_cpu.Memory.GetByte(operandAddress)):X2})",
                AddressingMode.Indirect => $"(${_cpu.Memory.GetNativeWord(_cpu.Memory.GetNativeWord(operandAddress)):X4})",
                AddressingMode.AbsoluteIndexedX => $"(${_cpu.Memory.GetByte((ushort)(_cpu.Memory.GetNativeWord(operandAddress) + _cpu.X)):X2})",
                AddressingMode.AbsoluteIndexedY => $"(${_cpu.Memory.GetByte((ushort)(_cpu.Memory.GetNativeWord(operandAddress) + _cpu.Y)):X2})",
                AddressingMode.ZeroPageIndexedX => $"(${_cpu.Memory.GetByte((byte)(_cpu.Memory.GetByte(operandAddress) + _cpu.X)):X2})",
                AddressingMode.ZeroPageIndexedY => $"(${_cpu.Memory.GetByte((byte)(_cpu.Memory.GetByte(operandAddress) + _cpu.Y)):X2})",
                AddressingMode.IndexedXIndirect => $"(${_cpu.Memory.GetByte(_cpu.Memory.GetNativeWord((byte)(_cpu.Memory.GetByte(operandAddress) + _cpu.X))):X2})",
                AddressingMode.IndirectIndexedY => $"(${_cpu.Memory.GetByte((ushort)(_cpu.Memory.GetNativeWord(_cpu.Memory.GetByte(operandAddress)) + _cpu.Y)):X2})",
                _ => throw new CPUException($"Invalid addressing mode '{addressingMode}'")
            };

            var symbol = _symbols[_cpu.PC];
            if (symbol != null)
            {
                _logger.Trace($"{symbol}:");
            }
            _logger.Trace($"{addrString,-5} {memString,-12} {disString,-11} {operandValueString,-8} {cpuString}");
        }

        private void UpdateDis()
        {
            (var memory, var dis) = GetDis(_cpu.PC);
            _display.AddDis(_cpu.PC, memory, dis);
        }

        private void UpdateStack()
        {
            var stack = new byte[0XFF - _cpu.S];

            for (var offset = 0; offset < stack.Length; offset++)
            {
                stack[offset] = _cpu.Memory.GetByte((ushort)(0X1FF - offset));
            }
            _display.UpdateStack(stack.Reverse().ToArray());
        }

        private void MemoryChangedCallback(byte newVal, byte oldVal, ushort address)
        {
            _display.AddMem(oldVal, newVal, address);
        }

        private void CpuChangedCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            UpdateCPU();
            UpdateDis();
            UpdateStack();
        }

        private void Error(string value = "Error")
        {
            _display.AddMessage(value, false, true);
        }

        private readonly List<ushort> _breakPoints = new List<ushort>();

        private bool ParseHexWord(string value, out ushort word)
        {
            var ok = ushort.TryParse(value,
                        System.Globalization.NumberStyles.HexNumber,
                        null,
                        out word);

            if (!ok)
            {
                Error($"Could not parse '{value}' as hex.");
            }

            return ok;
        }

        private bool ParseDecInt(string value, out ushort parsed)
        {
            var ok = ushort.TryParse(value, out parsed);

            if (!ok)
            {
                Error($"Could not parse '{value}' as base 10 integer.");
            }

            return ok;
        }

        private bool ParseNumber(string value, out ushort parsed)
        {
            var lcValue = value.ToLower().Trim();

            if (lcValue.StartsWith("$"))
            {
                return ParseHexWord(value.Substring(1), out parsed);
            }
            else
            if (lcValue.StartsWith("0x"))
            {
                return ParseHexWord(value.Substring(2), out parsed);
            }
            else
            {
                return ParseDecInt(value, out parsed);
            }
        }

        private const string TRON_CMD = "tron";
        private const string TRON_USAGE = TRON_CMD + " - Start tracing execution";

        private const string TROFF_CMD = "troff";
        private const string TROFF_USAGE = TROFF_CMD + " - Stop tracing execution";

        private const string BREAK_CMD = "b";
        private const string BREAK_USAGE = BREAK_CMD + " - Break into execution";

        private const string STEP_IN_CMD = "s";
        private const string STEP_IN_USAGE = STEP_IN_CMD + " - Single step in";

        private const string STEP_OVER_CMD = "o";
        private const string STEP_OVER_USAGE = STEP_OVER_CMD + " - Single step over";

        private const string RUN_CMD = "r";
        private const string RUN_USAGE = RUN_CMD + " - Run";

        private const string RUN_TO_RTS_CMD = "t";
        private const string RUN_TO_RTS_CMD_USAGE = RUN_TO_RTS_CMD + " - Return from subroutine";

        private const string SET_CMD = "set";
        private const string SET_USAGE = SET_CMD + " PC|S|A|X|Y|S|<addr> <value> - Set value";

        private const string SET_BP_CMD = "sb";
        private const string SET_BP_USAGE = SET_BP_CMD + " [addr] - Set breakpoint";

        private const string LIST_BP_CMD = "lb";
        private const string LIST_BP_USAGE = LIST_BP_CMD + " - List breakpoints";

        private const string CLEAR_BP_CMD = "cb";
        private const string CLEAR_BP_USAGE = CLEAR_BP_CMD + " <id> - Clear breakpoint";

        private const string SET_BPMW_CMD = "sbmw";
        private const string SET_BPMW_USAGE = SET_BPMW_CMD + " <addr> [length]- Set breakpoint on memory write";

        private const string LIST_BPMW_CMD = "lbmw";
        private const string LIST_CPMW_USAGE = LIST_BPMW_CMD + " - List memory write breakpoints";

        private const string CLEAR_BPMW_CMD = "cbmw";
        private const string CLEAR_BPMW_USAGE = CLEAR_BPMW_CMD + " <id> - Clear memory write breakpoint";

        private const string SET_BPMR_CMD = "sbmr";
        private const string SET_BPMR_USAGE = SET_BPMR_CMD + " <addr> [length]- Set breakpoint on memory read";

        private const string LIST_BPMR_CMD = "lbmr";
        private const string LIST_CPMR_USAGE = LIST_BPMR_CMD + " - List memory read breakpoints";

        private const string CLEAR_BPMR_CMD = "cbmr";
        private const string CLEAR_BPMR_USAGE = CLEAR_BPMR_CMD + " <id> - Clear memory read breakpoint";

        private const string SET_MM_CMD = "smm";
        private const string SET_MM_USAGE = SET_MM_CMD + " <addr> [length]- Set memory monitor";

        private const string LIST_MM_CMD = "lmm";
        private const string LIST_MM_USAGE = LIST_MM_CMD + " - List memory monitors";

        private const string CLEAR_MM_CMD = "cmm";
        private const string CLEAR_MM_USAGE = CLEAR_MM_CMD + " <id> - Clear memory monitor";

        private const string LM_CMD = "lm";
        private const string LM_USGE = LM_CMD + " [addr] [length] - List memory";

        private const string LD_CMD = "ld";
        private const string LD_USAGE = LD_CMD + " [addr] [length] - List disassembly";

        private const string C_CMD = "c";
        private const string C_USAGE = C_CMD + " - Dump core image";

        private const string HELP_CMD = "h";

        private void SetBreakPoint(List<string> command)
        {
            if (command.Count <= 2)
            {
                var ok = true;
                ushort breakpointAddress = _cpu.PC;

                if (command.Count() == 2)
                {
                    ok = ParseNumber(command[1], out breakpointAddress);
                }

                if (ok)
                {
                    _breakPoints.Add(breakpointAddress);
                    _display.AddMessage($"Set breakpoint {_breakPoints.Count() - 1} at ${breakpointAddress:X4}");
                }
            }
            else
            {
                Error(SET_BP_USAGE);
            }
        }

        private void ListBreakPoints(List<string> command)
        {
            if (command.Count == 1)
            {
                for (var i = 0; i < _breakPoints.Count; i++)
                {
                    _display.AddMessage($"{i}: {_breakPoints[i]:X4}");
                }
            }
            else
            {
                Error(LIST_BP_USAGE);
            }
        }

        private void ClearBreakPoint(List<string> command)
        {
            if (command.Count == 2)
            {
                if (ParseDecInt(command[1], out var operand))
                {
                    if (operand < _breakPoints.Count)
                    {
                        _display.AddMessage($"Cleared breakpoint {operand} at ${_breakPoints[operand]:X4}");
                        _breakPoints.RemoveAt(operand);
                    }
                    else
                    {
                        Error($"Breakpoint {operand} does not exist");
                    }
                }
            }
            else
            {
                Error(CLEAR_BP_USAGE);
            }
        }

        private void ListDis(List<string> command)
        {
            var ok = true;

            ushort count = 0x05;
            ushort baseAddress = _cpu.PC;

            if (command.Count() == 3)
            {
                ok = ParseNumber(command[2], out count);
            }

            if (ok && command.Count() >= 2)
            {
                ok = ParseNumber(command[1], out baseAddress);
            }

            if (ok)
            {
                var illegal = false;
                var address = baseAddress;
                for (var i = 0; i < count && !illegal; i++)
                {
                    try
                    {
                        (var memory, var dis) = GetDis(address);
                        _display.AddMessage($"${address:X4} {dis}");
                        address += (ushort)(memory.Length);
                    }
                    catch (Exception)
                    {
                        illegal = true;
                        Error($"Illegal instruction ${_cpu.Memory.GetByte(address):X2} at ${address:X4}");
                    }
                }
            }

            if (!ok)
            {
                Error(LD_USAGE);
            }
        }

        private void ListMemory(List<string> command)
        {
            var ok = true;

            ushort length = 0x40;
            ushort baseAddress = _cpu.PC;

            if (command.Count() == 3)
            {
                ok = ParseNumber(command[2], out length);
            }

            if (ok && command.Count() >= 2)
            {
                ok = ParseNumber(command[1], out baseAddress);
            }

            if (ok)
            {
                var offset = 0;
                while (offset < length)
                {
                    var line = new StringBuilder($"{(baseAddress + offset):X4}");
                    var hexLine = new StringBuilder();
                    var charLine = new StringBuilder();

                    for (var batchOffset = 0; batchOffset < 16; batchOffset++)
                    {
                        if (offset + batchOffset >= length)
                        {
                            break;
                        }
                        byte currentByte = _cpu.Memory.GetByte((ushort)(baseAddress + offset + batchOffset));
                        hexLine.Append($" {currentByte:X2}");
                        char currentChar = (char)currentByte;
                        charLine.Append((Char.IsControl(currentChar) || Char.IsWhiteSpace(currentChar)) ? '.' : currentChar);
                    }
                    offset += 16;
                    _display.AddMessage(line.Append(" ").Append(hexLine).Append(" ").Append(charLine).ToString());
                }
            }

            if (!ok)
            {
                Error(LM_USGE);
            }
        }

        private void Set(List<string> command)
        {
            if (command.Count() == 3)
            {
                if (ParseNumber(command[2], out var operand))
                {
                    switch (command[1].ToLower())
                    {
                        case "a":
                            _cpu.A = (byte)operand;
                            UpdateCPU();
                            break;

                        case "s":
                            _cpu.S = (byte)operand;
                            UpdateCPU();
                            break;

                        case "x":
                            _cpu.X = (byte)operand;
                            UpdateCPU();
                            break;

                        case "y":
                            _cpu.Y = (byte)operand;
                            UpdateCPU();
                            break;

                        case "pc":
                            _cpu.PC = operand;
                            UpdateCPU();
                            UpdateDis();
                            break;

                        case "p":
                            _cpu.P = (byte)operand;
                            UpdateCPU();
                            break;

                        default:
                            if (ParseNumber(command[1], out var address))
                            {
                                _cpu.Memory.SetByte((byte)operand, address);
                            }
                            else
                            {
                                Error(SET_USAGE);
                            }
                            break;
                    }
                }
                else
                {
                    Error(SET_USAGE);
                }
            }
            else
            {
                Error(SET_USAGE);
            }
        }

        private volatile bool _running = false;

        private void AddCallbacks()
        {
            _cpu.Memory.AddSetByteCallback(MemoryChangedCallback);
            _cpu.AddPostExecutionCallback(CpuChangedCallback);
        }

        private void RemoveCallbacks()
        {
            _cpu.Memory.RemoveSetByteCallback(MemoryChangedCallback);
            _cpu.RemovePostExecutionCallback(CpuChangedCallback);
        }

        private void ShowCallback(DebuggerDisplay display)
        {
            if (!_running)
            {
                AddCallbacks();
            }
        }

        private void HideCallback(DebuggerDisplay display)
        {
        }

        public void Execute(bool stopAfterRts)
        {
            _running = true;

            // Clear all and stop updates
            _display.Hide();
            RemoveCallbacks();
            _display.Background();
            _display.ClearDis();
            _display.ClearMem();
            _display.ClearStack();
            _display.ClearCpu();

            // Have we run until an RTS
            var rtsDone = false;

            // Done because of hitting a breakpoint
            var bpDone = false;

            // If we are stopping at RTS then it needs to be
            // the stack is a at the right level, so store the current value
            byte savedSp = _cpu.S;

            // We'll always execute one instruction
            do
            {
                // Byte at the PC
                var curByte = _cpu.Memory.GetByte(_cpu.PC);

                // Is the current instruction a RTS
                // If so we might be done depending on the depth of the stack
                var executedRts = curByte == (byte)0X60;

                _cpu.ExecuteNextInstruction();

                // We are done because of an RTS if we've executed an RTS
                // and the stack is at the right level
                rtsDone = stopAfterRts && executedRts && _cpu.S == savedSp + 2;

                // We are done because of a breakpoint if we have hit one!
                bpDone = _breakPoints.Contains(_cpu.PC);
            } while (!rtsDone && !bpDone && !_break);

            if (bpDone)
            {
                _display.AddMessage($"Stopped at breakpoint {_breakPoints.IndexOf(_cpu.PC)} at ${_cpu.PC:X4}");
            }
            else if (rtsDone)
            {
                _display.AddMessage($"Returned from subroutine");
            }
            else if (_break)
            {
                _display.AddMessage($"Broken into execution");
                _break = false;
            }

            _running = false;

            AddCallbacks();
            _display.Foreground();
            AddCallbacks();
            UpdateStack();
            UpdateCPU();
            UpdateDis();

            _display.Show();
        }

        private void DumpCore()
        {
            CoreDumper.DumpCore(_cpu);
        }

        private void Help()
        {
            _display.AddMessage("Commands", true);
            _display.AddMessage(BREAK_USAGE);
            _display.AddMessage(STEP_IN_USAGE);
            //_display.AddMessage(STEP_OVER_USAGE);
            _display.AddMessage(RUN_USAGE);
            _display.AddMessage(RUN_TO_RTS_CMD_USAGE);
            _display.AddMessage(SET_USAGE);
            _display.AddMessage(SET_BP_USAGE);
            _display.AddMessage(LIST_BP_USAGE);
            _display.AddMessage(CLEAR_BP_USAGE);
            _display.AddMessage(C_USAGE);
            //_display.AddMessage(SET_BPMW_USAGE);
            //_display.AddMessage(LIST_CPMW_USAGE);
            //_display.AddMessage(CLEAR_BPMW_USAGE);
            //_display.AddMessage(SET_BPMR_USAGE);
            //_display.AddMessage(LIST_CPMR_USAGE);
            //_display.AddMessage(CLEAR_BPMR_USAGE);
            //_display.AddMessage(SET_MM_USAGE);
            //_display.AddMessage(LIST_MM_USAGE);
            //_display.AddMessage(CLEAR_MM_USAGE);
            _display.AddMessage(LM_USGE);
            _display.AddMessage(LD_USAGE);
            _display.AddMessage(TRON_USAGE);
            _display.AddMessage(TROFF_USAGE);
        }

        private void ProcessCommandLine(string commandLine, DebuggerDisplay source)
        {
            var commandParts = commandLine.Split(" ").Select(word => word.Trim()).ToList();
            if (commandParts.Count == 0 || (commandParts.Count == 1 && commandParts[0].Length == 0))
            {
                // Default command
                commandParts = new List<string> { STEP_IN_CMD };
            }

            switch (commandParts.ElementAt(0).ToLower())
            {
                case TRON_CMD:
                    _tracing = true;
                    _display.AddMessage("Enabled execution tracing");
                    break;

                case TROFF_CMD:
                    _tracing = false;
                    _display.AddMessage("Disabled execution treacing");
                    break;

                case BREAK_CMD:
                    _display.AddMessage("Breaking into execution");
                    _break = true;
                    break;

                case STEP_IN_CMD:
                    if (_tracing)
                    {
                        Trace(_cpu.PC);
                    }
                    _cpu.ExecuteNextInstruction();
                    break;

                case RUN_CMD:
                    _display.AddMessage("Running until breakpoint");
                    Execute(false);
                    break;

                case RUN_TO_RTS_CMD:
                    _display.AddMessage("Returning from subroutine");
                    Execute(true);
                    break;

                case SET_CMD:
                    Set(commandParts);
                    break;

                case SET_BP_CMD:
                    SetBreakPoint(commandParts);
                    break;

                case LIST_BP_CMD:
                    ListBreakPoints(commandParts);
                    break;

                case CLEAR_BP_CMD:
                    ClearBreakPoint(commandParts);
                    break;

                case LM_CMD:
                    ListMemory(commandParts);
                    break;

                case LD_CMD:
                    ListDis(commandParts);
                    break;

                case C_CMD:
                    _display.AddMessage("Dumping core");
                    DumpCore();
                    break;

                case HELP_CMD:
                    Help();
                    break;

                default:
                    Error($"No such command '{commandLine}'");
                    break;
            }
        }
    }
}