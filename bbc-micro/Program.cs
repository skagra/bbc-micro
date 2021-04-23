using System;
using System.IO;
using BbcMicro.Cpu;

namespace BbcMicro
{
    internal class Program
    {
        private static bool PrintCharCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var runImpl = true;
            if (opCode == OpCode.JSR)
            {
                if (operand == 0xFFFE)
                {
                    Console.Write($"Output: '{(char)cpu.A}'");
                    runImpl = false;
                }
            }
            return runImpl;
        }

        private static Disassembler _dis = new Disassembler();

        private static void DisassembleCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            Console.WriteLine(_dis.Disassemble(opCode, addressingMode, cpu.PC, cpu));
        }

        private static void DisplayCallback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            _cpuDisplay.Render();
        }

        private static CPUDisplay _cpuDisplay;

        private static void Main(string[] args)
        {
            // Create CPU and address space
            var addressSpace = new FlatAddressSpace();
            var cpu = new CPU(addressSpace);

            // Add traps for OS routines
            cpu.SetInterceptionCallback(PrintCharCallback);

            //  cpu.AddPreExecutionCallback(DisassembleCallback);

            _cpuDisplay = new CPUDisplay(cpu);
            cpu.AddPreExecutionCallback(DisplayCallback);

            // Read the assembled code
            var bytes = File.ReadAllBytes(args[0]);
            var entryPoint = (ushort)(bytes[0] + (bytes[1] >> 8));

            // Load the code into memory
            for (ushort offset = 0; offset < bytes.Length - 2; offset++)
            {
                addressSpace.SetByte(bytes[offset + 2], (ushort)(entryPoint + offset));
            }

            // Run the programme
            cpu.PC = entryPoint;
            var con=true;
            while (con)
            {
                con=cpu.ExecuteNextInstruction();
                Console.ReadLine();
            }
            //cpu.ExecuteToBrk();
        }
    }
}