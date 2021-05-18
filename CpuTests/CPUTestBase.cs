using BbcMicro.Cpu;
using BbcMicro.Memory.Abstractions;
using Xunit.Abstractions;
using BbcMicro.Memory.Extensions;
using BbcMicro.Memory;

namespace BbcMicro.CpuTests
{
    public abstract class CPUTestBase
    {
        protected readonly ITestOutputHelper _stdOut;

        protected readonly CPU _cpu;
        protected readonly IAddressSpace _addressSpace;

        public CPUTestBase(ITestOutputHelper stdOut)
        {
            _stdOut = stdOut;
            _addressSpace = new FlatAddressSpace();
            _cpu = new CPU(_addressSpace);
        }

        protected byte GetByte(ushort address)
        {
            return _addressSpace.GetByte(address);
        }

        protected void SetByte(byte value, ushort address)
        {
            _addressSpace.SetByte(value, address);
        }

        protected void SetString(string value, ushort address)
        {
            _addressSpace.SetString(value, address);
        }

        protected void SetPascalString(string value, ushort address)
        {
            _addressSpace.SetPascalString(value, address);
        }

        protected byte[] GetByteArray(byte length, ushort address)
        {
            return _addressSpace.GetByteArray(length, address);
        }

        protected string GetPascalString(ushort address)
        {
            return _addressSpace.GetPascalString(address);
        }

        protected void DumpCore()
        {
            CoreDumper.DumpCore(_cpu);
        }
    }
}