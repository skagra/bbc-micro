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
        private static bool _displayCPU = true;

        private static void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            if (_displayCPU)
            {
                _cpuDisplay.Render();
            }
        }

        private static CPUDisplay _cpuDisplay;

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
            if (args[0]=="core.bin")
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

            var done = false;
            while (!done)
            {
                var key = Console.ReadKey(true).Key;
                _cpuDisplay.RenderMessage("");

                switch (key)
                {
                    case ConsoleKey.X:
                        _cpuDisplay.RenderMessage("Exiting...");
                        done = true;
                        break;

                    case ConsoleKey.C:
                        _cpuDisplay.RenderMessage("Dumping core!");
                        CoreDumper.DumpCore(cpu);
                        break;

                    case ConsoleKey.R:
                        _cpuDisplay.RenderMessage("Running to completion...");
                        cpu.ExecuteToBrk();
                        done = true;
                        break;

                    case ConsoleKey.H:
                        _displayCPU = false;
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