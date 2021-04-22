using System;
using BbcMicro.Cpu;

namespace BbcMicro
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create CPU and address space
            var addressSpace = new FlatAddressSpace();
            var cpu = new CPU(addressSpace);

            // Load programme from file

            // Set PC to start of programme

            // Run the programme
            cpu.ExecuteToBrk();
        }
    }
}