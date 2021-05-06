using BbcMicro.Memory.Abstractions;
using BbcMicro.OS.Image.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OS.Image
{
    public sealed class ROMLoader 
    {
        public void Load(string fileName, ushort baseAddress, IAddressSpace addressSpace)
        {
            var bytes = File.ReadAllBytes(fileName);
            for (ushort offset=0; offset<bytes.Length; offset++) {
                addressSpace.SetByte(bytes[offset], (ushort)(baseAddress + offset));
            }
        }
    }
}
