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
    }
}

