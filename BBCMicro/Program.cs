using System;
using BbcMicro.Cpu;
using BbcMicro.Cpu.Diagnostics;
using BbcMicro.Cpu.Memory;
using BbcMicro.OS.Image;
using BbcMicro.OS.Image.Abstractions;

namespace BbcMicro
{
    internal class Program
    {
        private static bool _displayingStatus = true;
        private static bool _runningToCompletion = false;

        private static void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            if (_displayingStatus)
            {
                _cpuDisplay.Render();
            }
        }

        private static CPUDisplay _cpuDisplay;

        private static void DisplayAddress(byte newVal, byte oldVal, ushort address) {
            if (_runningToCompletion)
            {
                _cpuDisplay.ResetMessage();
            }
            RenderMessage($"${address:X4} <= ${newVal:X2} (${oldVal:X2})");
            if (address <= 0x01FF && address >= 0x0100)
            {
                RenderMessage("[stack]");
            }
        }

        private static void RenderMessage(string message) {
            if (_displayingStatus)
            {
                _cpuDisplay.RenderMessage(message);
            }
        }

        private static void ResetMessage() {
            if (_displayingStatus)
            {
                _cpuDisplay.ResetMessage();
            }
        }

        private static void Main(string[] args)
        {
            // Create CPU and address space
            var addressSpace = new FlatAddressSpace();
            var cpu = new CPU(addressSpace);

            // Set up the OS
            var os = new OS.OperatingSystem();
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

            // Read the image to execute
            IImageLoader imageLoader = null;
            if (args[0] == "core.bin")
            {
                imageLoader = new CoreFileLoader(cpu);
            }
            else
            {
                imageLoader = new DasmLoaderType2(addressSpace);
            }
            var imageInfo = imageLoader.Load(args[0]);

            // Execute the loaded image
            cpu.PC = imageInfo.EntryPoint;

            // Single step mode
            _cpuDisplay = new CPUDisplay(cpu);
            cpu.AddPostExecutionCallback(DisplayCallback);

            addressSpace.AddSetByteCallback((newVal, oldVal, address) => 
                DisplayAddress(newVal, oldVal, address));

            var done = false;
            while (!done)
            {
                var key = Console.ReadKey(true).Key;
                ResetMessage();

                switch (key)
                {
                    case ConsoleKey.X:
                        RenderMessage("Exiting...");
                        done = true;
                        break;

                    case ConsoleKey.C:
                        RenderMessage("Dumping core!");
                        CoreDumper.DumpCore(cpu);
                        break;

                    case ConsoleKey.R:
                        RenderMessage("Running to completion...");
                        _runningToCompletion = true;
                        cpu.ExecuteToBrk();     
                        done = true;
                        break;

                    case ConsoleKey.H:
                        _displayingStatus = false;
                        Console.Clear();
                        break;

                    default:
                        cpu.ExecuteNextInstruction();
                        break;
                }
            }
            Console.WriteLine();
        }
    }
}