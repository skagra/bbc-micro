using System;

namespace BbcMicro.Memory.Abstractions
{
    public interface IAddressSpace
    {
        void SetByte(byte value, ushort address);

        byte GetByte(ushort address);

        void Flush();

        void AddSetByteCallback(Action<byte, byte, ushort> callback);

        void RemoveSetByteCallback(Action<byte, byte, ushort> callback);

        string ToString(ushort start, ushort length);
    }
}