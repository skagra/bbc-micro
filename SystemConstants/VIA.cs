namespace BbcMicro.SystemConstants
{
    public enum VIA : ushort
    {
        systemVIARegisterB = 0xFE40,
        systemVIARegisterA = 0xFE41,
        systemVIADataDirectionRegisterB = 0xFE42,
        systemVIADataDirectionRegisterA = 0xFE43,
        systemVIATimer1CounterLow = 0xFE44,
        systemVIATimer1CounterHigh = 0xFE45,
        systemVIATimer1LatchLow = 0xFE46,
        systemVIATimer1LatchHigh = 0xFE47,
        systemVIATimer2CounterLow = 0xFE48,
        systemVIATimer2CounterHigh = 0xFE49,
        systemVIAShiftRegister = 0xFE4A,
        systemVIAAuxiliaryControlRegister = 0xFE4B,
        systemVIAPeripheralControlRegister = 0xFE4C,
        systemVIAInterruptFlagRegister = 0xFE4D,
        systemViaInterruptEnableRegister = 0xFE4E,
        systemVIARegisterANoHandshake = 0xFE4F
    }
}