namespace BbcMicro.SystemConstants
{
    public static class CPU
    {
        public const ushort RESET_VECTOR = 0xFFFC;

        public const ushort STACK_HIMEM = 0x01FF;
        public const ushort STACK_LOMEM = 0x0100;

        public const byte S_MIN = 0x00;
        public const byte S_MAX = 0xFF;

        public const ushort IRQ_VECTOR = 0xFFFE;
    }
}