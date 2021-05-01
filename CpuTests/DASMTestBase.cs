using BbcMicro.OS.Image;
using Xunit.Abstractions;

namespace BbcMicro.CpuTests
{
    public abstract class DASMTestBase : CPUTestBase
    {
        public DASMTestBase(ITestOutputHelper stdOut) : base(stdOut)
        {
        }

        protected void Load(string fileName)
        {
            var imageLoader = new DasmLoaderType2(_addressSpace);
            var imageInfo = imageLoader.Load(fileName);
            _cpu.PC = imageInfo.EntryPoint;
        }

        protected void Run()
        {
            _cpu.Execute();
        }
    }
}