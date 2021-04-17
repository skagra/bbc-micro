namespace BbcMicro.Cpu
{
    public sealed class FlatAddressSpace : IAddressSpace
    {
        private readonly byte[] _memory = new byte[0x10000];

        public byte GetByte(ushort address)
        {
            return _memory[address];
        }

        public void Set(byte value, ushort address)
        {
            _memory[address] = value;
        }

        public ushort GetWord(ushort address)
        {
            return (ushort)(_memory[address] << 8 + _memory[address + 1]);
        }
    }
}