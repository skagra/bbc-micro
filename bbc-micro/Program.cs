using System;
using BbcMicro.Cpu;

namespace BbcMicro
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ushort PC = 10;

            byte operand = 0b1000_0010;

            Console.WriteLine(PC + operand);
            Console.WriteLine(PC + (sbyte)operand);


        }
    }
}