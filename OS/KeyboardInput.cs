using System;
using System.IO;
using System.Text;
using BbcMicro.Cpu;
using System.Windows.Input;
using BbcMicro.WPF;
using System.Windows;
using NLog;

namespace BbcMicro.OS
{
    public sealed class KeyboardInput
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private WPFKeyboardEmu _wpfKeyboardEmu;

        public KeyboardInput(WPFKeyboardEmu wpfKeyboardEmu = null)
        {
            _wpfKeyboardEmu = wpfKeyboardEmu;
        }

        public bool OSRDCH_WPF(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            cpu.PReset(CPU.PFlags.C);

            var keyDetails = _wpfKeyboardEmu.BlockAndReadKey();
            var key = keyDetails.Key;
            var modifiers = keyDetails.Modifiers;

            // Numerics
            if (key >= Key.D0 && key <= Key.D9)
            {
                if (!(modifiers.HasFlag(ModifierKeys.Shift)))
                {
                    cpu.A = (byte)('0' + (key - Key.D0));
                }
                else
                {
                    switch (key)
                    {
                        case Key.D1:
                            cpu.A = (byte)'!';
                            break;

                        case Key.D2:
                            cpu.A = (byte)'"';
                            break;

                        case Key.D3:
                            cpu.A = (byte)96;
                            break;

                        case Key.D4:
                            cpu.A = (byte)'$';
                            break;

                        case Key.D5:
                            cpu.A = (byte)'%';
                            break;

                        case Key.D6:
                            cpu.A = (byte)'^';
                            break;

                        case Key.D7:
                            cpu.A = (byte)'&';
                            break;

                        case Key.D8:
                            cpu.A = (byte)'*';
                            break;

                        case Key.D9:
                            cpu.A = (byte)'(';
                            break;

                        case Key.D0:
                            cpu.A = (byte)')';
                            break;
                    }
                }
            }
            // Alphabetics
            if (key >= Key.A && key <= Key.Z)
            {
                if (!(modifiers.HasFlag(ModifierKeys.Shift) ^ keyDetails.CapsLock))
                {
                    cpu.A = (byte)('a' + (key - Key.A));
                }
                else
                {
                    cpu.A = (byte)('A' + (key - Key.A));
                }
            }
            else
            if (key == Key.Space)
            {
                cpu.A = 32;
            }
            else
            if (key == Key.Back)
            {
                cpu.A = 127;
            }
            else
            if (key == Key.Return)
            {
                cpu.A = 13;
            }
            if (key == Key.OemPeriod)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'>';
                }
                else
                {
                    cpu.A = (byte)'.';
                }
            }
            else
            if (key == Key.OemComma)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'<';
                }
                else
                {
                    cpu.A = (byte)',';
                }
            }
            if (key == Key.OemPlus)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'+';
                }
                else
                {
                    cpu.A = (byte)'=';
                }
            }
            else
            if (key == Key.OemMinus)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'_';
                }
                else
                {
                    cpu.A = (byte)'-';
                }
            }
            else
            if (key == Key.OemQuestion)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'?';
                }
                else
                {
                    cpu.A = (byte)'/';
                }
            }
            else
            if (key == Key.Oem1)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)':';
                }
                else
                {
                    cpu.A = (byte)';';
                }
            }
            else
            if (key == Key.Oem3)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'@';
                }
                else
                {
                    cpu.A = (byte)'\'';
                }
            }
            else
            if (key == Key.OemOpenBrackets)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'{';
                }
                else
                {
                    cpu.A = (byte)'[';
                }
            }
            else
            if (key == Key.Oem6)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'}';
                }
                else
                {
                    cpu.A = (byte)']';
                }
            }
            else
            if (key == Key.OemQuotes)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    cpu.A = (byte)'~';
                }
                else
                {
                    cpu.A = (byte)'#';
                }
            }
            return true;
        }

        // https://tobylobster.github.io/mos/mos/S-s2.html#SP2
        public bool OSRDCH_CONSOLE(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            cpu.PReset(CPU.PFlags.C);

            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                cpu.A = 127;
            }
            else
            if (keyInfo.Key == ConsoleKey.G && ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0))
            {
                Console.Beep();
                cpu.A = 7;
            }
            else
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                // TODO: Make escape work properly
                // I suspect I'll need to emulator the keyboard at a lower level
                // in order that games function.
                cpu.A = 0x1B;
                cpu.PSet(CPU.PFlags.C);
                cpu.Memory.SetByte(0xFF, 0x00FF);
            }
            else
            if (keyInfo.Key == ConsoleKey.D && ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0))
            {
                Console.Beep();
                // Dump display memory
                using (var writer = new BinaryWriter(new FileStream("/temp/display.bin", FileMode.Create)))
                {
                    for (ushort address = 0X3000; address < 0x3100; address++)
                    {
                        writer.Write(cpu.Memory.GetByte(address));
                    }
                }
            }
            else
            {
                cpu.A = Encoding.ASCII.GetBytes(new char[] { keyInfo.KeyChar })[0];
            }
            return true;
        }

        /*
         * This interceptor is supply the values of the keyboard dip switches used at boot time
         *
         * Returns X = $80 + internal key number if key pressed (or zero otherwise)
         *
         * 8-6   bit 0-2        9,8,7           Together these bits determine the startup MODE
         * 8     bit 3            6             Set if the SHIFT-BREAK action is reversed with BREAK
         * 4-3   bit 4-5         5,4            Sets disc drive timings (depends on make of drive)
         * 2-1   bit 6-7         3,2            unused
         *
         * This is called at boot time https://tobylobster.github.io/mos/mos/S-s10.html#SP6
         *
         * Bit directions are inverted - at least for the screen mode
         */

        public bool INTERROGATE_KEYBOARD_CONSOLE(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var handled = false;
            if (cpu.X >= 1 && cpu.X <= 9)
            {
                handled = true;
                cpu.X = (byte)0;
            }
            return handled;
        }

        public bool INTERROGATE_KEYBOARD_WFP(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var handled = false;
            if (cpu.X >= 1 && cpu.X <= 9)
            {
                handled = true;

                if (cpu.X >= 7 && cpu.X <= 9)
                {
                    cpu.X = (byte)(0X80 | cpu.X);
                }
                else
                {
                    cpu.X = (byte)0;
                }
            }
            return handled;
        }
    }
}