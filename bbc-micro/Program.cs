using System;
using BbcMicro.Cpu;

namespace BbcMicro
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cpu = new CPU();

            cpu.PSet(CPU.PFlags.C);
            cpu.A = 15;

            var addressSpace = new FlatAddressSpace();

            Console.WriteLine(cpu);
        }
    }
}