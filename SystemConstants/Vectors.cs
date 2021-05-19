namespace BbcMicro.SystemConstants
{
    public enum Vectors : ushort
    {
        vectorUSERV = 0x0200,  // User vector
        vectorBRKV = 0x0202,   // BRK vector
        vectorIRQ1V = 0x0204,  // Primary IRQ vector
        vectorIRQ2V = 0x0206,  // Unrecognised IRQ vector
        vectorCLIV = 0x0208,   // Command line interpreter
        vectorBYTEV = 0x020A,  // OSBYTE call
        vectorWORDV = 0x020C,  // OSWORD call
        vectorWRCHV = 0x020E,  // OSWRCH call
        vectorRDCHV = 0x0210,  // OSRDCH call
        vectorFILEV = 0x0212,  // Load / Save file
        vectorARGSV = 0x0214,  // Load / Save file parameters
        vectorBGETV = 0x0216,  // Get byte from file
        vectorBPUTV = 0x0218,  // Put byte to file
        vectorGBPBV = 0x021A,  // Transfer data to or from a file
        vectorFINDV = 0x021C,  // Open / Close file
        vectorFSCV = 0x021E,   // Filing system control
        vectorEVNTV = 0x0220,  // Events
        vectorUPTV = 0x0222,   // User print
        vectorNETV = 0x0224,   // Econet
        vectorVDUV = 0x0226,   // Unrecognised PLOT / VDU 23 commands
        vectorKEYV = 0x0228,   // Keyboard
        vectorINSV = 0x022A,   // Insert character into buffer
        vectorREMV = 0x022C,   // Remove character from buffer
        vectorCNPV = 0x022E,   // Count or purge buffer
        vectorIND1V = 0x0230,  // Unused vector
        vectorIND2V = 0x0232,  // Unused vector
        vectorIND3V = 0x0234   // Unused vector
    }
}