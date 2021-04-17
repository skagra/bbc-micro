using System;
using System.Collections.Generic;
using System.Text;

namespace BbcMicro.Cpu
{
    public interface IAddressSpace
    {
        void Set(byte value, ushort address);

        byte GetByte(ushort address);

        ushort GetWord(ushort address);
    }
}