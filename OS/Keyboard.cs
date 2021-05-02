using System;
using System.IO;
using System.Text;
using BbcMicro.Cpu;

namespace BbcMicro.OS
{
    public sealed class Keyboard
    {
        // https://tobylobster.github.io/mos/mos/S-s2.html#SP2
        public static bool OSRDCH(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
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
                cpu.A = 27;
                // TODO: Make escape work properly
                // cpu.Memory.SetByte(0x80, 0x00FF);
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

        public static bool INTERROGATE_KEYBOARD(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var handled = false;
            if (cpu.X >= 1 && cpu.X <= 9)
            {
                // TODO: This seems backwards to me!
                handled = true;

                if (cpu.X >= 1 && cpu.X <= 3)
                {
                    cpu.X = (byte)(0X80 | cpu.X);
                }
                else
                {
                    cpu.X = 0;
                }
                using (var f = new StreamWriter("/temp/log.txt", true))
                {
                    f.WriteLine($"{cpu.Memory.GetByte(0x028E):X2}");
                }
            }
            return handled;
        }
    }
}