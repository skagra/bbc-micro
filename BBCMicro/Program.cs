using BbcMicro.Cpu;
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
using System.Collections.Generic;
using CommandLine.Text;
using BbcMicro.BbcMicro.VIA;

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

            public string DasmType2 { get; set; }

            [Option('c', "core", Required = false, HelpText = "Core file.")]
            public string CoreFile { get; set; }
        }

        private const string OS_ROM_DIR = "/Development/bbc-micro-roms/Os/";
        private const string DEFAULT_OS_ROM = "Os-1.2.ROM";

        private const string LANG_ROM_DIR = "/Development/bbc-micro-roms/Lang/";
        private const string DEFAULT_LANG_ROM = "BASIC2.rom";

        private static ParserResult<Options> _parserResult;

        [STAThread]
        private static void Main(string[] args)
        {
            _parserResult = Parser.Default.ParseArguments<Options>(args);
            _parserResult.
                WithParsed(options => Run(options)).
                WithNotParsed(errors => ShowHelpOrError(errors));
        }

        [STAThread]
        private static void Run(Options options)
        {
            var logger = LogManager.GetCurrentClassLogger();

            // Resolve options
            var coreFile = options.CoreFile;
            var osRom = options.OsRom ?? DEFAULT_OS_ROM;
            var langRom = options.LangRom ?? DEFAULT_LANG_ROM;
            var debugging = options.Debug;

            // Create the debugger window
            var debuggerDisplay = new DebuggerDisplay();

            debuggerDisplay.AddMessage("BBC Microcomputer Emulator", true);

            // Create the emulated RAM
            debuggerDisplay.AddMessage("Creating address space");
            var addressSpace = new FlatAddressSpace();

            // Create the emulated CPU
            debuggerDisplay.AddMessage("Creating CPU");
            var cpu = new CPU(addressSpace);

            // Load images
            if (coreFile != null)
            {
                // Core file
                var loader = new CoreFileLoader(cpu);
                cpu.PC = loader.Load(coreFile).EntryPoint;
            }
            else
            {
                // OS and/or Language
                var loader = new ROMLoader();

                debuggerDisplay.AddMessage($"Loading OS ROM from '{osRom}'");
                loader.Load(Path.Combine(OS_ROM_DIR, osRom), 0xC000, addressSpace);

                debuggerDisplay.AddMessage($"Loading language ROM from '{langRom}'");
                loader.Load(Path.Combine(LANG_ROM_DIR, langRom), 0x8000, addressSpace);

                cpu.PC = addressSpace.GetNativeWord((ushort)BbcMicro.SystemConstants.CPU.resetVector);
            }

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
            //var timerInterrupt = new TimerInterrupt(cpu);

            // Grab key events and send through to the buffer
            screen.AddKeyDownCallback((sender, keyEventArgs) =>
            {
                //    if (keyEventArgs.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                //    {
                //        if (Clipboard.ContainsText())
                //        {
                //            keyboardEmu.PushToBuffer(Clipboard.GetText());
                //        }
                //    }
                //    else
                if (keyEventArgs.Key == Key.D && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    debuggerDisplay.Show();
                }
                //    else
                //    if (keyEventArgs.Key == Key.I && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                //    {
                //        timerInterrupt.TriggerInterrupt();
                //    }
                //    else
                //    {
                //        keyboardEmu.PushToBuffer(new WPFKeyDetails
                //        {
                //            CapsLock = Keyboard.IsKeyToggled(Key.CapsLock),
                //            Key = keyEventArgs.Key,
                //            Modifiers = Keyboard.Modifiers
                //        });
                //    }
                //    keyEventArgs.Handled = true;
            });

            var via = new VIA(cpu);
            screen.AddKeyDownCallback(via.KeyPressCallback);
            via.StartTimers();

            // Start scanning screen memory and drawing the emulated screen
            debuggerDisplay.AddMessage("Starting screen scanning");
            Task.Run(() =>
            {
                // A frig to ensure we've booted before we start scanning the screen
                Thread.Sleep(500);
                screen.StartScan();
            });

            // Create the debugger
            var debugger = new Debugger(debuggerDisplay, cpu);

            // Point the CPU at the reset vector
            debuggerDisplay.AddMessage("Starting the CPU");

            // Start the CPU
            if (!debugging)
            {
                Task.Run(() =>
                {
                    debugger.Execute(false);
                });
            }
            else
            {
                debuggerDisplay.Show();
            }

            // Start the WPF application
            app.Run();
        }

        private static void ShowHelpOrError(IEnumerable<Error> errors)
        {
            var helpText = HelpText.AutoBuild(_parserResult, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                return h;
            });

            MessageBox.Show(helpText, "BBC Microcomputer Emulator", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}