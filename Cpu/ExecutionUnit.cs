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

        private (ushort calculatedAddress, byte pcDelta) GetAddressFromMemory(ushort address, AddressingMode addressingMode)
        {
            return addressingMode switch
            {
                AddressingMode.Immediate => (calculatedAddress: address, pcDelta: 1),
                AddressingMode.Absolute => (calculatedAddress: _memory.GetWord(address), pcDelta: 2),
                AddressingMode.AbsoluteIndexedByX => (calculatedAddress: (ushort)(_memory.GetWord(address) + _cpu.X), pcDelta: 2),
                AddressingMode.AbsoluteIndexedByY => (calculatedAddress: (ushort)(_memory.GetWord(address) + _cpu.Y), pcDelta: 2),
                AddressingMode.ZeroPage => (calculatedAddress: _memory.GetByte(address), pcDelta: 1),
                AddressingMode.ZeroPageIndexedByX => (calculatedAddress: (ushort)(_memory.GetByte(address) + _cpu.X), pcDelta: 1),
                AddressingMode.ZeroPageIndexedByY => (calculatedAddress: (ushort)(_memory.GetByte(address) + _cpu.Y), pcDelta: 1),
                AddressingMode.ZeroPageIndexedByXThenIndirect => (calculatedAddress: _memory.GetWord((ushort)(_memory.GetByte(address) + _cpu.X)), pcDelta: 1),
                AddressingMode.ZeroPageIndirectThenIndexedByY => (calculatedAddress: (ushort)(_memory.GetByte(_memory.GetByte(address)) + _cpu.Y), pcDelta: 1),
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

        // Group 1 op codes

        private const byte OPCODE_ORA = 0b000;
        private const byte OPCODE_AND = 0b001;
        private const byte OPCODE_EOR = 0b010;
        private const byte OPCODE_ADC = 0b011;
        private const byte OPCODE_STA = 0b100;
        private const byte OPCODE_LDA = 0b101;
        private const byte OPCODE_CMP = 0b110;
        private const byte OPCODE_SBC = 0b111;

        private void ExecuteNextGroup1Instruction(byte opCode, byte rawAddressingMode)
        {
            // Operand follows immediately after the op code
            var operandAddress = (ushort)(_cpu.PC + 1);

            // Decode the addressing mode
            var addressingMode = _groupOneAddressModeMap[rawAddressingMode];  // TODO: Exception on invalid mode

            // Grab the address indicated by the operand and addressing mode
            (var calculatedAddress, var pcDelta) = GetAddressFromMemory(operandAddress, addressingMode);

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

                case OPCODE_ADC: // Add with carry
                    ushort addWithCarryResult = (ushort)(_cpu.A + _memory.GetByte(calculatedAddress) + (_cpu.PIsSet(PFlags.C) ? 1 : 0));
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    _cpu.PSet(PFlags.C, addWithCarryResult > 0xFF);
                    _cpu.PSet(PFlags.V, ((_cpu.A ^ addWithCarryResult) & 0x80) != 0); // Overflow if bit 7 was changed
                    _cpu.A = (byte)addWithCarryResult;
                    break;

                case OPCODE_STA: // Store accumulator in memory
                    _memory.Set(_cpu.A, calculatedAddress);
                    break;

                case OPCODE_LDA: // Load accumulator from memory
                    _cpu.A = _memory.GetByte(calculatedAddress);
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    break;

                // TODO: Is BCD mode relevant here ?
                case OPCODE_CMP: // Compare value to accumulator
                    var valueToCompareTo = _memory.GetByte(calculatedAddress);
                    _cpu.PSet(PFlags.N, _cpu.A < valueToCompareTo); // TODO: Check this seems to disagree with the reference, maybe do the subtraction and look at bit 7?
                    _cpu.PSet(PFlags.C, _cpu.A >= valueToCompareTo);
                    _cpu.PSet(PFlags.Z, _cpu.A == valueToCompareTo);
                    break;

                case OPCODE_SBC: // Subtract with carry // TODO: Check it all!
                    ushort subtractWithCarryResult = (ushort)(_cpu.A - _memory.GetByte(calculatedAddress) - (_cpu.PIsSet(PFlags.C) ? 1 : 0));
                    UpdateFlags(_cpu.A, PFlags.N | PFlags.Z);
                    _cpu.PSet(PFlags.C, subtractWithCarryResult >= 0);
                    _cpu.PSet(PFlags.V, ((_cpu.A ^ subtractWithCarryResult) & 0x80) != 0); // Overflow if bit 7 was changed
                    _cpu.A = (byte)subtractWithCarryResult;
                    break;
            }

            // Update program counter
            _cpu.PC += (ushort)(operandAddress + pcDelta);
        }

        public void ExecuteNextInstruction()
        {
            // First get the opcode at the current PC
            var pc = _cpu.PC;
            var fullOpCode = _memory.GetByte(pc);

            // Split out the parts of the op code
            var opCodeGroup = (byte)(fullOpCode & 0b00000011);
            var addressingMode = (byte)(fullOpCode & 0b00011100 >> 2);
            var opCode = (byte)(fullOpCode & 0b11100000 >> 5);

            switch (opCodeGroup)
            {
                case 0b000:
                    throw new NotImplementedException();
                    break;

                case 0b001:
                    ExecuteNextGroup1Instruction(opCode, addressingMode);
                    break;

                case 0b010:
                    throw new NotImplementedException();
                    break;

                case 0b011:
                    throw new NotImplementedException();
                    break;
            }
        }

        // TODO - Might need to add accumulator addressing mode in here?
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
            Size = 9
        }

        private readonly Dictionary<byte, AddressingMode> _groupOneAddressModeMap = new Dictionary<byte, AddressingMode>
        {
            { 0b000, AddressingMode.ZeroPageIndexedByXThenIndirect },
            { 0b001, AddressingMode.ZeroPage  },
            { 0b010, AddressingMode.Immediate },
            { 0b011, AddressingMode.Absolute  },
            { 0b100, AddressingMode.ZeroPageIndirectThenIndexedByY  },
            { 0b101, AddressingMode.ZeroPageIndexedByX  },
            { 0b110, AddressingMode.AbsoluteIndexedByY  },
            { 0b111, AddressingMode.AbsoluteIndexedByX }
        };
    }
}