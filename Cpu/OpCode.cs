namespace BbcMicro.Cpu
{
    public enum OpCode : byte
    {
        ADC = 0, AND = 1, ASL = 2, BCC = 3, BCS = 4, BEQ = 5, BIT = 6, BMI = 7,
        BNE = 8, BPL = 9, BRK = 10, BVC = 11, BVS = 12, CLC = 13, CLD = 14, CLI = 15,
        CLV = 16, CMP = 17, CPX = 18, CPY = 19, DEC = 20, DEX = 21, DEY = 22, EOR = 23,
        INC = 24, INX = 25, INY = 26, JMP = 27, JSR = 28, LDA = 29, LDX = 30, LDY = 31,
        LSR = 32, NOP = 33, ORA = 34, PHA = 35, PHP = 36, PLA = 37, PLP = 38, ROL = 39,
        ROR = 40, RTI = 41, RTS = 42, SBC = 43, SEC = 44, SED = 45, SEI = 46, STA = 47,
        STX = 48, STY = 49, TAX = 50, TAY = 51, TSX = 52, TXA = 53, TXS = 54, TYA = 55
    }
}