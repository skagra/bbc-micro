using BbcMicro.Memory.Abstractions;
using System;
using System.Text;

namespace BbcMicro.Cpu.Memory
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

        public void Flush()
        {
            Array.Clear(_memory, 0, _memory.Length);
        }

        public string ToString(ushort start, ushort length)
        {
            var result = new StringBuilder();

            var address = start;
            var count = 0;

            while (address + count < _memory.Length && count < length)
            {
                var currentAddr = address + count;
                result.Append($"0x{currentAddr:X4} 0x{_memory[currentAddr]:X2}\n");
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