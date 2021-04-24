using BbcMicro.Cpu;
using BbcMicro.Cpu.Memory;
using BbcMicro.Memory.Abstractions;
using BbcMicro.OS.Image;
using Xunit;
using Xunit.Abstractions;

namespace CpuTests
{
    public class DASMTestBase
    {
        protected readonly ITestOutputHelper _stdOut;

        protected readonly CPU _cpu;
        protected readonly IAddressSpace _addressSpace;

        public DASMTestBase(ITestOutputHelper stdOut)
        {
            _stdOut = stdOut;
            _addressSpace = new FlatAddressSpace();
            _cpu = new CPU(_addressSpace);

            var os = new BbcMicro.OS.OperatingSystem();
            _cpu.AddInterceptionCallback(os.InterceptorDispatcher.Dispatch);
        }

        protected void Load(string fileName)
        {
            var imageLoader = new DasmLoaderType2(_addressSpace);
            var imageInfo= imageLoader.Load(fileName);
            _cpu.PC = imageInfo.EntryPoint;
        }

        protected void Run()
        {
            _cpu.ExecuteToBrk();
        }
    }
}
