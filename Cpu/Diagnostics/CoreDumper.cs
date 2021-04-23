using BbcMicro.Cpu.Memory.Abstractions;
using System.IO;

namespace BbcMicro.Cpu.Diagnostics
{
    public sealed class CoreDumper
    {
        private static readonly string MEMORY_CORE_FILENAME = "core.bin";
        private static readonly string CPU_STATE_FILENAME = "cpu.txt";

        public static void DumpMemory(IAddressSpace memory)
        {
            using (var writer = new BinaryWriter(new FileStream(MEMORY_CORE_FILENAME, FileMode.Create)))
            {
                for (ushort address = 0; address < 0xFFFF; address++)
                {
                    writer.Write(memory.GetByte(address));
                }
            }
        }

        public static void DumpCpuState(CPU cpu)
        {
            using (var stream = new StreamWriter(new FileStream(CPU_STATE_FILENAME, FileMode.Create)))
            {
                stream.Write(cpu.ToString());
            }
        }

        public static void DumpCore(CPU cpu)
        {
            DumpCpuState(cpu);
            DumpMemory(cpu.Memory);
        }
    }
}