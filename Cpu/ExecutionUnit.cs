using System;
using System.Collections.Generic;
using static BbcMicro.Cpu.CPU;

namespace BbcMicro.Cpu
{
    public sealed class ExecutionUnit
    {
        private readonly CPU _cpu;
        private readonly IAddressSpace _memory;

        public ExecutionUnit(CPU cpu, IAddressSpace memory)
        {
            _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        private ushort GeEffectiveAddress(ushort operandAddress, AddressingMode addressingMode)
        {
            return addressingMode switch
            {
                AddressingMode.Immediate => operandAddress,
                AddressingMode.Absolute => _memory.GetWord(operandAddress),
                AddressingMode.AbsoluteIndexedByX => (ushort)(_memory.GetWord(operandAddress) + _cpu.X),
                AddressingMode.AbsoluteIndexedByY => (ushort)(_memory.GetWord(operandAddress) + _cpu.Y),
                AddressingMode.ZeroPage => _memory.GetByte(operandAddress),
                AddressingMode.ZeroPageIndexedByX => (ushort)(_memory.GetByte(operandAddress) + _cpu.X),
                AddressingMode.ZeroPageIndexedByY => (ushort)(_memory.GetByte(operandAddress) + _cpu.Y),
                AddressingMode.ZeroPageIndexedByXThenIndirect => _memory.GetWord((ushort)(_memory.GetByte(operandAddress) + _cpu.X)),
                AddressingMode.ZeroPageIndirectThenIndexedByY => (ushort)(_memory.GetByte(_memory.GetByte(operandAddress)) + _cpu.Y),
                _ => throw new Exception($"Invalid addressing mode '{addressingMode}'")
            };
        }

        public void UpdateFlags(byte accordingToValue, PFlags flagsToUpdate)
        {
            if (flagsToUpdate.HasFlag(PFlags.Z))
            {
                _cpu.PSet(PFlags.Z, accordingToValue == 0);
            }

            if (flagsToUpdate.HasFlag(PFlags.N))
            {
                _cpu.PSet(PFlags.N, (accordingToValue & 0b10000000) != 0);
            }
        }

        private const byte FULL_OPCODE_BRK = 0x0;

        /*
         * Group 0 op codes
         */
	    private const byte OPCODE_BIT = 0b001;
	    private const byte OPCODE_JMP = 0b010;
	    private const byte OPCODE_JMP_ABS = 0b011;
	    private const byte OPCODE_STY = 0b100;
	    private const byte OPCODE_LDY = 0b101;
	    private const byte OPCODE_CPY = 0b110;
	    private const byte OPCODE_CPX = 0b111;

        /*
         * Group 1 op codes
         */
        private const byte OPCODE_ORA = 0b000;
        private const byte OPCODE_AND = 0b001;
        private const byte OPCODE_EOR = 0b010;
        private const byte OPCODE_ADC = 0b011;
        private const byte OPCODE_STA = 0b100;
        private const byte OPCODE_LDA = 0b101;
        private const byte OPCODE_CMP = 0b110;
        private const byte OPCODE_SBC = 0b111;

        /*
         * Group 2 op codes
         */
        private const byte OPCODE_ASL = 0b000;
        private const byte OPCODE_ROL = 0b001;
        private const byte OPCODE_LSR = 0b010;
        private const byte OPCODE_ROR = 0b011;
        private const byte OPCODE_STX = 0b100;
        private const byte OPCODE_LDX = 0b101;
        private const byte OPCODE_DEC = 0b110;
        private const byte OPCODE_INC = 0b111;

        private void ExecuteNextGroupZeroInstruction(byte opCode, byte rawAddressingMode)
        {
            // Operand follows immediately after the op code
            var operandAddress = (ushort)(_cpu.PC + 1);

            // Decode the addressing mode
            var addressingMode = _groupZeroAddressModeMap[rawAddressingMode];  // TODO: Exception on invalid mode

            // Grab the address indicated by the operand and addressing mode
            ushort calculatedAddress = 0;
            if (opCode != OPCODE_JMP && opCode != OPCODE_JMP_ABS)
            {
                GeEffectiveAddress(operandAddress, addressingMode);
            }
            
            byte operand = 0;
            if (opCode != OPCODE_STY)
            {
                operand = _memory.GetByte(calculatedAddress);
            }

            // TODO: Some instructions support only a subset of addressing modes
            switch (opCode)
            {
                // BIT sets the Z flag as though the value in the address tested were ANDed with the accumulator.
                // The N and V flags are set to match bits 7 and 6 respectively in the value stored at the tested address.
                case OPCODE_BIT:
                    _cpu.PSet(PFlags.Z, (_cpu.A & operand) == 0);
                    _cpu.PSet(PFlags.N, (operand & 0b1000_0000) != 0);
                    _cpu.PSet(PFlags.V, (operand & 0b0100_0000) != 0);
                    break;
                case OPCODE_JMP:
                    _cpu.PC = _memory.GetWord(_memory.GetWord(operandAddress));
                    break;
                case OPCODE_JMP_ABS:
                    _cpu.PC = _memory.GetWord(operandAddress);
                    break;
                case OPCODE_STY:
                    _memory.SetByte(_cpu.Y, calculatedAddress);
                    break;
                case OPCODE_LDY:
                    _cpu.Y = operand;
                    UpdateFlags(operand, PFlags.N | PFlags.Z);
                    break;
                // ComPare Y register
                case OPCODE_CPY:
                    var valueToCompareYTo = _memory.GetByte(calculatedAddress);
                    _cpu.PSet(PFlags.N, _cpu.Y < valueToCompareYTo);
                    _cpu.PSet(PFlags.C, _cpu.Y >= valueToCompareYTo);
                    _cpu.PSet(PFlags.Z, _cpu.Y == valueToCompareYTo);
                    break;
                // ComPare X register
                case OPCODE_CPX:
                    var valueToCompareXTo = _memory.GetByte(calculatedAddress);
                    _cpu.PSet(PFlags.N, _cpu.Y < valueToCompareXTo);
                    _cpu.PSet(PFlags.C, _cpu.Y >= valueToCompareXTo);
                    _cpu.PSet(PFlags.Z, _cpu.Y == valueToCompareXTo);
                    break;
            }

            // Update program counter
            _cpu.PC += (ushort)(operandAddress + +_addressingModeToPCDelta[addressingMode]);
        }

        private void ExecuteNextGroupOneInstruction(byte opCode, byte rawAddressingMode)
        {
            // Operand follows immediately after the op code
            var operandAddress = (ushort)(_cpu.PC + 1);

            // Decode the addressing mode
            var addressingMode = _groupOneAddressModeMap[rawAddressingMode];  // TODO: Exception on invalid mode

            // Grab the address indicated by the operand and addressing mode
            var calculatedAddress = GeEffectiveAddress(operandAddress, addressingMode);

            switch (opCode)
            {
                case OPCODE_ORA: // Binary "or"
                    _cpu.A |= _memory.GetByte(calculatedAddress);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                case OPCODE_AND: // Binary "and"
                    _cpu.A &= _memory.GetByte(calculatedAddress);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                case OPCODE_EOR: // Binary "exclusive or"
                    _cpu.A ^= _memory.GetByte(calculatedAddress);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                // TODO: BCD
                case OPCODE_ADC: // Add with carry
                    byte adcOperandByte = _memory.GetByte(calculatedAddress);
                    short addWithCarryResult = (short)(_cpu.A + adcOperandByte + (_cpu.PIsSet(PFlags.C) ? 1 : 0));
                    _cpu.PSet(PFlags.C, addWithCarryResult > 0xFF); // Set carry if we have results did not fit in 8 bits
                    _cpu.PSet(PFlags.V, ((_cpu.A ^ addWithCarryResult) & (adcOperandByte ^ addWithCarryResult) & 0x80) != 0);
                    _cpu.A = (byte)addWithCarryResult;
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                case OPCODE_STA: // Store accumulator in memory
                    _memory.SetByte(_cpu.A, calculatedAddress);
                    break;

                case OPCODE_LDA: // Load accumulator from memory
                    _cpu.A = _memory.GetByte(calculatedAddress);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                case OPCODE_CMP: // Compare value to accumulator // TODO!
                    var valueToCompareTo = _memory.GetByte(calculatedAddress);
                    _cpu.PSet(PFlags.N, _cpu.A < valueToCompareTo);
                    _cpu.PSet(PFlags.C, _cpu.A >= valueToCompareTo);
                    _cpu.PSet(PFlags.Z, _cpu.A == valueToCompareTo);
                    break;

                // TODO: BCD
                case OPCODE_SBC: // Subtract with carry
                    byte sbcOperandByte = _memory.GetByte(calculatedAddress);
                    short subtractWithCarryResult = (short)(_cpu.A - _memory.GetByte(calculatedAddress) - (_cpu.PIsSet(PFlags.C) ? 0 : 1));
                    _cpu.PSet(PFlags.C, subtractWithCarryResult >= 0);
                    _cpu.PSet(PFlags.V, ((_cpu.A ^ subtractWithCarryResult) & (sbcOperandByte ^ subtractWithCarryResult) & 0x80) != 0);
                    _cpu.A = (byte)subtractWithCarryResult;
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;
            }

            // Update program counter
            _cpu.PC += (ushort)(operandAddress + _addressingModeToPCDelta[addressingMode]);
        }

        private void ExecuteNextGroupTwoInstruction(byte opCode, byte rawAddressingMode)
        {
            // Operand follows immediately after the op code
            var operandAddress = (ushort)(_cpu.PC + 1);

            // Decode the addressing mode
            var addressingMode = _groupTwoAddressModeMap[rawAddressingMode];  // TODO: Exception on invalid mode

            // If we are processing LDX and STX the we index by Y instead of the general decoding to index by X 
            if (opCode==OPCODE_LDX || opCode == OPCODE_STX)
            {
                if (addressingMode==AddressingMode.ZeroPageIndexedByX)
                {
                    addressingMode = AddressingMode.ZeroPageIndexedByY;
                }
                else if (addressingMode == AddressingMode.AbsoluteIndexedByX)
                {
                    addressingMode = AddressingMode.AbsoluteIndexedByY;
                }
            }

            // Grab the address indicated by the operand and addressing mode
            ushort calculatedAddress = 0;
            if (addressingMode != AddressingMode.Accumulator)
            {
                calculatedAddress = GeEffectiveAddress(operandAddress, addressingMode);
            }

            byte operand = 0;

            // Not storing into memory
            if (opCode != OPCODE_STX)
            {
                if (addressingMode == AddressingMode.Accumulator)
                {
                    operand = _cpu.A;
                }
                else
                {
                    operand = _memory.GetByte(calculatedAddress);
                }
            }

            // TODO: Some instructions support only a subset of addressing modes
            switch (opCode)
            {
                // ASL shifts all bits left one position. 0 is shifted into bit 0 and the original bit 7 is shifted into the Carry.
                case OPCODE_ASL:
                    _cpu.A = (byte)(operand << 1);
                    _cpu.PSet(PFlags.C, (operand & 0b1000_0000) != 0);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                // ROL shifts all bits left one position. The Carry is shifted into bit 0 and the original bit 7 is shifted into the Carry.
                case OPCODE_ROL:
                    _cpu.A = (byte)((byte)(operand << 1) | (byte)(_cpu.PIsSet(PFlags.C) ? 0b0000_0001 : 0b0000_0000));
                    _cpu.PSet(PFlags.C, (operand & 0b1000_0000) != 0);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                // LSR shifts all bits right one position. 0 is shifted into bit 7 and the original bit 0 is shifted into the Carry.
                case OPCODE_LSR:
                    _cpu.A = (byte)(operand >> 1);
                    _cpu.PSet(PFlags.C, (operand & 00000_0001) != 0);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                // ROR shifts all bits right one position. The Carry is shifted into bit 7 and the original bit 0 is shifted into the Carry.
                case OPCODE_ROR:
                    _cpu.A = (byte)((byte)(operand >> 1) | (byte)(_cpu.PIsSet(PFlags.C) ? 0b1000_0000 : 0b0000_000));
                    _cpu.PSet(PFlags.C, (operand & 0b0000_0001) != 0);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                // Store X register BUG - zero page, X => zero page,Y and absolute, X becomes absolute, Y
                case OPCODE_STX:
                    _memory.SetByte(_cpu.X, calculatedAddress);
                    break;

                // Load X register
                case OPCODE_LDX:
                    _cpu.X = operand;
                    UpdateFlags(operand, PFlags.N | PFlags.Z);
                    break;

                // Decrement memory
                case OPCODE_DEC:
                    byte decResult = (byte)(operand - 1);
                    _memory.SetByte(decResult, calculatedAddress);
                    UpdateFlags(decResult, PFlags.N | PFlags.Z);
                    break;

                // Increment memory
                case OPCODE_INC:
                    byte incResult = (byte)(operand + 1);
                    _memory.SetByte(incResult, calculatedAddress);
                    UpdateFlags(incResult, PFlags.N | PFlags.Z);
                    break;
            }

            // Update program counter
            _cpu.PC += (ushort)(operandAddress + +_addressingModeToPCDelta[addressingMode]);
        }

        public void ExecuteNextInstruction()
        {
            // First get the opcode at the current PC
            var pc = _cpu.PC;
            var fullOpCode = _memory.GetByte(pc);

            // Split out the parts of the op code
            var opCodeGroup = (byte)(fullOpCode & 0b00000011);
            var addressingMode = (byte)((fullOpCode & 0b00011100) >> 2);
            var opCode = (byte)((fullOpCode & 0b11100000) >> 5);

            switch (opCodeGroup)
            {
                case 0b000:
                    ExecuteNextGroupZeroInstruction(opCode, addressingMode);
                    break;

                case 0b001:
                    ExecuteNextGroupOneInstruction(opCode, addressingMode);
                    break;

                case 0b010:
                    ExecuteNextGroupTwoInstruction(opCode, addressingMode);
                    break;
            }
        }

        public void ExecuteToBrk()
        {
            while (_memory.GetByte(_cpu.PC) != FULL_OPCODE_BRK)
            {
                ExecuteNextInstruction();
            }
        }

        // All possible addressing modes
        public enum AddressingMode : ushort
        {
            Immediate = 0,  // 8 bit value stored directly in next byte
            Absolute = 1, // 16 bit address stored in next two bytes
            AbsoluteIndexedByX = 2,  // 16 bit memory address stored in next two bytes + Y
            AbsoluteIndexedByY = 3, // 16 bit memory stored in next two bytes + X
            ZeroPage = 4, // 8 bit zero page address stored in next byte
            ZeroPageIndexedByX = 5, // 8 bit zero page address stored in next byte + X
            ZeroPageIndexedByY = 6, // 8 bit zero page address stored in next byte + Y
            ZeroPageIndexedByXThenIndirect = 7, // (8 bit zero page address in next byte + X)
            ZeroPageIndirectThenIndexedByY = 8, // (8 bit zero page address stored in next byte) + Y
            Accumulator = 9 // Accumulator addressing
        }

        // Delta's to PC added by each address mode
        public readonly Dictionary<AddressingMode, byte> _addressingModeToPCDelta = new Dictionary<AddressingMode, byte>
        {
            { AddressingMode.Immediate, 1 },
            { AddressingMode.Absolute, 2 },
            { AddressingMode.AbsoluteIndexedByX, 2 },
            { AddressingMode.AbsoluteIndexedByY, 2 },
            { AddressingMode.ZeroPage, 1 },
            { AddressingMode.ZeroPageIndexedByX, 1 },
            { AddressingMode.ZeroPageIndexedByY, 1 },
            { AddressingMode.ZeroPageIndexedByXThenIndirect, 1 },
            { AddressingMode.ZeroPageIndirectThenIndexedByY, 1 },
            { AddressingMode.Accumulator, 0 },
        };

        private readonly Dictionary<byte, AddressingMode> _groupZeroAddressModeMap = new Dictionary<byte, AddressingMode> {
            { 0b000, AddressingMode.Immediate },
            { 0b001, AddressingMode.ZeroPage },
            { 0b011, AddressingMode.Absolute },
            { 0b101, AddressingMode.ZeroPageIndexedByX },
            { 0b111, AddressingMode.AbsoluteIndexedByX }
        };

        private readonly Dictionary<byte, AddressingMode> _groupOneAddressModeMap = new Dictionary<byte, AddressingMode>
        {
            { 0b000, AddressingMode.ZeroPageIndexedByXThenIndirect },
            { 0b001, AddressingMode.ZeroPage },
            { 0b010, AddressingMode.Immediate },
            { 0b011, AddressingMode.Absolute },
            { 0b100, AddressingMode.ZeroPageIndirectThenIndexedByY },
            { 0b101, AddressingMode.ZeroPageIndexedByX },
            { 0b110, AddressingMode.AbsoluteIndexedByY },
            { 0b111, AddressingMode.AbsoluteIndexedByX }
        };

        private readonly Dictionary<byte, AddressingMode> _groupTwoAddressModeMap = new Dictionary<byte, AddressingMode>
        {
            { 0b000, AddressingMode.Immediate },
            { 0b001, AddressingMode.ZeroPage },
            { 0b010, AddressingMode.Accumulator },
            { 0b011, AddressingMode.Absolute },
            { 0b101, AddressingMode.ZeroPageIndexedByX },
            { 0b111, AddressingMode.AbsoluteIndexedByX }
        };
    }
}