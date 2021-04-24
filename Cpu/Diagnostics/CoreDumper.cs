using System.IO;

namespace BbcMicro.Cpu.Diagnostics
{
    public sealed class CoreDumper
    {
        private static readonly string MEMORY_CORE_FILENAME = "core.bin";

        public static void WriteWord(ushort word, BinaryWriter writer)
        {
            writer.Write((byte)word);
            writer.Write((byte)(word >> 8));
        }

        public static void DumpCore(CPU cpu)
        {
            using (var writer = new BinaryWriter(new FileStream(MEMORY_CORE_FILENAME, FileMode.Create)))
            {
                WriteWord(cpu.PC, writer);
                writer.Write(cpu.S);
                writer.Write(cpu.A);
                writer.Write(cpu.X);
                writer.Write(cpu.Y);
                writer.Write(cpu.P);

                for (ushort address = 0; address < 0xFFFF; address++)
                {
                    writer.Write(cpu.Memory.GetByte(address));
                }
            }
        }
    }
}