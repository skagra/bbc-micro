using Xunit;
using Xunit.Abstractions;

namespace BbcMicro.CpuTests
{
    public class DASMNibble : DASMTestBase
    {
        public DASMNibble(ITestOutputHelper stdOut) : base(stdOut)
        {
        }

        [Theory]
        [InlineData(0xAD)]
        [InlineData(0x21)]
        [InlineData(0xF2)]
        public void UnsignedAddition(byte inByte)
        {
            Load("Nibble.out");

            _cpu.A = inByte;

            Run();

            Assert.Equal((byte)((inByte << 4) | (inByte >> 4)), _cpu.A);
        }
    }
}