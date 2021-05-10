namespace BbcMicro.SystemConstants
{
    public enum CPU : ushort
    {
        stackLomem = 0x0100,
        stackHimem = 0x01FF,
        resetVector = 0xFFFC,
        irqVector = 0xFFFE
    }
}