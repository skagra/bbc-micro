namespace BbcMicro.SystemConstants
{
    public static class CPU
    {
        public const ushort RESET_VECTOR = 0xFFFC;

        public const ushort STACK_HIMEM = 0X01FF;
        public const ushort STACK_LOMEM = 0X0100;

        public const byte S_MIN = 0x00;
        public const byte S_MAX = 0XFF;

        public const ushort IRQ_VECTOR = 0xFFFE;
    }
}