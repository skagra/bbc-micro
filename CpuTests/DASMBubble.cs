using Xunit;
using Xunit.Abstractions;

namespace BbcMicro.CpuTests
{
    public class DASMBubble : DASMTestBase
    {
        public DASMBubble(ITestOutputHelper stdOut) : base(stdOut)
        {
        }

        [Theory]
        [InlineData("befcda", "abcdef")]
        public void UnsignedAddition(string inString, string outString)
        {
            Load("Bubble.out");

            SetPascalString(inString, 0x500);

            Run();

            Assert.Equal(outString, GetPascalString(0x500));
        }
    }
}