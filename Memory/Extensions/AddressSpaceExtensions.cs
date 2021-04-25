using BbcMicro.Memory.Abstractions;
using System.Text;

namespace BbcMicro.Memory.Extensions
{
    public static class AddressSpaceExtensions
    {
        public static void SetString(this IAddressSpace addressSpace, string value, ushort address)
        {
            var asciiBytes = Encoding.ASCII.GetBytes(value.ToCharArray());
            for (var offset = 0; offset < asciiBytes.Length; offset++)
            {
                addressSpace.SetByte(asciiBytes[offset], (ushort)(address + offset));
            }
        }

        public static void SetPascalString(this IAddressSpace addressSpace, string value, ushort address)
        {
            addressSpace.SetByte((byte)value.Length, address);
            addressSpace.SetString(value, (ushort)(address + 1));
        }

        public static byte[] GetByteArray(this IAddressSpace addressSpace, byte length, ushort address)
        {
            var result = new byte[length];
            for (byte offset = 0; offset < length; offset++)
            {
                result[offset] = addressSpace.GetByte((ushort)(address + offset));
            }
            return result;
        }

        public static string GetPascalString(this IAddressSpace addressSpace, ushort address)
        {
            byte length = addressSpace.GetByte(address);
            var res = Encoding.ASCII.GetString(addressSpace.GetByteArray(length, (ushort)(address + 1)));
            return Encoding.ASCII.GetString(addressSpace.GetByteArray(length, (ushort)(address + 1)));
        }
    }
}
