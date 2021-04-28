using Xunit;
using Xunit.Abstractions;
using static BbcMicro.Cpu.CPU;

namespace BbcMicro.CpuTests
{
    public class DASMShifts : DASMTestBase
    {
        public DASMShifts(ITestOutputHelper stdOut) : base(stdOut)
        {
        }

        [Theory]
        [InlineData(0b1001_1011, false,
            0b0011_0110, true, 0b0100_1101, true,
            0b0011_0110, true, 0b0100_1101, true)]
        [InlineData(0b0001_1011, true,
            0b0011_0110, false, 0b0000_1101, true,
            0b0011_0111, false, 0b1000_1101, true)]
        public void Shifts(byte inByte, bool cIn,
            byte aslOut, bool aslCOut, byte lsrOut, bool lsrCOut,
            byte rolOut, bool rolCOut, byte rorOut, bool rorCOut)
        {
            Load("Shifts.out");

            _cpu.A = inByte;
            _cpu.PSet(PFlags.C, cIn);

            Run();

            var asl = GetByte(0);
            var aslC = (GetByte(1) & 0b0000_0001) != 0;

            var lsr = GetByte(2);
            var lsrC = (GetByte(3) & 0b0000_0001) != 0;

            var rol = GetByte(4);
            var rolC = (GetByte(5) & 0b0000_0001) != 0;

            var ror = GetByte(6);
            var rorC = (GetByte(7) & 0b0000_0001) != 0;

            Assert.Equal(aslOut, asl);
            Assert.Equal(aslCOut, aslC);

            Assert.Equal(lsrOut, lsr);
            Assert.Equal(lsrCOut, lsrC);

            Assert.Equal(rolOut, rol);
            Assert.Equal(rolCOut, rolC);

            Assert.Equal(rorOut, ror);
            Assert.Equal(rorCOut, rorC);
        }
    }
}