﻿using BbcMicro.Cpu.Image.Abstractions;
using BbcMicro.Cpu.Memory.Abstractions;
using System.IO;

namespace BbcMicro.Cpu.Image
{
    public sealed class DasmLoaderType1 : IImageLoader
    {
        public ImageInfo Load(string fileName, IAddressSpace memory)
        {
            var bytes = File.ReadAllBytes(fileName);
            var entryPoint = (ushort)(bytes[0] + (bytes[1] >> 8));

            for (ushort offset = 0; offset < bytes.Length - 2; offset++)
            {
                memory.SetByte(bytes[offset + 2], (ushort)(entryPoint + offset));
            }

            return new ImageInfo { EntryPoint = entryPoint };
        }
    }
}