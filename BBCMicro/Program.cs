using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BbcMicro.Cpu;
using BbcMicro.Cpu.Diagnostics;
using BbcMicro.Memory;
using BbcMicro.Memory.Abstractions;
using BbcMicro.OS.Image;
using BbcMicro.OS.Image.Abstractions;
using OS.Image;

namespace BbcMicro
{
    internal class Program
    {
        private static bool _displayingStatus = true;

        private static void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            if (_displayingStatus)
            {
                _cpuDisplay.Update();
            }
        }

        private static Debugger _cpuDisplay;

        private static void DisplayAddress(byte newVal, byte oldVal, ushort address)
        {
            var message = new StringBuilder($"${address:X4} <= ${newVal:X2} (${oldVal:X2})");

            if (address <= 0x01FF && address >= 0x0100)
            {
                message.Append(" [stack]");
            }
            RenderMessage(message.ToString());
        }

        private static void RenderMessage(string message)
        {
            if (_displayingStatus)
            {
                _cpuDisplay.WriteMessage(message);
            }
        }

        private static void DoSet(List<string> command)
        {
            byte value = byte.Parse(command[2], System.Globalization.NumberStyles.HexNumber);
            _cpu.A = value;
            _cpuDisplay.Update();
        }

        private static bool ProcessCommandLine(string commandLine)
        {
            var done = false;
            var commandParts = commandLine.Split(" ").Select(word => word.Trim()).ToList();
            switch (commandParts.ElementAt(0))
            {
                case "x":
                case "exit":
                    done = true;
                    _cpuDisplay.WriteResult("Exiting");
                    break;

                case "r":
                case "run":
                    _cpuDisplay.WriteResult("Running to completion...");
                    _cpu.ExecuteToBrk();
                    done = true;
                    break;

                case "set":
                    DoSet(commandParts);
                    break;

                default:
                    _cpuDisplay.WriteResult($"No such command '{commandLine}'.");
                    break;
            }

            return done;
        }

        private static CPU _cpu;
        private static FlatAddressSpace _addressSpace;

        private static void Main(string[] args)
        {
            // Create CPU and address space
            _addressSpace = new FlatAddressSpace();
            _cpu = new CPU(_addressSpace);

            // Set up the OS
            var os = new OS.OperatingSystem();
            _cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

            // Read the image to execute
            IImageLoader imageLoader = null;
            if (args[0] == "core.bin")
            {
                imageLoader = new CoreFileLoader(_cpu);
            }
            else
            {
                imageLoader = new DasmLoaderType2(_addressSpace);
            }
            var imageInfo = imageLoader.Load(args[0]);

            //Execute the loaded image
            _cpu.PC = imageInfo.EntryPoint;

            //var loader = new ROMLoader();
            //loader.Load("/Development/BBCRoms/OS-1.2.rom", 0xC000, addressSpace);
            //cpu.PC = 0xda42;// addressSpace.GetNativeWord(0xFFFC);

            // Single step mode
            _cpuDisplay = new Debugger(_cpu);
            _cpu.AddPostExecutionCallback(DisplayCallback);

            _addressSpace.AddSetByteCallback((newVal, oldVal, address) =>
                DisplayAddress(newVal, oldVal, address));

            // TODO
            Console.SetCursorPosition(0, 28);

            var done = false;
            while (!done)
            {
                var key = Console.ReadKey(true).Key;
                _cpuDisplay.ClearMessage();

                if (key == ConsoleKey.Spacebar || key == ConsoleKey.Enter)
                {
                    _cpu.ExecuteNextInstruction();
                }
                else
                {
                    _cpuDisplay.ClearCommand();
                    var firstChar = key.ToString().ToLower();
                    var commandLine = new StringBuilder(firstChar);
                    _cpuDisplay.WriteCommand(firstChar);
                    var commandLineComplete = false;
                    while (!commandLineComplete)
                    {
                        var commkey = Console.ReadKey(true);
                        commandLineComplete = (commkey.Key == ConsoleKey.Enter);
                        if (!commandLineComplete)
                        {
                            commandLine.Append(commkey.KeyChar);
                            _cpuDisplay.WriteCommand(commkey.KeyChar.ToString());
                        }
                    }

                    _cpuDisplay.ClearCommand();
                    _cpuDisplay.WriteResult(commandLine.ToString());
                    done = ProcessCommandLine(commandLine.ToString());
                }

                //switch (key)
                //{
                //    case ConsoleKey.X:
                //        RenderMessage("Exiting...");
                //        done = true;
                //        break;

                //    case ConsoleKey.C:
                //        RenderMessage("Dumping core!");
                //        CoreDumper.DumpCore(cpu);
                //        break;

                //    case ConsoleKey.R:
                //        RenderMessage("Running to completion...");
                //        cpu.ExecuteToBrk();
                //        done = true;
                //        break;

                //    case ConsoleKey.H:
                //        _displayingStatus = false;
                //        Console.Clear();
                //        break;

                //    default:
                //        cpu.ExecuteNextInstruction();
                //        break;
                //}
            }
            Console.WriteLine();
        }
    }
}