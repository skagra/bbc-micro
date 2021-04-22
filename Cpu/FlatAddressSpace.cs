using System;
using System.Text;

namespace BbcMicro.Cpu
{
    public sealed class FlatAddressSpace : IAddressSpace
    {
        private readonly byte[] _memory = new byte[0x10000];

        public byte GetByte(ushort address)
        {
            return _memory[address];
        }

        public void SetByte(byte value, ushort address)
        {
            _memory[address] = value;
        }

        // The little endian values in memory are converted a C# ushort representation
        public ushort GetWord(ushort address)
        {
            return (ushort)(_memory[address] + (_memory[address + 1] << 8));
        }

        public void Flush()
        {
            Array.Clear(_memory, 0, _memory.Length);
        }

        public string ToString(ushort start, ushort length)
        {
            var result = new StringBuilder();

            var address = start;
            var count = 0;

            while (address < _memory.Length && count < length)
            {
                result.Append($"0x{address:X4} 0x{_memory[address]:X2}\n");
                count++;
            }

            while (count < length)
            {
                result.Append($"------ ----");
            }

            return result.ToString();
        }
    }
}