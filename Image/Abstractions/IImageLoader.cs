namespace BbcMicro.Image.Abstractions
{
    public interface IImageLoader
    {
        ImageInfo Load(string fileName);
    }
}