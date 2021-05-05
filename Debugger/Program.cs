using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.OS;
using BbcMicro.OS.Image;
using BbcMicro.OS.Image.Abstractions;
using System;

namespace BbcMicro.Debugger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create CPU and address space
            var addressSpace = new FlatAddressSpace();
            var cpu = new CPU(addressSpace);

            // Set up the OS
            var os = new OS.OperatingSystem(addressSpace, OSMode.Debug);
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

            // Set the entry point address the loaded image
            cpu.PC = imageInfo.EntryPoint;

            // Single step mode
            var debugger = new Debugger(cpu);

            Console.SetCursorPosition(0, 36);

            debugger.Run();
        }
    }
}