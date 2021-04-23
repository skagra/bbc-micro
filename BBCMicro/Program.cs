using System;
using BbcMicro.Cpu;
using BbcMicro.Cpu.Diagnostics;
using BbcMicro.Cpu.Memory;
using BbcMicro.OS.Image;

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
            var imageLoader = new DasmLoaderType1();
            var imageInfo = imageLoader.Load(args[0], addressSpace);

            // Single step mode
            _cpuDisplay = new CPUDisplay(cpu);
            cpu.AddPreExecutionCallback(DisplayCallback);

            // Execute the loaded image
            cpu.PC = imageInfo.EntryPoint;
            cpu.ExecuteNextInstruction();
            var done = false;
            while (!done)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.X:
                        Console.WriteLine("Exiting");
                        done = true;
                        break;

                    case ConsoleKey.C:
                        Console.WriteLine("Dumping core");
                        CoreDumper.DumpCore(cpu);
                        break;

                    case ConsoleKey.R:
                        cpu.ExecuteToBrk();
                        done = true;
                        break;

                    case ConsoleKey.H:
                        _displayCPU = false;
                        Console.Clear();
                        break;

                    case ConsoleKey.S:
                        _displayCPU = true;
                        Console.Clear();
                        break;

                    default:
                        cpu.ExecuteNextInstruction();
                        break;
                }
            }
        }
    }
}