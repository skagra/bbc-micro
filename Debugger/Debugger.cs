using BbcMicro.Cpu;
using BbcMicro.Memory.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BbcMicro.Debugger
{
    public sealed class Debugger
    {
        private readonly CPU _cpu;
        private readonly Disassembler _dis = new Disassembler();
        private readonly Cpu.Decoder _decoder = new Cpu.Decoder();
        private readonly Display _display = new Display();

        public Debugger(CPU cpu)
        {
            _cpu = cpu;
            _cpu.Memory.AddSetByteCallback((newVal, oldVal, address) =>
                DisplayAddress(newVal, oldVal, address));
            _cpu.AddPostExecutionCallback(DisplayCallback);

            UpdateCPU();
            UpdateDis();
        }

        private void UpdateCPU()
        {
            _display.WriteCPU(new ProcessorState
            {
                PC = _cpu.PC,
                S = _cpu.S,
                A = _cpu.A,
                X = _cpu.X,
                Y = _cpu.Y,
                P = _cpu.P
            });
        }

        private (byte[] memory, string dis) GetDis(ushort address)
        {
            (var opCode, var addressingMode) = _decoder.Decode(_cpu.Memory.GetByte(_cpu.PC));

            var memory = new byte[_decoder.GetAddressingModePCDelta(addressingMode) + 1];
            for (ushort pcOffset = 0; pcOffset < memory.Length; pcOffset++)
            {
                memory[pcOffset] = _cpu.Memory.GetByte((ushort)(_cpu.PC + pcOffset));
            }
            return (memory, _dis.Disassemble(address, _cpu.Memory));
        }

        private void UpdateDis()
        {
            (var memory, var dis) = GetDis(_cpu.PC);

            _display.WriteDis(_cpu.PC, memory, dis);
        }

        private void DisplayAddress(byte newVal, byte oldVal, ushort address)
        {
            var message = new StringBuilder($"${address:X4} <= ${newVal:X2} (${oldVal:X2})");

            if (address <= 0x01FF && address >= 0x0100)
            {
                message.Append(" [stack]");
            }
            _display.WriteMemory(message.ToString());
        }

        private void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            UpdateCPU();
            UpdateDis();
        }

        private void Error()
        {
            Console.Beep();
        }

        private List<ushort> _breakPoints = new List<ushort>();

        private void AddBreakPoint(List<string> command)
        {
            if (command.Count() == 2)
            {
                if (ushort.TryParse(command[1],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out var operand))
                {
                    _breakPoints.Add(operand);

                    _display.WriteResult($"Added breakpoint {_breakPoints.Count() - 1} at ${operand:X4}");
                }
                else
                {
                    Error();
                }
            }
            else
            {
                Error();
            }
        }

        private void ListBreakPoints(List<string> command)
        {
            if (command.Count == 1)
            {
                for (var i = 0; i < _breakPoints.Count; i++)
                {
                    _display.WriteResult($"{i}: {_breakPoints[i]:X4}");
                }
            }
            else
            {
                Error();
            }
        }

        private void DeleteBreakPoint(List<string> command)
        {
            if (command.Count == 2)
            {
                if (int.TryParse(command[1], out var operand))
                {
                    if (operand < _breakPoints.Count)
                    {
                        _display.WriteResult($"Cleared breakpoint {operand} at ${_breakPoints[operand]:X4}");
                        _breakPoints.RemoveAt(operand);
                    }
                    else
                    {
                        _display.WriteResult($"Breakpoint {operand} does not exist");
                        Error();
                    }
                }
            }
            else
            {
                Error();
            }
        }

        private void ListDis(List<string> command)
        {
            if (command.Count == 3)
            {
                if (ushort.TryParse(command[1],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out var startingAddress) &&
                    ushort.TryParse(command[2], out var count))
                {
                    var address = startingAddress;
                    for (var i = 0; i < count; i++)
                    {
                        try
                        {
                            (var memory, var dis) = GetDis(address);
                            _display.WriteResult($"${address:X4} {dis}");
                            address += (ushort)memory.Length;
                        }
                        catch (Exception)
                        {
                            _display.WriteResult($"${address:X4} -");
                            address += 1;
                        }
                    }
                }
                else
                {
                    Error();
                }
            }
            else
            {
                Error();
            }
        }

        private void ListMemory(List<string> command)
        {
            if (command.Count == 3)
            {
                if (ushort.TryParse(command[1],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out var startingAddress) &&
                    ushort.TryParse(command[2], out var count))
                {
                    var offset = 0;
                    while (offset < count)
                    {
                        var line = new StringBuilder($"{(startingAddress + offset):X4}");

                        for (var batchOffset = 0; batchOffset < 16; batchOffset++)
                        {
                            if (offset + batchOffset >= count)
                            {
                                break;
                            }
                            line.Append($" {_cpu.Memory.GetByte((ushort)(startingAddress + offset + batchOffset)):X2}");
                        }
                        offset += 16;
                        _display.WriteResult(line.ToString());
                    }
                }
                else
                {
                    Error();
                }
            }
            else
            {
                Error();
            }
        }

        private void DoSet(List<string> command)
        {
            if (command.Count() == 3)
            {
                if (ushort.TryParse(command[2],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out var operand))
                {
                    switch (command[1])
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

                        case "PC":
                            _cpu.PC = operand;
                            UpdateCPU();
                            UpdateDis();
                            break;

                        case "P":
                            _cpu.P = (byte)operand;
                            UpdateCPU();
                            break;

                        default:
                            if (ushort.TryParse(command[1],
                                System.Globalization.NumberStyles.HexNumber,
                                null,
                                out var address))
                            {
                                _cpu.Memory.SetByte((byte)operand, address);
                            }
                            else
                            {
                                Error();
                            }
                            break;
                    }
                }
                else
                {
                    Error();
                }
            }
            else
            {
                Error();
            }
        }

        private bool ExecuteToBreakPoint()
        {
            var done = false;
            _cpu.ExecuteNextInstruction();
            while (!_breakPoints.Contains(_cpu.PC) && !done)
            {
                done = _cpu.Memory.GetByte(_cpu.PC) == (byte)0;
                if (!done)
                {
                    _cpu.ExecuteNextInstruction();
                }
            }
            if (!done)
            {
                _display.WriteResult($"Stopped at breakpoint {_breakPoints.IndexOf(_cpu.PC)} at ${_cpu.PC:X4}");
            }
            return done;
        }

        private void Help()
        {
            _display.WriteResult("mem <address> <count> - Dump memory");
            _display.WriteResult("dis <address> <count> - Disassemble");
            _display.WriteResult("set PC|S|A|X|Y|P|<address> <value> - Set register or memory");
            _display.WriteResult("r|run - Run until breakpoint");
            _display.WriteResult("sb <address> - Set breakpoint");
            _display.WriteResult("lb - List breakpoints");
            _display.WriteResult("db <breakpoint> - Delete breakpoint");
            _display.WriteResult("x|exit - Exit");
        }

        private bool ProcessCommandLine(string commandLine)
        {
            var done = false;
            var commandParts = commandLine.Split(" ").Select(word => word.Trim()).ToList();
            switch (commandParts.ElementAt(0))
            {
                case "x":
                case "exit":
                    done = true;
                    _display.WriteResult("Exiting");
                    break;

                case "r":
                case "run":
                    _display.WriteResult("Running to breakpoint");
                    done = ExecuteToBreakPoint();
                    break;

                case "s":
                case "set":
                    DoSet(commandParts);
                    break;

                case "sb":
                    AddBreakPoint(commandParts);
                    break;

                case "lb":
                    ListBreakPoints(commandParts);
                    break;

                case "cb":
                    DeleteBreakPoint(commandParts);
                    break;

                case "mem":
                    ListMemory(commandParts);
                    break;

                case "dis":
                    ListDis(commandParts);
                    break;

                case "?":
                case "h":
                    Help();
                    break;

                default:
                    _display.WriteResult($"No such command '{commandLine}'.");
                    Error();
                    break;
            }

            return done;
        }

        private string GetCommandLine(ConsoleKey firstKey)
        {
            var firstCommandChar = firstKey.ToString().ToLower();
            var commandLine = new StringBuilder(firstCommandChar);
            _display.WriteCommand(firstCommandChar);

            var commandLineComplete = false;
            while (!commandLineComplete)
            {
                var keyInfo = Console.ReadKey(true);
                var key = keyInfo.Key;

                if ((key >= ConsoleKey.A && key <= ConsoleKey.Z) ||
                    (key >= ConsoleKey.D0 && key <= ConsoleKey.D9) ||
                    key == ConsoleKey.Spacebar)
                {
                    commandLine.Append(keyInfo.KeyChar);
                    _display.WriteCommand(keyInfo.KeyChar.ToString());
                }
                else if (key == ConsoleKey.Backspace)
                {
                    if (commandLine.Length > 0)
                    {
                        commandLine.Remove(commandLine.Length - 1, 1);
                        _display.ClearCommand();
                        _display.WriteCommand(commandLine.ToString());
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    commandLineComplete = true;
                }
                else
                {
                    Console.Beep();
                }
            }

            _display.ClearCommand();
            _display.WriteResult($"> {commandLine}");

            return commandLine.ToString();
        }

        public void Run()
        {
            var done = false;
            while (!done)
            {
                var firstKey = Console.ReadKey(true).Key;

                _display.ClearMemory();
                _display.ClearCommand();

                if (firstKey == ConsoleKey.Spacebar || firstKey == ConsoleKey.Enter)
                {
                    _cpu.ExecuteNextInstruction();
                }
                else if (firstKey >= ConsoleKey.A && firstKey < ConsoleKey.Z)
                {
                    done = ProcessCommandLine(GetCommandLine(firstKey));
                }
                else
                {
                    Console.Beep();
                }
            }
        }
    }
}