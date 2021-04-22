namespace BbcMicro.Cpu
{
    public enum AddressingMode : ushort
    {
        Accumulator = 0,        // Operand is stored in the accumulator
        Immediate = 1,          // Operand is stored in next byte
        Implied = 2,            // Implied by the op code
        Relative = 3,           // Operand is stored in the next byte is a signed value used to relative jumps
        Absolute = 4,           // Operand is obtained by referencing the two byte address in the next two bytes
        ZeroPage = 5,           // Operand is stored in the zero page location indicated by the next byte
        Indirect = 6,           // The two byte address of the operand is obtained by dereferencing the address in the next two bytes (used by JMP only)
        AbsoluteIndexedX = 7,   // The two byte address of the operand is obtained by adding the address in the next two bytes to the X register
        AbsoluteIndexedY = 8,   // The two byte address of the operand is obtained by adding the address in the next two bytes to the Y register
        ZeroPageIndexedX = 9,   // The zero page one byte address of the operand is obtained by adding the address in the next byte to the X register, wraps to zero page
        ZeroPageIndexedY = 10,  // The zero page one byte address of the operand is obtained by adding the address in the next byte to the Y register, wraps to zero page
        IndexedXIndirect = 11,  // The next byte is added to X register (zero page wrapped) and is dereferenced to obtain the address of the operand
        IndirectIndexedY = 12   // The next byte is dereferenced to an the address which is added to the X register obtain the address of the operand
    }
}
