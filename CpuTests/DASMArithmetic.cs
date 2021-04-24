using BbcMicro.Cpu;
using Xunit;
using Xunit.Abstractions;

namespace CpuTests
{
    public class DASMArithmetic: DASMTestBase
    {
        public DASMArithmetic(ITestOutputHelper stdOut):base(stdOut)
        {
        }

        [Theory]
        [InlineData(0b0000_1101, 0b1101_0011, true, 0b1110_0001, false)] // (2.1) 13 + 211 + carry = 225
        [InlineData(0b1111_1110, 0b0000_0110, true, 0b0000_0101, true)]  // (2.2) 254 + 6 + carry = 5 (with carry)
        public void UnsignedAddition(byte inA, byte inImmediate, bool inC, byte outA, bool outC)
        {
            Load("Add.out");

            _addressSpace.SetByte(inA, 0);
            _addressSpace.SetByte(inImmediate, 1);
            _cpu.PSet(CPU.PFlags.C, inC);

            Run();

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
            Load("Add.out");

            _addressSpace.SetByte(inA, 0);
            _addressSpace.SetByte(inImmediate, 1);
            _cpu.PSet(CPU.PFlags.C, inC);

            _cpu.ExecuteToBrk();

            Assert.Equal(outA, _cpu.A);
            Assert.Equal(outC, _cpu.PIsSet(CPU.PFlags.C));
            Assert.Equal(outV, _cpu.PIsSet(CPU.PFlags.V));
            Assert.Equal(outN, _cpu.PIsSet(CPU.PFlags.N));
            Assert.Equal(outZ, _cpu.PIsSet(CPU.PFlags.Z));
        }
    }
}
