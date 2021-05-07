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
using BbcMicro.Diagnostics;

namespace BBCMicro
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
            var screen = new GenericScreen(addressSpace);

            // Create the WPF application
            var app = new Application();

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
            Task.Run(() =>
            {
                // A frig to ensure we've booted before we start
                // scanning the screen
                Thread.Sleep(500);
                screen.StartScan();
            });

            // Point the CPU at the reset vector
            cpu.PC = addressSpace.GetNativeWord(0xFFFC);

            // Start the CPU
            Task.Run(() =>
            {
                cpu.Execute();
            });

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

            var cpuMon = new CpuAddressMonitor(cpu);
            cpuMon.AddMonitor(".irqEntryPoint", 0xdc1c);
            cpuMon.AddMonitor(".irq1Handler", 0xdc93);
            cpuMon.AddMonitor(".irq1CheckACIA", 0xdca2);
            cpuMon.AddMonitor(".irq1CheckSystemVIA", 0xdd06);
            cpuMon.AddMonitor(".irq1CheckUserVIA", 0xdd47);
            cpuMon.AddMonitor(".irq1CheckSystemVIA100HzTimer", 0xddca);
            cpuMon.AddMonitor(".eventEntryPoint ", 0xe494);
            cpuMon.AddMonitor(".irq1CheckSystemVIAADCEndConversion", 0xde47);
            cpuMon.AddMonitor(".irq1CheckSystemVIASpeech", 0xdd69);

            const ushort timeClockSwitch = 0X0283;
            const ushort timeClockA = 0X0292;
            const ushort timeClockB = 0X0297;
            const ushort systemVIAIRQBitMask = 0X0297;

            // https://tobylobster.github.io/mos/mos/S-s3.html#SP19;
            const ushort systemVIAInterruptFlagRegister = 0XFE4D;
            const ushort acia6850StatusRegister = 0XFE08;

            var memMon = new MemoryMonitor(addressSpace);
            memMon.AddRange(timeClockSwitch, timeClockSwitch, "timeClockSwitch");
            memMon.AddRange(timeClockA, timeClockA + 4, "timeClockA");
            memMon.AddRange(timeClockB, timeClockB + 4, "timeClockB");
            memMon.AddRange(systemVIAInterruptFlagRegister, systemVIAInterruptFlagRegister, "systemVIAInterruptFlagRegister");

            // Timer code .irq1CheckSystemVIA100HzTimer
            Task.Run(() =>
            {
                Thread.Sleep(500);

                addressSpace.SetByte(10, timeClockSwitch);
                while (true)
                {
                    cpu.TriggerIRQ(() =>
                    {
                        addressSpace.SetByte(0xFF, systemVIAIRQBitMask);

                        // ACIA did not generate the interrupt
                        addressSpace.SetByte(0b0000_0000, acia6850StatusRegister);

                        // 100Hz timer interrupt 0b1100_0000
                        addressSpace.SetByte(0b1100_0000, systemVIAInterruptFlagRegister);
                    });

                    Thread.Sleep(100);
                }
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