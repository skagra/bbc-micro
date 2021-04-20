﻿namespace BbcMicro.Cpu
{
    public interface IAddressSpace
    {
        void SetByte(byte value, ushort address);

        byte GetByte(ushort address);

        ushort GetWord(ushort address);

        void Flush();
    }
}