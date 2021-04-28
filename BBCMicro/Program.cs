using System;
using System.Text;
using BbcMicro.Cpu;
using BbcMicro.Cpu.Diagnostics;
using BbcMicro.Memory;
using BbcMicro.OS.Image;
using BbcMicro.OS.Image.Abstractions;
using OS.Image;

namespace BbcMicro
{
    internal class Program
    {
        private static bool _displayingStatus = true;

        private static void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            if (_displayingStatus)
            {
                _cpuDisplay.Update();
            }
        }

        private static CPUDisplay _cpuDisplay;

        private static void DisplayAddress(byte newVal, byte oldVal, ushort address)
        {
            var message = new StringBuilder($"${address:X4} <= ${newVal:X2} (${oldVal:X2})");

            if (address <= 0x01FF && address >= 0x0100)
            {
                message.Append(" [stack]");
            }
            RenderMessage(message.ToString());
        }

        private static void RenderMessage(string message)
        {
            if (_displayingStatus)
            {
                _cpuDisplay.WriteMessage(message);
            }
        }

        private static void Main(string[] args)
        {
            // Create CPU and address space
            var addressSpace = new FlatAddressSpace();
            var cpu = new CPU(addressSpace);

            // Set up the OS
            var os = new OS.OperatingSystem();
            cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);

            // Read the image to execute
            IImageLoader imageLoader = null;
            if (args[0] == "core.bin")
            {
                imageLoader = new CoreFileLoader(cpu);
            }
            else
            {
                imageLoader = new DasmLoaderType2(addressSpace);
            }
            var imageInfo = imageLoader.Load(args[0]);

            //Execute the loaded image
            cpu.PC = imageInfo.EntryPoint;

            //var loader = new ROMLoader();
            //loader.Load("/Development/BBCRoms/OS-1.2.rom", 0xC000, addressSpace);
            //cpu.PC = 0xda42;// addressSpace.GetNativeWord(0xFFFC);

            // Single step mode
            _cpuDisplay = new CPUDisplay(cpu);
            cpu.AddPostExecutionCallback(DisplayCallback);

            addressSpace.AddSetByteCallback((newVal, oldVal, address) =>
                DisplayAddress(newVal, oldVal, address));

            Console.SetCursorPosition(0, 15);

            var done = false;
            while (!done)
            {
                var key = Console.ReadKey(true).Key;
                _cpuDisplay.ClearMessage();

                switch (key)
                {
                    case ConsoleKey.X:
                        RenderMessage("Exiting...");
                        done = true;
                        break;

                    case ConsoleKey.C:
                        RenderMessage("Dumping core!");
                        CoreDumper.DumpCore(cpu);
                        break;

                    case ConsoleKey.R:
                        RenderMessage("Running to completion...");
                        cpu.ExecuteToBrk();
                        done = true;
                        break;

                    case ConsoleKey.H:
                        _displayingStatus = false;
                        Console.Clear();
                        break;

                    default:
                        cpu.ExecuteNextInstruction();
                        break;
                }
            }
            Console.WriteLine();
        }
    }
}