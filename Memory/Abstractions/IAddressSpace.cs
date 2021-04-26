namespace BbcMicro.Memory.Abstractions
{
    public interface IAddressSpace
    {
        void SetByte(byte value, ushort address);

        byte GetByte(ushort address);

        void Flush();

        string ToString(ushort start, ushort length);
    }
}