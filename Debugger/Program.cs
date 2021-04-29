using System;
using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.OS.Image;
using BbcMicro.OS.Image.Abstractions;

namespace BbcMicro.Debugger
{
    internal class Program
    {
        private static CPU _cpu;
        private static FlatAddressSpace _addressSpace;
        private static Debugger _debugger;

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

            // Set the entry point address the loaded image
            _cpu.PC = imageInfo.EntryPoint;

            // Single step mode
            _debugger = new Debugger(_cpu);

            // TODO
            Console.SetCursorPosition(0, 28);

            _debugger.Run();
        }
    }
}