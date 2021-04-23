using System;
using BbcMicro.Cpu;
using BbcMicro.Cpu.Diagnostics;
using BbcMicro.Cpu.Image;
using BbcMicro.Cpu.Memory;

namespace BbcMicro
{
    internal class Program
    {
        private static void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            _cpuDisplay.Render();
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

            _cpuDisplay = new CPUDisplay(cpu);
            cpu.AddPreExecutionCallback(DisplayCallback);

            // Execute the loaded image
            cpu.PC = imageInfo.EntryPoint;
            while (true)
            {
                cpu.ExecuteNextInstruction();
                Console.ReadKey(true);
            }
            //cpu.ExecuteToBrk();
        }
    }
}