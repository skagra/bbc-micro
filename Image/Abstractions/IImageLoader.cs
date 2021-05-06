namespace BbcMicro.OS.Image.Abstractions
{
    public interface IImageLoader
    {
        ImageInfo Load(string fileName);
    }
}