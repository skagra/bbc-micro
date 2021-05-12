namespace BbcMicro.SystemConstants
{
    public enum VIA : ushort
    {
        systemVIARegisterB = 0xFE40,
        systemVIADataDirectionRegisterA = 0xFE43,
        systemVIAInterruptFlagRegister = 0xFE4D,
        systemViaInterruptEnableRegister = 0xFE4E,
        systemVIARegisterANoHandshake = 0xFE4F
    }
}