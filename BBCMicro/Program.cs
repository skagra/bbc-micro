﻿using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.Memory.Extensions;
using BbcMicro.OS;
using BbcMicro.WPF;
using NLog;
using BbcMicro.Image;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BbcMicro.Screen;
using BbcMicro.WPFDebugger;
using CommandLine;
using System.Runtime.InteropServices;

namespace BBCMicro
{
    internal class Program
    {
        public class Options
        {
            [Option('d', "debug", Required = false, HelpText = "Run in the debugger.")]
            public bool Debug { get; set; }

            [Option('o', "os", Required = false, HelpText = "OS ROM.")]
            public string OsRom { get; set; }

            [Option('l', "lang", Required = false, HelpText = "Language ROM.")]
            public string LangRom { get; set; }

            [Option('2', "dasm2", Required = false, HelpText = "DASM type 2 file.")]
            public string DasmType2 { get; set; }

            [Option('c', "core", Required = false, HelpText = "Core file.")]
            public string CoreFile { get; set; }
        }

        private const string OS_ROM_DIR = "/Development/bbc-micro-roms/Os/";
        private const string DEFAULT_OS_ROM = "Os-1.2.ROM";

        private const string LANG_ROM_DIR = "/Development/bbc-micro-roms/Lang/";
        private const string DEFAULT_LANG_ROM = "BASIC2.rom";

        [STAThread]
        private static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                // Create the debugger window
                var debuggerDisplay = new DebuggerDisplay();

                var result = Parser.Default.ParseArguments<Options>(args);

                var langRom = DEFAULT_LANG_ROM;
                var osRom = DEFAULT_OS_ROM;

                //if (args.Length > 0)
                //{
                //    langRom = args[0];
                //    if (args.Length == 2)
                //    {
                //        osRom = args[1];
                //    }
                //}

                debuggerDisplay.AddMessage("BBC Microcomputer Emulator", true);

                // Create the emulated RAM
                debuggerDisplay.AddMessage("Creating address space");
                var addressSpace = new FlatAddressSpace();

                // Create the emulated CPU
                debuggerDisplay.AddMessage("Creating CPU");
                var cpu = new CPU(addressSpace);

                // Load images for OS and Basic
                var loader = new ROMLoader();
                debuggerDisplay.AddMessage($"Loading OS ROM from '{osRom}'");
                loader.Load(Path.Combine(OS_ROM_DIR, osRom), 0xC000, addressSpace);
                debuggerDisplay.AddMessage($"Loading language ROM from '{langRom}'");
                loader.Load(Path.Combine(LANG_ROM_DIR, langRom), 0x8000, addressSpace);

                // Create the keyboard emulation for WPF
                debuggerDisplay.AddMessage("Creating keyboard");
                var keyboardEmu = new WPFKeyboardEmu();

                // Install the operating system settings and traps
                debuggerDisplay.AddMessage("Installing OS traps");
                var os = new BbcMicro.OS.OperatingSystem(addressSpace, OSMode.WPF, keyboardEmu);
                cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

                // Create the screen emulator for WPF
                debuggerDisplay.AddMessage("Creating screen");
                var screen = new GenericScreen(addressSpace);

                // Create the WPF application
                var app = new Application();

                // For debugging IRQ
                var timerInterrupt = new TimerInterrupt(cpu);

                // Grab key events and send through to the buffer
                screen.GetWindow().KeyDown += new KeyEventHandler((sender, keyEventArgs) =>
                {
                    if (keyEventArgs.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        if (Clipboard.ContainsText())
                        {
                            keyboardEmu.PushToBuffer(Clipboard.GetText());
                        }
                    }
                    else
                    if (keyEventArgs.Key == Key.D && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        debuggerDisplay.Show();
                    }
                    else
                    if (keyEventArgs.Key == Key.I && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        timerInterrupt.TriggerInterrupt();
                    }
                    else
                    {
                        keyboardEmu.PushToBuffer(new WPFKeyDetails
                        {
                            CapsLock = Keyboard.IsKeyToggled(Key.CapsLock),
                            Key = keyEventArgs.Key,
                            Modifiers = Keyboard.Modifiers
                        });
                    }
                    keyEventArgs.Handled = true;
                });

                // Start scanning screen memory and drawing the emulated screen
                debuggerDisplay.AddMessage("Starting screen scanning");
                Task.Run(() =>
                {
                    // A frig to ensure we've booted before we start scanning the screen
                    Thread.Sleep(500);
                    screen.StartScan();
                });

                // Point the CPU at the reset vector
                debuggerDisplay.AddMessage("Starting the CPU");

                cpu.PC = addressSpace.GetNativeWord((ushort)BbcMicro.SystemConstants.CPU.resetVector);

                // Start the CPU

                // Initial timer frig
                // https://tobylobster.github.io/mos/mos/S-s11.html#SP16
                //Task.Run(() =>
                //{
                //    var timer = new BbcMicro.Timers.Timer(addressSpace);
                //    while (true)
                //    {
                //        timer.Tick();
                //        // Clock resolution is 10ms
                //        Thread.Sleep(5);
                //    }
                //});

                if (!o.Debug)
                {
                    Task.Run(() =>
                    {
                        cpu.Execute();
                    });
                }
                else
                {
                    debuggerDisplay.Show();
                    // Create the debugger
                    var debugger = new Debugger(debuggerDisplay, cpu);
                }

                // Start the WPF application
                try
                {
                    app.Run();
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }).WithNotParsed<Options>((errs) =>
            {
                String.Join(" ", errs);
            });
        }
    }
}