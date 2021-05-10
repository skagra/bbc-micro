namespace BbcMicro.SystemConstants
{
    public enum IRQ : ushort
    {
        irqEntryPoint = 0xdc1c,
        brkRoutine = 0xdc27,
        rs423Handler = 0xdc68,
        rs423SetRequestToSendInactive = 0xdc78,
        rs423BufferSpaceOK = 0xdc7a,
        readFromRS423 = 0xdc7d,
        irq1Handler = 0xdc93,
        irq1CheckACIA = 0xdca2,
        rs423ErrorDetectedSetAY = 0xdcb3,
        writeToACIA = 0xdcbf,
        rs423HasControl = 0xdcde,
        unrecognisedInterrupt = 0xdcf3,
        irq1CheckSystemVIA = 0xdd06,
        irq1CheckUserVIA = 0xdd47,
        irq1CheckSystemVIASpeech = 0xdd69,
        irq1CheckSystemVIA100HzTimer = 0xddca,
        irq1CheckSystemVIAADCEndConversion = 0xde47,
        irq1CheckSystemVIAKeyboard = 0xde72,
        restoreRegistersAndReturnFromInterrupt = 0xde82,
        eventEntryPoint = 0xe494,
        irq2Handler = 0xde89
    }
}