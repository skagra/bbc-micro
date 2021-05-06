using BbcMicro.Cpu;
using BbcMicro.OS.Image.Abstractions;
using System;
using System.IO;

namespace BbcMicro.OS.Image
{
    public sealed class CoreFileLoader : IImageLoader
    {
        private readonly CPU _cpu;

        public CoreFileLoader(CPU cpu)
        {
            _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        }

        public ImageInfo Load(string fileName)
        {
            var bytes = File.ReadAllBytes(fileName);

            _cpu.PC = (ushort)(bytes[0] + (bytes[1] << 8));
            _cpu.S = bytes[2];
            _cpu.A = bytes[3];
            _cpu.X = bytes[4];
            _cpu.Y = bytes[5];
            _cpu.P = bytes[6];

            for (ushort offset = 0; offset < bytes.Length - 7; offset++)
            {
                _cpu.Memory.SetByte(bytes[offset + 7], (ushort)(offset));
            }

            return new ImageInfo { EntryPoint = _cpu.PC };
        }
    }
}