using BbcMicro.Cpu;
using BbcMicro.Cpu.Memory;
using BbcMicro.Memory.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace CpuTests
{
    /*
     * Section numbers refer to MCS6500 Family Programming Manual
     * http://archive.6502.org/books/mcs6500_family_programming_manual.pdf
     */

    public class Arithmetic
    {
        private readonly ITestOutputHelper _stdOut;

        private readonly CPU _cpu;
        private readonly IAddressSpace _addressSpace;

        private const byte LDA_IMMEDIATE = 0xA9;
        private const byte ADC_IMMEDIATE = 0x69;
        private const byte SBC_IMMEDIATE = 0xE9;

        private ushort _memPtr = 0;

        private void SetMem(byte value)
        {
            _addressSpace.SetByte(value, _memPtr);
            _memPtr++;
        }

        public Arithmetic(ITestOutputHelper stdOut)
        {
            _stdOut = stdOut;
            _addressSpace = new FlatAddressSpace();
            _cpu = new CPU(_addressSpace);
        }

        [Theory]
        [InlineData(0b0000_1101, 0b1101_0011, true, 0b1110_0001, false)] // (2.1) 13 + 211 + carry = 225
        [InlineData(0b1111_1110, 0b0000_0110, true, 0b0000_0101, true)]  // (2.2) 254 + 6 + carry = 5 (with carry)
        public void UnsignedAddition(byte inA, byte inImmediate, bool inC, byte outA, bool outC)
        {
            _cpu.PSet(CPU.PFlags.C, inC);

            SetMem(LDA_IMMEDIATE);
            SetMem(inA);
            SetMem(ADC_IMMEDIATE);
            SetMem(inImmediate);

            _cpu.ExecuteToBrk();

            Assert.Equal(outA, _cpu.A);
            Assert.Equal(outC, _cpu.PIsSet(CPU.PFlags.C));
        }

        [Theory]
        [InlineData(0b0000_0101, 0b0000_0111, false, 0b0000_1100, false, false, false, false)] // (2.6)  5 + 7 = 12
        [InlineData(0b0111_1111, 0b0000_0010, false, 0b1000_0001, false, true, true, false)]   // (2.7)  127 + 2 = "-127" (with overflow)
        [InlineData(0b0000_0101, 0b1111_1101, false, 0b0000_0010, true, false, false, false)]  // (2.8)  5 + -3 = 2
        [InlineData(0b0000_0101, 0b1111_1001, false, 0b1111_1110, false, false, true, false)]  // (2.9)  5 + -7 = -2
        [InlineData(0b1111_1011, 0b1111_1001, false, 0b1111_0100, true, false, true, false)]   // (2.10) -5 + -7 = -12
        [InlineData(0b1011_1110, 0b1011_1111, false, 0b0111_1101, true, true, false, false)]   // (2.11) -66 + -65 = "125" (with overflow)
        public void SignedAddition(byte inA, byte inImmediate, bool inC, byte outA, bool outC, bool outV, bool outN, bool outZ)
        {
            _cpu.PSet(CPU.PFlags.C, inC);

            SetMem(LDA_IMMEDIATE);
            SetMem((byte)inA);
            SetMem(ADC_IMMEDIATE);
            SetMem((byte)inImmediate);

            _cpu.ExecuteToBrk();

            Assert.Equal(outA, _cpu.A);
            Assert.Equal(outC, _cpu.PIsSet(CPU.PFlags.C));
            Assert.Equal(outV, _cpu.PIsSet(CPU.PFlags.V));
            Assert.Equal(outN, _cpu.PIsSet(CPU.PFlags.N));
            Assert.Equal(outZ, _cpu.PIsSet(CPU.PFlags.Z));
        }

        [Theory]
        [InlineData(0b0000_0101, 0b0000_0011, true, 0b0000_0010, true)] // (2.13) 5 - 3 = 2   (carry set as no borrow)
        [InlineData(0b0000_0101, 0b0000_0110, true, 0b1111_1111, false)] // (2.14) 5 - 6 = -1 (carry not set as borrow?)
        public void UnsignedSubtraction(byte inA, byte inImmediate, bool inC, byte outA, bool outC)
        {

            SetMem(LDA_IMMEDIATE);
            SetMem(inA);
            SetMem(SBC_IMMEDIATE);
            SetMem(inImmediate);

            _cpu.PSet(CPU.PFlags.C, inC);
            _cpu.ExecuteToBrk();

            Assert.Equal(outA, _cpu.A);
            Assert.Equal(outC, _cpu.PIsSet(CPU.PFlags.C));
        }
    }
}