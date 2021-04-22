using System;
using System.Collections.Generic;
using System.Linq;

namespace BbcMicro.Cpu
{
    public sealed class CPU
    {
        #region Configuration
        /*
         * Set up
        */
        private sealed class ExecutionDefinition
        {
            private readonly Action<ushort, AddressingMode> _implementation;
            private readonly List<(byte opCode, AddressingMode addressingMode)> _codeAndMode;

            public ExecutionDefinition(Action<ushort, AddressingMode> implementation, List<(byte opCode, AddressingMode addressingMode)> codeAndMode)
            {
                _implementation = implementation;
                _codeAndMode = codeAndMode;
            }

            public Action<ushort, AddressingMode> Impl { get { return _implementation; } }

            public IReadOnlyList<(byte opCode, AddressingMode addressingMode)> CodeAndMode
            {
                get
                {
                    return _codeAndMode.AsReadOnly();
                }
            }
        }

        private (Action<ushort, AddressingMode> impl, AddressingMode addressingMode)[] _decoder = new (Action<ushort, AddressingMode>, AddressingMode)[256];
        private bool[] _opcodeIsValid = new bool[256];

        private void PopulateDecoder()
        {
            foreach (var executionDefinition in _executionDefinitions)
            {
                foreach (var codeAndMode in executionDefinition.CodeAndMode)
                {
                    if (_opcodeIsValid[codeAndMode.opCode])
                    {
                        throw new Exception("Seen it before"); // TODO
                    }
                    else
                    {
                        _opcodeIsValid[codeAndMode.opCode] = true;
                    }
                    _decoder[codeAndMode.opCode] = (impl: executionDefinition.Impl, addressingMode: codeAndMode.addressingMode);
                }
            }
        }
        #endregion

        private readonly List<ExecutionDefinition> _executionDefinitions;

        public CPU(IAddressSpace addressSpace)
        {
            Memory = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));

