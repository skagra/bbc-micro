using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.Memory.Extensions;
using OS.Image;
using Screen;
using System;
using System.Threading;

namespace BbcMicro
{
    internal class Program
    {
        private const string OS_ROM = "/Development/bbc-micro-roms/Os-1.2.ROM";
        private const string LANG_ROM = "/Development/bbc-micro-roms/BASIC1.rom";

        private static void Main(string[] args)
        {
            Console.Clear();

            // Create CPU and address space
            Console.Write("Creating address space...");
            var addressSpace = new FlatAddressSpace();
            Console.WriteLine("done.");
            Console.Write("Creating CPU emulator...");
            var cpu = new CPU(addressSpace);
            Console.WriteLine("done.");

            // Set up the OS
            Console.Write("Setting up OS routine interception...");
            var os = new OS.OperatingSystem();
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);
            Console.WriteLine("done.");

            // Load images for OS and Basic
            var loader = new ROMLoader();

            Console.Write($"Loading operating system from '{OS_ROM}'...");
            loader.Load(OS_ROM, 0xC000, addressSpace);
            Console.WriteLine("done.");

            Console.Write($"Loading BASIC from '{LANG_ROM}'");
            loader.Load(LANG_ROM, 0x8000, addressSpace);
            Console.WriteLine("done.");

            // Start the CPU
            Console.WriteLine($"Starting emulation.");
            cpu.PC = addressSpace.GetNativeWord(0xFFFC);

            // Sleep to give a moment to review messages
            Thread.Sleep(2000);

            Console.Write("Creating screen emulation...");
            var screen = new Mode7Screen(addressSpace, 100, 2, 0);
            Console.WriteLine("done.");

            Console.Clear();
            Console.WriteLine("BBC Microcomputer Emulator");

            // Start screen drawing thread
            screen.StartScan();

            // Run OS
            cpu.Execute();
        }
    }
}