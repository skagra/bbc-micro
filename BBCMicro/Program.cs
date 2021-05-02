using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.Memory.Extensions;
using OS.Image;
using Screen;
using System;
using System.IO;
using System.Threading;

namespace BbcMicro
{
    internal class Program
    {
        private const string OS_ROM_DIR = "/Development/bbc-micro-roms/Os/";
        private const string DEFAULT_OS_ROM = "Os-1.2.ROM";

        private const string LANG_ROM_DIR = "/Development/bbc-micro-roms/Lang/";
        private const string DEFAULT_LANG_ROM = "BASIC2.rom";

        private static void Main(string[] args)
        {
            Console.Clear();

            var langRom = DEFAULT_LANG_ROM;
            var osRom = DEFAULT_OS_ROM;

            if (args.Length > 0)
            {
                langRom = args[0];
                if (args.Length == 2)
                {
                    osRom = args[1];
                }
            }

            // Create CPU and address space
            Console.Write("Creating address space...");
            var addressSpace = new FlatAddressSpace();
            Console.WriteLine("done.");
            Console.Write("Creating CPU emulator...");
            var cpu = new CPU(addressSpace);
            Console.WriteLine("done.");

            // Load images for OS and Basic
            var loader = new ROMLoader();

            Console.Write($"Loading operating system from '{osRom}'...");
            loader.Load(Path.Combine(OS_ROM_DIR, osRom), 0xC000, addressSpace);
            Console.WriteLine("done.");

            Console.Write($"Loading BASIC from '{langRom}'");
            loader.Load(Path.Combine(LANG_ROM_DIR, langRom), 0x8000, addressSpace);
            Console.WriteLine("done.");

            // Set up the OS
            Console.Write("Setting up OS routine interception...");
            var os = new OS.OperatingSystem(addressSpace);
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);
            Console.WriteLine("done.");

            // Start the CPU
            Console.WriteLine($"Starting emulation.");
            cpu.PC = addressSpace.GetNativeWord(0xFFFC);

            Thread.Sleep(2000);

            Console.Write("Creating screen emulation...");
            var screen = new Mode7Screen(addressSpace, 100, 0, 0);
            Console.WriteLine("done.");

            Console.Clear();
            screen.StartScan();

            // Run OS
            cpu.Execute();
        }
    }
}