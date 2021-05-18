using System;

namespace BbcMicro.SystemConstants
{
    public sealed class Symbols
    {
        private readonly string[] _symbols = new string[0xFFFF];

        public Symbols()
        {
            LoadSymbols();
        }

        public string GetSymbol(ushort address)
        {
            return _symbols[address];
        }

        private void LoadSymbols<T>() where T : Enum
        {
            var enumValues = typeof(T).GetEnumValues();

            foreach (var enumValue in enumValues)
            {
                _symbols[(ushort)enumValue] = enumValue.ToString();
            }
        }

        private void LoadSymbols()
        {
            LoadSymbols<VDU>();
            LoadSymbols<VIA>();
            LoadSymbols<IRQ>();
            LoadSymbols<SystemConstants.CPU>();
        }
    }
}