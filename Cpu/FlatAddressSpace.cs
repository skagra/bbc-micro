using System;

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
            return (ushort)(_memory[address] + _memory[address + 1] << 8);
        }

        public void Flush()
        {
            Array.Clear(_memory, 0, _memory.Length);
        }
    }
}