            #region Op code mappings
            _executionDefinitions = new List<ExecutionDefinition>()
            {
                new ExecutionDefinition(ADC,
                    new List<(byte, AddressingMode)> {
                        (0x69, AddressingMode.Immediate),
                        (0x65, AddressingMode.ZeroPage),
                        (0x75, AddressingMode.ZeroPageIndexedX),
                        (0x6D, AddressingMode.Absolute),
                        (0x7D, AddressingMode.AbsoluteIndexedX),
                        (0x79, AddressingMode.AbsoluteIndexedY),
                        (0x61, AddressingMode.IndexedXIndirect),
                        (0x71, AddressingMode.IndirectIndexedY),
                    }),
                new ExecutionDefinition(AND,
                    new List<(byte, AddressingMode)> {
                        (0x29, AddressingMode.Immediate),
                        (0x25, AddressingMode.ZeroPage),
                        (0x35, AddressingMode.ZeroPageIndexedX),
                        (0x2D, AddressingMode.Absolute),
                        (0x3D, AddressingMode.AbsoluteIndexedX),
                        (0x39, AddressingMode.AbsoluteIndexedY),
                        (0x21, AddressingMode.IndexedXIndirect),
                        (0x31, AddressingMode.IndirectIndexedY),
                    }),
                new ExecutionDefinition(ASL,
                    new List<(byte, AddressingMode)> {
                        (0x0A, AddressingMode.Accumulator),
                        (0x06, AddressingMode.ZeroPage),
                        (0x16, AddressingMode.ZeroPageIndexedX),
                        (0x0E, AddressingMode.Absolute),
                        (0x1E, AddressingMode.AbsoluteIndexedX)
                    }),
                new ExecutionDefinition(BCC,
                    new List<(byte, AddressingMode)> {
                        (0x90, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(BCS,
                    new List<(byte, AddressingMode)> {
                        (0xB0, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(BEQ,
                    new List<(byte, AddressingMode)> {
                        (0xF0, AddressingMode.Relative)
                    }),
                 new ExecutionDefinition(BIT,
                    new List<(byte, AddressingMode)> {
                        (0x24, AddressingMode.ZeroPage),
                        (0x2C, AddressingMode.Absolute)
                    }),
                new ExecutionDefinition(BMI,
                    new List<(byte, AddressingMode)> {
                        (0x30, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(BNE,
                    new List<(byte, AddressingMode)> {
                        (0xD0, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(BPL,
                    new List<(byte, AddressingMode)> {
                        (0x10, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(BRK,
                    new List<(byte, AddressingMode)> {
                        (0x00, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(BVC,
                    new List<(byte, AddressingMode)> {
                        (0x50, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(BVS,
                    new List<(byte, AddressingMode)> {
                        (0x50, AddressingMode.Relative)
                    }),
                new ExecutionDefinition(CLC,
                    new List<(byte, AddressingMode)> {
                        (0x18, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(CLD,
                    new List<(byte, AddressingMode)> {
                        (0xD8, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(CLI,
                    new List<(byte, AddressingMode)> {
                        (0x58, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(CLV,
                    new List<(byte, AddressingMode)> {
                        (0xB8, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(CMP,
                    new List<(byte, AddressingMode)> {
                        (0xC9, AddressingMode.Immediate),
                        (0xC5, AddressingMode.ZeroPage),
                        (0xD5, AddressingMode.ZeroPageIndexedX),
                        (0xCD, AddressingMode.Absolute),
                        (0xDD, AddressingMode.AbsoluteIndexedX),
                        (0xD9, AddressingMode.AbsoluteIndexedY),
                        (0xC1, AddressingMode.IndexedXIndirect),
                        (0xD1, AddressingMode.IndirectIndexedY)
                    }),
                new ExecutionDefinition(CPX,
                    new List<(byte, AddressingMode)> {
                        (0xE0, AddressingMode.Immediate),
                        (0xE4, AddressingMode.ZeroPage),
                        (0xEC, AddressingMode.Absolute)
                }),
                new ExecutionDefinition(CPY,
                    new List<(byte, AddressingMode)> {
                        (0xC0, AddressingMode.Immediate),
                        (0xC4, AddressingMode.ZeroPage),
                        (0xCC, AddressingMode.Absolute)
                }),
                new ExecutionDefinition(DEC,
                    new List<(byte, AddressingMode)> {
                        (0xC6, AddressingMode.ZeroPage),
                        (0xD6, AddressingMode.ZeroPageIndexedX),
                        (0xCE, AddressingMode.Absolute),
                        (0xDE, AddressingMode.AbsoluteIndexedX)
                }),
                new ExecutionDefinition(DEX,
                    new List<(byte, AddressingMode)> {
                        (0xCA, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(DEY,
                    new List<(byte, AddressingMode)> {
                        (0x88, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(EOR,
                    new List<(byte, AddressingMode)> {
                        (0x49, AddressingMode.Immediate),
                        (0x45, AddressingMode.ZeroPage),
                        (0x55, AddressingMode.ZeroPageIndexedX),
                        (0x4D, AddressingMode.Absolute),
                        (0x5D, AddressingMode.AbsoluteIndexedX),
                        (0x59, AddressingMode.AbsoluteIndexedY),
                        (0x41, AddressingMode.IndexedXIndirect),
                        (0x51, AddressingMode.IndirectIndexedY)
                    }),
                new ExecutionDefinition(INC,
                    new List<(byte, AddressingMode)> {
                        (0xE6, AddressingMode.ZeroPage),
                        (0xF6, AddressingMode.ZeroPageIndexedX),
                        (0xEE, AddressingMode.Absolute),
                        (0xFE, AddressingMode.AbsoluteIndexedX)
                    }),
                new ExecutionDefinition(INX,
                    new List<(byte, AddressingMode)> {
                        (0xE8, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(INY,
                    new List<(byte, AddressingMode)> {
                        (0xC8, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(JMP,
                    new List<(byte, AddressingMode)> {
                        (0x4C, AddressingMode.Absolute),
                        (0x6C, AddressingMode.Indirect)
                    }),
                new ExecutionDefinition(JSR,
                    new List<(byte, AddressingMode)> {
                        (0x20, AddressingMode.Absolute)
                    }),
                new ExecutionDefinition(LDA,
                    new List<(byte, AddressingMode)> {
                        (0xA9, AddressingMode.Immediate),
                        (0xA5, AddressingMode.ZeroPage),
                        (0xB5, AddressingMode.ZeroPageIndexedX),
                        (0xAD, AddressingMode.Absolute),
                        (0xBD, AddressingMode.AbsoluteIndexedX),
                        (0xB9, AddressingMode.AbsoluteIndexedY),
                        (0xA1, AddressingMode.IndexedXIndirect),
                        (0xB1, AddressingMode.IndirectIndexedY)
                    }),
                new ExecutionDefinition(LDX,
                    new List<(byte, AddressingMode)> {
                        (0xA2, AddressingMode.Immediate),
                        (0xA6, AddressingMode.ZeroPage),
                        (0xB6, AddressingMode.ZeroPageIndexedY),
                        (0xAE, AddressingMode.Absolute),
                        (0xBE, AddressingMode.AbsoluteIndexedY)
                    }),
                new ExecutionDefinition(LDY,
                    new List<(byte, AddressingMode)> {
                        (0xA0, AddressingMode.Immediate),
                        (0xA4, AddressingMode.ZeroPage),
                        (0xB4, AddressingMode.ZeroPageIndexedX),
                        (0xAC, AddressingMode.Absolute),
                        (0xBC, AddressingMode.AbsoluteIndexedX)
                    }),
                new ExecutionDefinition(LSR,
                    new List<(byte, AddressingMode)> {
                        (0x4A, AddressingMode.Accumulator),
                        (0x46, AddressingMode.ZeroPage),
                        (0x56, AddressingMode.ZeroPageIndexedX),
                        (0x4E, AddressingMode.Absolute),
                        (0x5E, AddressingMode.AbsoluteIndexedX)
                    }),
                new ExecutionDefinition(NOP,
                    new List<(byte, AddressingMode)> {
                        (0xEA, AddressingMode.Implied)
                    }),
                new ExecutionDefinition(ORA,
                    new List<(byte, AddressingMode)> {
                        (0x09, AddressingMode.Immediate),
                        (0x05, AddressingMode.ZeroPage),
                        (0x15, AddressingMode.ZeroPageIndexedX),
                        (0x0D, AddressingMode.Absolute),
                        (0x1D, AddressingMode.AbsoluteIndexedX),
                        (0x19, AddressingMode.AbsoluteIndexedY),
                        (0x01, AddressingMode.IndexedXIndirect),
                        (0x11, AddressingMode.IndirectIndexedY)
                    }),
                 new ExecutionDefinition(PHA,
                    new List<(byte, AddressingMode)> {
                        (0x48, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(PHP,
                    new List<(byte, AddressingMode)> {
                        (0x08, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(PLA,
                    new List<(byte, AddressingMode)> {
                        (0x68, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(PLP,
                    new List<(byte, AddressingMode)> {
                        (0x28, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(ROL,
                    new List<(byte, AddressingMode)> {
                        (0x2A, AddressingMode.Accumulator),
                        (0x26, AddressingMode.ZeroPage),
                        (0x36, AddressingMode.ZeroPageIndexedX),
                        (0x2E, AddressingMode.Absolute),
                        (0x3E, AddressingMode.AbsoluteIndexedX)
                    }),
                 new ExecutionDefinition(ROR,
                    new List<(byte, AddressingMode)> {
                        (0x6A, AddressingMode.Accumulator),
                        (0x66, AddressingMode.ZeroPage),
                        (0x76, AddressingMode.ZeroPageIndexedX),
                        (0x6E, AddressingMode.Absolute),
                        (0x7E, AddressingMode.AbsoluteIndexedX)
                    }),
                 new ExecutionDefinition(RTI,
                    new List<(byte, AddressingMode)> {
                        (0x40, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(RTS,
                    new List<(byte, AddressingMode)> {
                        (0x60, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(SBC,
                    new List<(byte, AddressingMode)> {
                        (0xE9, AddressingMode.Immediate),
                        (0xE5, AddressingMode.ZeroPage),
                        (0xF5, AddressingMode.ZeroPageIndexedX),
                        (0xED, AddressingMode.Absolute),
                        (0xFD, AddressingMode.AbsoluteIndexedX),
                        (0xF9, AddressingMode.AbsoluteIndexedY),
                        (0xE1, AddressingMode.IndexedXIndirect),
                        (0xF1, AddressingMode.IndirectIndexedY),
                    }),
                 new ExecutionDefinition(SEC,
                    new List<(byte, AddressingMode)> {
                        (0x38, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(SED,
                    new List<(byte, AddressingMode)> {
                        (0xF8, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(SEI,
                    new List<(byte, AddressingMode)> {
                        (0x78, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(STA,
                    new List<(byte, AddressingMode)> {
                        (0x85, AddressingMode.ZeroPage),
                        (0x95, AddressingMode.ZeroPageIndexedX),
                        (0x8D, AddressingMode.Absolute),
                        (0x9D, AddressingMode.AbsoluteIndexedX),
                        (0x99, AddressingMode.AbsoluteIndexedY),
                        (0x81, AddressingMode.IndexedXIndirect),
                        (0x91, AddressingMode.IndirectIndexedY)
                    }),
                 new ExecutionDefinition(STX,
                    new List<(byte, AddressingMode)> {
                        (0x86, AddressingMode.ZeroPage),
                        (0x96, AddressingMode.ZeroPageIndexedY),
                        (0x8E, AddressingMode.Absolute)
                    }),
                 new ExecutionDefinition(STY,
                    new List<(byte, AddressingMode)> {
                        (0x84, AddressingMode.ZeroPage),
                        (0x94, AddressingMode.ZeroPageIndexedX),
                        (0x8C, AddressingMode.Absolute)
                    }),
                 new ExecutionDefinition(TAX,
                    new List<(byte, AddressingMode)> {
                        (0xAA, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(TAY,
                    new List<(byte, AddressingMode)> {
                        (0xA8, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(TSX,
                    new List<(byte, AddressingMode)> {
                        (0xBA, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(TXA,
                    new List<(byte, AddressingMode)> {
                        (0x8A, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(TXS,
                    new List<(byte, AddressingMode)> {
                        (0x9A, AddressingMode.Implied)
                    }),
                 new ExecutionDefinition(TYA,
                    new List<(byte, AddressingMode)> {
                        (0x98, AddressingMode.Implied)
                    })
            };
            #endregion

            PopulateDecoder();
        }

        #region Callbacks

        /*
         * Callbacks
         */

        private readonly List<Action<CPU>> _preExecutionCallbacks = new List<Action<CPU>>();
        private readonly List<Action<CPU>> _postExecutionCallbacks = new List<Action<CPU>>();
        private Action<string, CPU> _errorCallback = null;

        public void AddPreExecutionCallback(Action<CPU> callback)
        {
            _preExecutionCallbacks.Add(callback);
        }

        public void AddPostExecutionCallback(Action<CPU> callback)
        {
            _postExecutionCallbacks.Add(callback);
        }

        public void SetErrorCallback(Action<string, CPU> callback)
        {
            _errorCallback = callback;
        }
        #endregion

        #region Execution

        /*
         * Execution
         */

        public bool ExecuteNextInstruction()
        {
            var result = true;

            // Grab to opCode at PC
            byte opCode = Memory.GetByte(PC);

            // Is this a valid op code?
            if (_opcodeIsValid[opCode])
            {
                // Call any pre-execution callbacks
                _preExecutionCallbacks.ForEach(cb => cb(this));

                // Find the decoder entry for the instruction
                var decoded = _decoder[opCode];

                // Opcode operand follows immediately after the op code, skip forward one
                var operandAddress = PC++;

                // Calculate the address of the target value based on the addressing mode
                var resolvedAddress = ResolveAddress(PC, decoded.addressingMode);

                // Move the PC forward based on the addressing mode
                PC += _addressingModeToPCDelta[(ushort)decoded.addressingMode];

                // Interpret the instruction
                decoded.impl(resolvedAddress, decoded.addressingMode);

                // Call any post-execution callbacks
                _postExecutionCallbacks.ForEach(cb => cb(this));
            }
            else
            {
                result = false;
                if (_errorCallback != null)
                {
                    _errorCallback($"Invalid op code 0x{opCode:X2} at address 0x{PC:X2}", this);
                }
            }

            return result;
        }

        public void ExecuteToBrk()
        {
            while (Memory.GetByte(PC) != 0x00)
            {
                if (!ExecuteNextInstruction())
                {
                    throw new CPUException(this);
                }
            }
        }
        #endregion

        #region Opcode implementations
        /*
         * Op code implementations
         * 
         * Definitions are taken from http://www.obelisk.me.uk/6502/reference.html
         */

        private void UpdateFlags(byte accordingToValue, PFlags flagsToUpdate)
        {
            if (flagsToUpdate.HasFlag(PFlags.Z))
            {
                PSet(PFlags.Z, accordingToValue == 0);
            }

            if (flagsToUpdate.HasFlag(PFlags.N))
            {
                PSet(PFlags.N, (accordingToValue & 0b10000000) != 0);
            }
        }

        /*
         * Add with carry
         * 
         * This instruction adds the contents of a memory location to the accumulator 
         * together with the carry bit. If overflow occurs the carry bit is set, 
         * this enables multiple byte addition to be performed.
         */
        private void ADC(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            short result = (short)(A + operand + (PIsSet(PFlags.C) ? 1 : 0));
            PSet(PFlags.C, result > 0xFF);
            PSet(PFlags.V, ((A ^ result) & (operand ^ result) & 0x80) != 0);
            A = (byte)result;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Logical and
         * 
         * A logical AND is performed, bit by bit, on the accumulator contents 
         * using the contents of a byte of memory.
         */
        private void AND(ushort operandAddress, AddressingMode addressingMode) {
            A &= Memory.GetByte(operandAddress);
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Arithmetic shift left
         * 
         * This operation shifts all the bits of the accumulator or memory contents one
         * bit left. Bit 0 is set to 0 and bit 7 is placed in the carry flag. The 
         * effect of this operation is to multiply the memory contents by 2 
         * (ignoring 2's complement considerations), setting the carry if the result 
         * will not fit in 8 bits.
         */
        private void ASL(ushort operandAddress, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                PSet(PFlags.C, (A & 0b1000_0000) != 0);
                A = (byte)(A << 1);
                UpdateFlags(A, PFlags.N | PFlags.Z);
            }
            else
            {
                byte operand = Memory.GetByte(operandAddress);
                Memory.SetByte((byte)(operand << 1), operandAddress);
                PSet(PFlags.C, (operand & 0b1000_0000) != 0);
                UpdateFlags(operand, PFlags.N | PFlags.Z);
            }
        }

        /*
         * Branch if carry clear
         * 
         * If the carry flag is clear then add the relative displacement to the 
         * program counter to cause a branch to a new location.
         */
        private void BCC(ushort operandAddress, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.C))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Branch if carry set
         * 
         * If the carry flag is set then add the relative displacement to the 
         * program counter to cause a branch to a new location.
         */
        private void BCS(ushort operandAddress, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.C))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Branch if equal
         * 
         * If the zero flag is set then add the relative displacement to 
         * the program counter to cause a branch to a new location.
         */
        private void BEQ(ushort operandAddress, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.Z))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Bit test
         * 
         * This instructions is used to test if one or more bits are set
         * in a target memory location. The mask pattern in A is ANDed with 
         * the value in memory to set or clear the zero flag, but the result
         * is not kept. Bits 7 and 6 of the value from memory are copied into 
         * the N and V flags.
         */
        private void BIT(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            PSet(PFlags.Z, (A & operand) == 0);
            PSet(PFlags.V, (operand & 0b0100_0000) != 0);
            PSet(PFlags.N, (operand & 0b1000_0000) != 0);
        }

        /*
         * Branch if minus
         * 
         * If the negative flag is set then add the relative displacement to 
         * the program counter to cause a branch to a new location.
         */
        private void BMI(ushort operandAddress, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.N))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Branch if not equal
         * 
         * If the zero flag is clear then add the relative displacement 
         * to the program counter to cause a branch to a new location.
         */
        private void BNE(ushort operandAddress, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.Z))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Branch if positive
         * 
         * If the negative flag is clear then add the relative displacement
         * to the program counter to cause a branch to a new location.
         */
        private void BPL(ushort operandAddress, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.N))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Force Interrupt
         * 
         * The BRK instruction forces the generation of an interrupt request. 
         * The program counter and processor status are pushed on the stack then 
         * the IRQ interrupt vector at $FFFE/F is loaded into the PC and the break 
         * flag in the status set to one.
         * 
         * The interpretation of a BRK depends on the operating system. On the BBC 
         * Microcomputer it is used by language ROMs to signal run time errors but 
         * it could be used for other purposes (e.g. calling operating system 
         * functions, etc.).
         */
        private void BRK(ushort operandAddress, AddressingMode addressingMode)
        {
            // TODO
            throw new NotImplementedException("BRK");
        }

        /*
         * Branch if overflow clear
         * 
         * If the overflow flag is clear then add the relative displacement to 
         * the program counter to cause a branch to a new location.
         */
        private void BVC(ushort operandAddress, AddressingMode addressingMode)
        {
            if (!PIsSet(PFlags.V))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Branch if overflow set
         * 
         * If the overflow flag is set then add the relative displacement to 
         * the program counter to cause a branch to a new location.
         */
        private void BVS(ushort operandAddress, AddressingMode addressingMode)
        {
            if (PIsSet(PFlags.V))
            {
                // Cast to sbyte as the offset is signed
                PC = (ushort)(PC + (sbyte)Memory.GetByte(operandAddress));
            }
        }

        /*
         * Clear carry
         */ 
        private void CLC(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.C);
        }

        /*
         * Clear decimal mode
         */
        private void CLD(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.D);
        }

        /*
         * Clear interrupt disable
         * 
         * Clears the interrupt disable flag allowing normal interrupt requests
         * to be serviced.
         */
        private void CLI(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.I);
        }

        /*
         * Clear overflow flag
         */ 
        private void CLV(ushort operandAddress, AddressingMode addressingMode)
        {
            PReset(PFlags.V);
        }

        /*
         * Compare
         * 
         * This instruction compares the contents of the accumulator with another
         * memory held value and sets the zero and carry flags as appropriate.
         */
        private void CMP(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            PSet(PFlags.N, A < operandAddress);
            PSet(PFlags.C, A >= operandAddress);
            PSet(PFlags.Z, A == operandAddress);
        }

       /*
        * Compare X register
        * 
        * This instruction compares the contents of the X register with another 
        * memory held value and sets the zero and carry flags as appropriate.
        */
        private void CPX(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            PSet(PFlags.N, X < operandAddress);
            PSet(PFlags.C, X >= operandAddress);
            PSet(PFlags.Z, X == operandAddress);
        }

        /*
         * Compare Y register
         * 
         * This instruction compares the contents of the Y register with another
         * memory held value and sets the zero and carry flags as appropriate.
         */
        private void CPY(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            PSet(PFlags.N, Y < operandAddress);
            PSet(PFlags.C, Y >= operandAddress);
            PSet(PFlags.Z, Y == operandAddress);
        }

        /*
         * Decrement memory
         * 
         * Subtracts one from the value held at a specified memory location 
         * setting the zero and negative flags as appropriate.
         */
        private void DEC(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            byte result = (byte)(operand - 1);
            Memory.SetByte(result, operandAddress);
            UpdateFlags(result, PFlags.N | PFlags.Z);
        }

        /*
         * Decrement X register
         * 
         * Subtracts one from the X register setting the zero and negative 
         * flags as appropriate.
         */
        private void DEX(ushort operandAddress, AddressingMode addressingMode)
        {
            X -= 1;
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Decrement Y register
         * 
         * Subtracts one from the Y register setting the zero and negative 
         * flags as appropriate.
         */
        private void DEY(ushort operandAddress, AddressingMode addressingMode)
        {
            Y -= 1;
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Exclusive or
         * 
         * An exclusive OR is performed, bit by bit, on the accumulator 
         * contents using the contents of a byte of memory.
         */
        private void EOR(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            A ^= operand;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Increment memory
         * 
         * Adds one to the value held at a specified memory location 
         * setting the zero and negative flags as appropriate.
         */
        private void INC(ushort operandAddress, AddressingMode addressingMode)
        {
            byte operand = Memory.GetByte(operandAddress);
            operand++;
            Memory.SetByte(operand, operandAddress);
            UpdateFlags(operand, PFlags.N | PFlags.Z);
        }

        /*
         * Increment X register
         * 
         * Adds one to the X register setting the zero and negative flags 
         * as appropriate.
         */
        private void INX(ushort operandAddress, AddressingMode addressingMode)
        {
            X++;
            UpdateFlags(X, PFlags.N | PFlags.Z);

        }

        /*
         * Increment Y register
         * 
         * Adds one to the X register setting the zero and negative flags 
         * as appropriate.
         */
        private void INY(ushort operandAddress, AddressingMode addressingMode)
        {
            Y++;
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Jump
         * 
         * Sets the program counter to the address specified by the operand.
         * 
         * An original 6502 has does not correctly fetch the target address if 
         * the indirect vector falls on a page boundary (e.g. $xxFF where xx is 
         * any value from $00 to $FF). In this case fetches the LSB from $xxFF 
         * as expected but takes the MSB from $xx00. This is fixed in some later 
         * chips like the 65SC02 so for compatibility always ensure the indirect 
         * vector is not at the end of the page.
         */
        private void JMP(ushort operandAddress, AddressingMode addressingMode)
        {
            PC = Memory.GetWord(operandAddress);
        }

        /*
         * Jump to Subroutine
         * 
         * The JSR instruction pushes the address (minus one) of the return point
         * on to the stack and then sets the program counter to the target memory
         * address.
         */
        private void JSR(ushort operandAddress, AddressingMode addressingMode)
        {
            // Target address in host byte order
            ushort operand = Memory.GetByte(operandAddress);

            // This will be in host machine byte order
            ushort addressToPush = (ushort)(PC - 1); // This is what the 6502 does! 
            byte operandLSB = (byte)addressToPush;
            byte operandMSB = (byte)(addressToPush >> 8);

            // Push the return address onto the stack
            PushByte(operandMSB);
            PushByte(operandLSB);

            PC = operand;
        }

        /*
         * Load accumulator
         * 
         * Loads a byte of memory into the accumulator setting the 
         * zero and negative flags as appropriate.
         */
        private void LDA(ushort operandAddress, AddressingMode addressingMode)
        {
            A = Memory.GetByte(operandAddress);
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        /*
         * Load X register
         * 
         * Loads a byte of memory into the X register setting the zero 
         * and negative flags as appropriate.
         */
        private void LDX(ushort operandAddress, AddressingMode addressingMode)
        {
            X = Memory.GetByte(operandAddress);
            UpdateFlags(X, PFlags.N | PFlags.Z);
        }

        /*
         * Load Y register
         * 
         * Loads a byte of memory into the Y register setting the zero 
         * and negative flags as appropriate.
         */
        private void LDY(ushort operandAddress, AddressingMode addressingMode)
        {
            Y = Memory.GetByte(operandAddress);
            UpdateFlags(Y, PFlags.N | PFlags.Z);
        }

        /*
         * Logical shift right
         * 
         * Each of the bits in A or M is shift one place to the right. The 
         * bit that was in bit 0 is shifted into the carry flag. Bit 7 is set 
         * to zero.
         */
        private void LSR(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * No operation
         * 
         * The NOP instruction causes no changes to the processor other 
         * than the normal incrementing of the program counter to the next 
         * instruction.
         */
        private void NOP(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Logical inclusive or
         * 
         * An inclusive OR is performed, bit by bit, on the accumulator 
         * contents using the contents of a byte of memory.
         */
        private void ORA(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Push accumulator
         * 
         * Pushes a copy of the accumulator on to the stack.
         */
        private void PHA(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Push processor status
         * 
         * Pushes a copy of the status flags on to the stack.
         */
        private void PHP(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Pull accumulator
         * 
         * Pulls an 8 bit value from the stack and into the accumulator. 
         * The zero and negative flags are set as appropriate.
         */
        private void PLA(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Pull processor status
         * 
         * Pulls an 8 bit value from the stack and into the processor flags. 
         * The flags will take on new states as determined by the value pulled.
         */
        private void PLP(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Rotate left
         * 
         * Move each of the bits in either A or M one place to the left. Bit 0
         * is filled with the current value of the carry flag whilst the old 
         * bit 7 becomes the new carry flag value.
         */
        private void ROL(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Rotate right
         * 
         * Move each of the bits in either A or M one place to the right. Bit 7
         * is filled with the current value of the carry flag whilst the old 
         * bit 0 becomes the new carry flag value.
         */
        private void ROR(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Return from interrupt
         * 
         * The RTI instruction is used at the end of an interrupt processing 
         * routine. It pulls the processor flags from the stack followed by 
         * the program counter.
         */
        private void RTI(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Return from subroutine
         * 
         * The RTS instruction is used at the end of a subroutine to return 
         * to the calling routine. It pulls the program counter (minus one) from the stack.
         */
        private void RTS(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Subtract with carry
         * 
         * This instruction subtracts the contents of a memory location to the 
         * accumulator together with the not of the carry bit. If overflow occurs 
         * the carry bit is clear, this enables multiple byte subtraction to be
         * performed.
         */
        private void SBC(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Set carry flag
         */ 
        private void SEC(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Set decimal flag
         */
        private void SED(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Set interrupt disable
         */
        private void SEI(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Store accumulator
         * 
         * Stores the contents of the accumulator into memory.
         */
        private void STA(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Store X register
         * 
         * Stores the contents of the X register into memory.
         */
        private void STX(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Store Y register
         * 
         * Stores the contents of the Y register into memory.
         */
        private void STY(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Transfer accumulator to X
         * 
         * Copies the current contents of the accumulator into the X 
         * register and sets the zero and negative flags as appropriate.
         */
        private void TAX(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Transfer accumulator to Y
         * 
         * Copies the current contents of the accumulator into the Y
         * register and sets the zero and negative flags as appropriate.
         */
        private void TAY(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Transfer stack pointer to X
         * 
         * Copies the current contents of the stack register into 
         * the X register and sets the zero and negative flags as appropriate.
         */
        private void TSX(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Transfer X to accumulator
         * 
         * Copies the current contents of the X register into the 
         * accumulator and sets the zero and negative flags as appropriate.
         */
        private void TXA(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Transfer X to stack pointer
         * 
         * Copies the current contents of the X register into the stack register.
         */
        private void TXS(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }

        /*
         * Transfer to accumulator
         * 
         * Copies the current contents of the Y register into the accumulator
         * and sets the zero and negative flags as appropriate.
         */
        private void TYA(ushort operandAddress, AddressingMode addressingMode)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region CPU state

        /* 
         * CPU state
         */

        // Program counter
        public ushort PC { get; set; }

        // Stack pointer
        public byte S { get; set; } = 0xFF;

        // Accumulator
        public byte A { get; set; }

        // Index register X
        public byte X { get; set; }

        // Index register Y
        public byte Y { get; set; }

        // Processor status
        public byte P { get; set; }

        [Flags]
        public enum PFlags : byte
        {
            C = 0x01, // Carry
            Z = 0x02, // Zero result
            I = 0x04, // Interrupt disable
            D = 0x08, // Decimal mode
            B = 0x10, // Break command
            X = 0x20, // Reserved for expansion
            V = 0x40, // Overflow
            N = 0x80  // Negative result
        }

        public bool PIsSet(PFlags flags)
        {
            return (P & (byte)flags) == (byte)flags;
        }

        public void PSet(PFlags flags)
        {
            P = (byte)(P | (byte)flags);
        }

        public void PReset(PFlags flags)
        {
            P = (byte)(P & ~(byte)flags);
        }

        public void PSet(PFlags flags, bool value)
        {
            if (value)
            {
                PSet(flags);
            }
            else
            {
                PReset(flags);
            }
        }

        private const ushort STACK_BASE_ADRESS = 0x100;
        private const ushort STACK_SIZE = 0X1000;

        public void PushByte(byte value)
        {
            if (S==0)
            {
                // TODO
                throw new Exception("Stack overflow");
            }
            Memory.SetByte(value, (ushort)(STACK_BASE_ADRESS + S));
            S--;
        }

        public byte PopByte()
        {
            if (S==0xFF)
            {
                // TODO
                throw new Exception("Stack underflow");
            }
            S++;
            return Memory.GetByte((ushort)(STACK_BASE_ADRESS + S));
        }

        public IAddressSpace Memory { get; }

        #endregion

        #region Addressing modes

        /*
         * Addressing modes
         */
        private readonly byte[] _addressingModeToPCDelta = new byte[]
        {
            0, // Accumulator,
            1, // Immediate,
            0, // Implied,
            1, // Relative,
            2, // Absolute,
            1, // ZeroPage,
            2, // Indirect,
            2, // AbsoluteIndexedX,
            2, // AbsoluteIndexedY,
            1, // ZeroPageIndexedX,
            1, // ZeroPageIndexedY,
            1, // IndexedXIndirect,
            1, // IndirectIndexedY
        };

        private ushort ResolveAddress(ushort operandAddress, AddressingMode addressingMode)
        {
            return addressingMode switch
            {
                AddressingMode.Accumulator => 0,
                AddressingMode.Immediate => operandAddress,
                AddressingMode.Implied => 0,
                AddressingMode.Relative => operandAddress,
                AddressingMode.Absolute => Memory.GetWord(operandAddress),
                AddressingMode.ZeroPage => Memory.GetByte(operandAddress),
                AddressingMode.Indirect => Memory.GetWord(Memory.GetWord(operandAddress)),
                AddressingMode.AbsoluteIndexedX => (ushort)(Memory.GetWord(operandAddress) + X),
                AddressingMode.AbsoluteIndexedY => (ushort)(Memory.GetWord(operandAddress) + Y),
                AddressingMode.ZeroPageIndexedX => (byte)(Memory.GetByte(operandAddress) + X), // Cast to byte to wrap to zero page
                AddressingMode.ZeroPageIndexedY => (byte)(Memory.GetByte(operandAddress) + Y), // Cast to byte to wrap to zero page
                AddressingMode.IndexedXIndirect => Memory.GetWord((byte)(Memory.GetByte(operandAddress) + X)), // Cast to byte to wrap to zero page
                AddressingMode.IndirectIndexedY => (ushort)(Memory.GetWord(Memory.GetByte(operandAddress)) + Y),
                _ => throw new Exception($"Invalid addressing mode '{addressingMode}'") // TODO
            };
        }
        #endregion

        public override string ToString()
        {
            return $"PC=0x{PC:X4}, S=0x{S:X2} A=0x{A:X2}, X=0x{X:X2}, Y=0x{Y:X2}, " +
                $"P=0x{P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => ((PIsSet(flag)) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})";
        }
    }
}