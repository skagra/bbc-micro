using System;
using System.IO;
using System.Text;
using BbcMicro.Cpu;
using Keyboard;
using System.Windows.Input;

namespace BbcMicro.OS
{
    public sealed class Keyboard
    {
        private KeyboardEmu _keyboardEmu;

        public Keyboard(KeyboardEmu keyboardEmu = null)
        {
            _keyboardEmu = keyboardEmu;
        }

        public bool OSRDCH(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            if (_keyboardEmu == null)
            {
                return OSRDCH_CONSOLE(cpu, opCode, addressingMode, operand);
            }
            else
            {
                return OSRDCH_WPF(cpu, opCode, addressingMode, operand);
            }
        }

        public bool OSRDCH_WPF(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            cpu.PReset(CPU.PFlags.C);

            var key = _keyboardEmu.BlockAndReadKey();

            if (key == Key.Back)
            {
                cpu.A = 127;
            }
            else
            if (key == Key.Return)
            {
                cpu.A = 13;
            }
            else
            if (key == Key.Space)
            {
                cpu.A = 32;
            }
            else
            if (key == Key.OemComma)
            {
                cpu.A = 44;
            }
            //else
            //if (key == Key.D2 && System.Windows.Input.Keyboard.IsKeyToggled(Key.LeftShift))
            //{
            //    cpu.A = 34;
            //}
            else
            if (key >= Key.D0 && key <= Key.D9)
            {
                cpu.A = (byte)key.ToString().ToCharArray()[1];
            }
            else
            if (key >= Key.A && key <= Key.Z)
            {
                cpu.A = (byte)key.ToString().AsSpan()[0];
            }
            else
            {
                cpu.A = 0;
            }

            return true;
        }

        // https://tobylobster.github.io/mos/mos/S-s2.html#SP2
        public static bool OSRDCH_CONSOLE(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
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
                    for (ushort address = 0X3000; address < 0x8000; address++)
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
         */

        public bool INTERROGATE_KEYBOARD(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var handled = false;
            if (cpu.X >= 1 && cpu.X <= 9)
            {
                // TODO: This seems backwards to me!
                handled = true;

                if (cpu.X >= 1 && cpu.X <= 3)
                {
                    cpu.X = cpu.X;
                }
                else
                {
                    cpu.X = (byte)(0X80 | cpu.X);
                }

                //if (cpu.X >= 1 && cpu.X <= 3)
                //{
                //    cpu.X = (byte)(0X80 | cpu.X);
                //}
                //else
                //{
                //    cpu.X = 0;
                //}
            }
            return handled;
        }
    }
}