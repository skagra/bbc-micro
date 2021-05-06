using BbcMicro.Image.Abstractions;
using BbcMicro.Memory.Abstractions;
using System;
using System.IO;

namespace BbcMicro.Image
{
    public sealed class DasmLoaderType2 : IImageLoader
    {
        private readonly IAddressSpace _memory;

        public DasmLoaderType2(IAddressSpace memory)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        private void LoadSegment(ushort segmentOrigin, ushort segmentLength,
            ushort imageBase, byte[] image, IAddressSpace memory)
        {
            for (ushort index = 0; index < segmentLength; index++)
            {
                memory.SetByte(image[imageBase + index], (ushort)(segmentOrigin + index));
            }
        }

        public ImageInfo Load(string fileName)
        {
            var bytes = File.ReadAllBytes(fileName);
            ushort offset = 0;

            while (offset < bytes.Length)
            {
                var segmentOrigin = (ushort)(bytes[offset] + (bytes[offset + 1] << 8));
                var segmentLength = (ushort)(bytes[offset + 2] + (bytes[offset + 3] << 8));

                offset += (ushort)4;

                LoadSegment(segmentOrigin, segmentLength, offset, bytes, _memory);

                offset += (ushort)segmentLength;
            }

            return new ImageInfo { EntryPoint = (ushort)(bytes[0] + (bytes[1] << 8)) };
        }
    }
}