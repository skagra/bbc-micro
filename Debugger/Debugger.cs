using BbcMicro.ConsoleWindowing;
using BbcMicro.Cpu;
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

        private void UpdateDis()
        {
            (var opCode, var addressingMode) = _decoder.Decode(_cpu.Memory.GetByte(_cpu.PC));

            var memory = new byte[_decoder.GetAddressingModePCDelta(addressingMode)];
            for (ushort pcOffset = 0; pcOffset < memory.Length; pcOffset++)
            {
                memory[pcOffset] = _cpu.Memory.GetByte((ushort)(_cpu.PC + pcOffset));
            }
            _display.WriteDis(_cpu.PC,
                memory,
                _dis.Disassemble(_cpu));
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

        private void DoSet(List<string> command)
        {
            byte value = byte.Parse(command[2], System.Globalization.NumberStyles.HexNumber);
            _cpu.A = value;
            UpdateCPU();
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
                    _display.WriteResult("Running to completion...");
                    _cpu.ExecuteToBrk();
                    done = true;
                    break;

                case "set":
                    DoSet(commandParts);
                    break;

                default:
                    _display.WriteResult($"No such command '{commandLine}'.");
                    break;
            }

            return done;
        }

        public void Run()
        {
            var done = false;
            while (!done)
            {
                var firstKey = Console.ReadKey(true).Key;

                _display.ClearMemory();
                _display.ClearCommand();
                _display.WriteCommand("> ");

                if (firstKey == ConsoleKey.Spacebar || firstKey == ConsoleKey.Enter)
                {
                    _cpu.ExecuteNextInstruction();
                }
                else
                {
                    var firstCommandChar = firstKey.ToString().ToLower();
                    var commandLine = new StringBuilder(firstCommandChar);
                    _display.WriteCommand(firstCommandChar);

                    var commandLineComplete = false;
                    while (!commandLineComplete)
                    {
                        var commandKey = Console.ReadKey(true);
                        commandLineComplete = (commandKey.Key == ConsoleKey.Enter);
                        if (!commandLineComplete)
                        {
                            commandLine.Append(commandKey.KeyChar);
                            _display.WriteCommand(commandKey.KeyChar.ToString());
                        }
                    }

                    _display.ClearCommand();
                    _display.WriteResult(commandLine.ToString());

                    done = ProcessCommandLine(commandLine.ToString());
                }
            }
        }
    }
}