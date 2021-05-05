﻿using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.Memory.Extensions;
using BbcMicro.OS;
using BbcMicro.WPF;
using NLog;
using OS.Image;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BbcMicroMode0
{
    internal class Program
    {
        private const string OS_ROM_DIR = "/Development/bbc-micro-roms/Os/";
        private const string DEFAULT_OS_ROM = "Os-1.2.ROM";

        private const string LANG_ROM_DIR = "/Development/bbc-micro-roms/Lang/";
        private const string DEFAULT_LANG_ROM = "BASIC2.rom";

        [STAThread]
        private static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

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

            // Create the emulated RAM
            var addressSpace = new FlatAddressSpace();

            // Create the emulated CPU
            var cpu = new CPU(addressSpace);

            // Load images for OS and Basic
            var loader = new ROMLoader();

            loader.Load(Path.Combine(OS_ROM_DIR, osRom), 0xC000, addressSpace);
            loader.Load(Path.Combine(LANG_ROM_DIR, langRom), 0x8000, addressSpace);

            // Create the keyboard emulation for WPF
            var keyboardEmu = new WPFKeyboardEmu();

            // Install the operating system settings and traps
            var os = new BbcMicro.OS.OperatingSystem(addressSpace, OSMode.WPF, keyboardEmu);
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

            // Create the screen emuator for WPF
            var screen = new Mode0Screen(addressSpace);

            // Create the WPF application
            var app = new Application();

            // Grab key events and send through to the buffer
            screen.GetWindow().KeyDown += new KeyEventHandler((sender, keyEventArgs) =>
            {
                keyboardEmu.PushToBuffer(new WPFKeyDetails
                {
                    CapsLock = Keyboard.IsKeyToggled(Key.CapsLock),
                    Key = keyEventArgs.Key,
                    Modifiers = Keyboard.Modifiers
                });

                keyEventArgs.Handled = true;
            });

            // Start scanning screen memory and drawing the emulated screen
            screen.StartScan();

            // Point the CPU at the reset vector
            cpu.PC = addressSpace.GetNativeWord(0xFFFC);

            // Start the CPU
            Task.Run(() =>
            {
                cpu.Execute();
            });

            // Start the WPF application
            try
            {
                app.Run();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}