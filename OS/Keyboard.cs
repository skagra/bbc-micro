using System;
using System.Text;
using BbcMicro.Cpu;

namespace BbcMicro.OS
{
    public sealed class Keyboard
    {
        public static bool OSRDCH(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var key = Console.ReadKey(true).KeyChar;
            cpu.A = Encoding.ASCII.GetBytes(new char[] { key })[0];
            return true;
        }

        /*
         * return X = $80 + internal key number) if key pressed (or zero otherwise)
         * 8-6   bit 0-2        9,8,7           Together these bits determine the startup MODE
         * 8     bit 3            6             Set if the SHIFT-BREAK action is reversed with BREAK
         * 4-3   bit 4-5         5,4            Sets disc drive timings (depends on make of drive)
         * 2-1   bit 6-7         3,2            unused
         */

        public static bool INTERROGATE_KEYBOARD(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var handled = false;
            if (cpu.X >= 1 && cpu.X <= 9)
            {
                Console.CursorLeft = 50;
                Console.Write($"Checking key {cpu.X:X2}");
                handled = true;
                if (cpu.X >= 7 && cpu.X <= 9)
                {
                    cpu.X = (byte)(0X80 | cpu.X);
                }
                else
                {
                    cpu.X = 0;
                }
                Console.WriteLine($"Returning {cpu.X:X2}");
            }
            return handled;
        }
    }
}