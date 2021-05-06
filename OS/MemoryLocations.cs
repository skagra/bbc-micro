namespace BbcMicro.OS
{
    public static class MemoryLocations
    {
        // https://tobylobster.github.io/mos/mos/S-s3.html#SP20
        public const ushort SYSTEM_VIA_INTERRUPT_ENABLE_REGISTER = 0xFE4E;

        // https://tobylobster.github.io/mos/mos/S-s2.html#SP15
        public const ushort VDU_CURRENT_SCREEN_MODE = 0X0355;
    }
}