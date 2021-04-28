using BbcMicro.Memory.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BbcMicro.Memory
{
    public sealed class FlatAddressSpace : IAddressSpace
    {
        private readonly byte[] _memory = new byte[0x10000];

        private readonly List<Action<byte, byte, ushort>> _setByteCallbacks =
            new List<Action<byte, byte, ushort>>();

        public byte GetByte(ushort address)
        {
            return _memory[address];
        }

        public void SetByte(byte value, ushort address)
        {
            _setByteCallbacks.ForEach(callback => callback(value, _memory[address], address));
            _memory[address] = value;
        }

        public void AddSetByteCallback(Action<byte, byte, ushort> callback)
        {
            _setByteCallbacks.Add(callback);
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