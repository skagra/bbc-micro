using BbcMicro.Memory.Abstractions;

namespace BbcMicro.OS.Image.Abstractions
{
    public interface IImageLoader
    {
        ImageInfo Load(string fileName, IAddressSpace memory);
    }
}