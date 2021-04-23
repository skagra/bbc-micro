using BbcMicro.Cpu.Memory.Abstractions;

namespace BbcMicro.Cpu.Image.Abstractions
{
    public interface IImageLoader
    {
        ImageInfo Load(string fileName, IAddressSpace memory);
    }
}