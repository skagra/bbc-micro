using System;
using System.Linq;

namespace BbcMicro.Cpu
{
    public sealed class CPU
    {
        // Program counter
        public ushort PC { get; set; }

        // Stack pointer
        public byte S { get; set; }

        // Accumulator
        public byte A { get; set; }

        // Index register X
        public byte X { get; set; }

        // Index register Y
        public byte Y { get; set; }

        // Processor status
        public byte P { get; set; }

        [Flags]
        public enum PFlags : byte
        {
            C = 0x01, // Carry
            Z = 0x02, // Zero result
            I = 0x04, // Interrupt disable
            D = 0x08, // Decimal mode
            B = 0x10, // Break command
            X = 0x20, // Reserved for expansion
            V = 0x40, // Overflow
            N = 0x80  // Negative result
        }

        public bool PIsSet(PFlags flags)
        {
            return (P & (byte)flags) == (byte)flags;
        }

        public void PSet(PFlags flags)
        {
            P = (byte)(P | (byte)flags);
        }

        public void PReset(PFlags flags)
        {
            P = (byte)(P & ~(byte)flags);
        }

        public void PSet(PFlags flags, bool value)
        {
            if (value)
            {
                PSet(flags);
            }
            else
            {
                PReset(flags);
            }
        }

        public override string ToString()
        {
            return $"PC=0x{PC:X4}, S=0x{S:X2} A=0x{A:X2}, X=0x{X:X2}, Y=0x{Y:X2}, " +
                $"P=0x{P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => ((PIsSet(flag)) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})";
        }
    }
}