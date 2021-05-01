using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.OS.Image;
using BbcMicro.OS.Image.Abstractions;
using OS.Image;

namespace BbcMicro
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create CPU and address space
            var addressSpace = new FlatAddressSpace();
            var cpu = new CPU(addressSpace);

            // Set up the OS
            var os = new OS.OperatingSystem();
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

            // Read the image to execute
            //IImageLoader imageLoader = null;
            //if (args[0] == "core.bin")
            //{
            //    imageLoader = new CoreFileLoader(cpu);
            //}
            //else
            //{
            //    imageLoader = new DasmLoaderType2(addressSpace);
            //}
            //var imageInfo = imageLoader.Load(args[0]);

            var loader = new ROMLoader();
            loader.Load("/Development/BBCRoms/MOS010", 0xC000, addressSpace);
            loader.Load("/Development/BBCRoms/Basic1.rom", 0x8000, addressSpace);

            cpu.PC = 0xda42;// addressSpace.GetNativeWord(0xFFFC);

            // Run the loaded image
            //cpu.PC = imageInfo.EntryPoint;
            cpu.ExecuteToBrk();
        }
    }
}