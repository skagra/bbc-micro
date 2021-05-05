using BbcMicro.Cpu;
using BbcMicro.Memory;
using BbcMicro.Memory.Extensions;
using Keyboard;
using NLog;
using OS.Image;
using Screen;
using System;
using System.IO;
using System.Threading;
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

        private static bool IsKeyOfInterest(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) ||
                ((key >= Key.A) && (key <= Key.Z)) ||
                key == Key.Space ||
                key == Key.Back ||
                key == Key.Return ||
                key == Key.Divide ||
                key == Key.OemMinus ||
                key == Key.OemPlus ||
                key == Key.OemComma ||
                key == Key.OemPeriod ||
                key == Key.OemQuestion;
        }

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

            var addressSpace = new FlatAddressSpace();

            var cpu = new CPU(addressSpace);

            // Load images for OS and Basic
            var loader = new ROMLoader();

            loader.Load(Path.Combine(OS_ROM_DIR, osRom), 0xC000, addressSpace);

            loader.Load(Path.Combine(LANG_ROM_DIR, langRom), 0x8000, addressSpace);

            var keyboardEmu = new KeyboardEmu();

            var os = new BbcMicro.OS.OperatingSystem(addressSpace, keyboardEmu, false);
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

            var screen = new Mode0Screen(addressSpace);
            var app = new Application();

            // Need to add a structure to hold modifiers!
            screen.GetWindow().KeyDown += new KeyEventHandler((sender, b) =>
            {
                if (IsKeyOfInterest(b.Key))
                {
                    logger.Debug(b.Key);

                    keyboardEmu.PushToBuffer(b.Key);
                }
                b.Handled = true;
            });

            screen.StartScan();

            cpu.PC = addressSpace.GetNativeWord(0xFFFC);

            Task.Run(() =>
            {
                cpu.Execute();
            });

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