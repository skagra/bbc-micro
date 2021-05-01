using BbcMicro.Cpu;
using System;

namespace BbcMicro.OS
{
    public sealed class TextOutput
    {
        // Sends the char in the accumulator to the VDU driver.

        public static bool OSWRCH(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            Console.Write((char)cpu.A);
            return true;
        }

        public static bool OSASCI(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            Console.Write((char)cpu.A);
            return true;
        }

        public static bool VDUCHR(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            Console.Write("VDUCHR");
            return true;
        }
    }
}