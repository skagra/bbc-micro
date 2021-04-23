using BbcMicro.Cpu;
using System;

namespace BbcMicro.OS
{
    // Sends the char in the al register to the VDU driver.
    public sealed class TextOutput
    {
        public static bool OSWRCH(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            Console.Write($"'{(char)cpu.A}'");
            return true;
        }
    }
}