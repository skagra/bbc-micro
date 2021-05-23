using System;

namespace BbcMicro.Memory.Abstractions
{
    public interface IAddressSpace
    {
        void SetByte(byte value, ushort address, bool ignoreCallbacks = false);

        byte GetByte(ushort address, bool ignoreCallbacks = false);

        void Flush();

        void AddGetByteCallback(Func<ushort, byte?> callback);

        void AddSetByteCallback(Action<byte, byte, ushort> callback);

        void RemoveSetByteCallback(Action<byte, byte, ushort> callback);

        string ToString(ushort start, ushort length);
    }
}