using System;
using BbcMicro.Cpu;

namespace BbcMicro
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cpu = new CPU();
            var addressSpace = new FlatAddressSpace();

            byte a;

            a = 20;

            short b = (short)(a - 121);
            Console.WriteLine(b);

            Console.WriteLine(b & 0b10000000);

            Console.WriteLine(cpu);
        }
    }
}