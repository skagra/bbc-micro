using BbcMicro.Memory.Abstractions;
using System.IO;

namespace BbcMicro.Image
{
    public sealed class ROMLoader
    {
        public void Load(string fileName, ushort baseAddress, IAddressSpace addressSpace)
        {
            var bytes = File.ReadAllBytes(fileName);
            for (ushort offset = 0; offset < bytes.Length; offset++)
            {
                addressSpace.SetByte(bytes[offset], (ushort)(baseAddress + offset));
            }
        }
    }
}