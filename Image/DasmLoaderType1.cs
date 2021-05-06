using BbcMicro.Memory.Abstractions;
using System.IO;
using System;
using BbcMicro.Image.Abstractions;

namespace BbcMicro.Image
{
    public sealed class DasmLoaderType1 : IImageLoader
    {
        private readonly IAddressSpace _memory;

        public DasmLoaderType1(IAddressSpace memory)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        public ImageInfo Load(string fileName)
        {
            var bytes = File.ReadAllBytes(fileName);
            var entryPoint = (ushort)(bytes[0] + (bytes[1] << 8));

            for (ushort offset = 0; offset < bytes.Length - 2; offset++)
            {
                _memory.SetByte(bytes[offset + 2], (ushort)(entryPoint + offset));
            }

            return new ImageInfo { EntryPoint = entryPoint };
        }
    }
}