using BbcMicro.ConsoleWindowing;
using BbcMicro.Cpu;
using BbcMicro.Diagnostics;
using BbcMicro.Memory;
using BbcMicro.Memory.Extensions;
using OS.Image;
using Screen;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

            var infoViewpoint = new Viewport(0, 42, 30, 20, ConsoleColor.DarkGray, ConsoleColor.Black);
            infoViewpoint.Write("BBC Microcomputer Emulator").NewLine().NewLine();

            // Create CPU and address space
            infoViewpoint.Write("Creating address space...");
            var addressSpace = new FlatAddressSpace();
            infoViewpoint.Write("done.").NewLine();
            infoViewpoint.Write("Creating CPU...");
            var cpu = new CPU(addressSpace);
            infoViewpoint.Write("done.").NewLine();

            // Load images for OS and Basic
            var loader = new ROMLoader();

            infoViewpoint.Write($"Loading OS from '{osRom}'...");
            loader.Load(Path.Combine(OS_ROM_DIR, osRom), 0xC000, addressSpace);
            infoViewpoint.Write("done.").NewLine();

            infoViewpoint.Write($"Loading language from '{langRom}'...");
            loader.Load(Path.Combine(LANG_ROM_DIR, langRom), 0x8000, addressSpace);
            infoViewpoint.Write("done.").NewLine();

            // Set up the OS
            infoViewpoint.Write("Installing OS traps...");
            var os = new OS.OperatingSystem(addressSpace, null, false);
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);
            infoViewpoint.Write("done.").NewLine();

            infoViewpoint.Write("Creating screen...");
            var screen = new Mode7Screen(addressSpace, 100, 0, 0);

            infoViewpoint.Write("done.").NewLine();

            Task.Run(() =>
            {
                // A frig to ensure we've booted before we start
                // scanning the screen
                Thread.Sleep(500);
                infoViewpoint.Write("Starting screen refresh.").NewLine();
                screen.StartScan();
            });

            // Start the CPU
            infoViewpoint.Write($"Handing control to emulator.").NewLine(); ;
            cpu.PC = addressSpace.GetNativeWord(0xFFFC);

            //var mon = new MemoryMonitor(addressSpace);
            //mon.AddRange(0x3000, 0x8000, "Screen");

            // Run OS
            cpu.Execute();
        }
    }
}