//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace BbcMicro.Cpu
//{
//    public sealed class XXXXCPU
//    {
//        #region Configuration

//        /*
//         * Set up
//        */

//        private sealed class ExecutionDefinition
//        {
//            private readonly List<(byte opCode, AddressingMode addressingMode)> _codeAndMode;

//            public ExecutionDefinition(OpCode opCode, Action<ushort, AddressingMode> implementation, List<(byte opCode, AddressingMode addressingMode)> codeAndMode)
//            {
//                OpCode = opCode;
//                Impl = implementation;
//                _codeAndMode = codeAndMode;
//            }

//            public OpCode OpCode { get; }

//            public Action<ushort, AddressingMode> Impl { get; }

//            public IReadOnlyList<(byte opCode, AddressingMode addressingMode)> CodeAndMode
//            {
//                get
//                {
//                    return _codeAndMode.AsReadOnly();
//                }
//            }
//        }

//        private (OpCode opCode, Action<ushort, AddressingMode> impl, AddressingMode addressingMode)[] _decoder =
//            new (OpCode opCode, Action<ushort, AddressingMode>, AddressingMode)[256];

//        private bool[] _opcodeIsValid = new bool[256];

//        private void PopulateDecoder()
//        {
//            foreach (var executionDefinition in _executionDefinitions)
//            {
//                foreach (var codeAndMode in executionDefinition.CodeAndMode)
//                {
//                    if (_opcodeIsValid[codeAndMode.opCode])
//                    {
//                        throw new Exception($"Duplicate op code {executionDefinition.OpCode} 0x{codeAndMode.opCode:X2}"); // TODO
//                    }
//                    else
//                    {
//                        _opcodeIsValid[codeAndMode.opCode] = true;
//                    }
//                    _decoder[codeAndMode.opCode] = (opCode: executionDefinition.OpCode, impl: executionDefinition.Impl, addressingMode: codeAndMode.addressingMode);
//                }
//            }
//        }

//        #endregion Configuration

//        private readonly List<ExecutionDefinition> _executionDefinitions;

//        public XXXXCPU(IAddressSpace addressSpace)
//        {
//            Memory = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));

//            #region Op code mappings

//            _executionDefinitions = new List<ExecutionDefinition>()
//            {
//                new ExecutionDefinition(OpCode.ADC, ADC,
//                    new List<(byte, AddressingMode)> {
//                        (0x69, AddressingMode.Immediate),
//                        (0x65, AddressingMode.ZeroPage),
//                        (0x75, AddressingMode.ZeroPageIndexedX),
//                        (0x6D, AddressingMode.Absolute),
//                        (0x7D, AddressingMode.AbsoluteIndexedX),
//                        (0x79, AddressingMode.AbsoluteIndexedY),
//                        (0x61, AddressingMode.IndexedXIndirect),
//                        (0x71, AddressingMode.IndirectIndexedY),
//                    }),
//                new ExecutionDefinition(OpCode.AND, AND,
//                    new List<(byte, AddressingMode)> {
//                        (0x29, AddressingMode.Immediate),
//                        (0x25, AddressingMode.ZeroPage),
//                        (0x35, AddressingMode.ZeroPageIndexedX),
//                        (0x2D, AddressingMode.Absolute),
//                        (0x3D, AddressingMode.AbsoluteIndexedX),
//                        (0x39, AddressingMode.AbsoluteIndexedY),
//                        (0x21, AddressingMode.IndexedXIndirect),
//                        (0x31, AddressingMode.IndirectIndexedY),
//                    }),
//                new ExecutionDefinition(OpCode.ASL, ASL,
//                    new List<(byte, AddressingMode)> {
//                        (0x0A, AddressingMode.Accumulator),
//                        (0x06, AddressingMode.ZeroPage),
//                        (0x16, AddressingMode.ZeroPageIndexedX),
//                        (0x0E, AddressingMode.Absolute),
//                        (0x1E, AddressingMode.AbsoluteIndexedX)
//                    }),
//                new ExecutionDefinition(OpCode.BCC, BCC,
//                    new List<(byte, AddressingMode)> {
//                        (0x90, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.BCS, BCS,
//                    new List<(byte, AddressingMode)> {
//                        (0xB0, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.BEQ, BEQ,
//                    new List<(byte, AddressingMode)> {
//                        (0xF0, AddressingMode.Relative)
//                    }),
//                 new ExecutionDefinition(OpCode.BIT, BIT,
//                    new List<(byte, AddressingMode)> {
//                        (0x24, AddressingMode.ZeroPage),
//                        (0x2C, AddressingMode.Absolute)
//                    }),
//                new ExecutionDefinition(OpCode.BMI, BMI,
//                    new List<(byte, AddressingMode)> {
//                        (0x30, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.BNE, BNE,
//                    new List<(byte, AddressingMode)> {
//                        (0xD0, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.BPL, BPL,
//                    new List<(byte, AddressingMode)> {
//                        (0x10, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.BRK, BRK,
//                    new List<(byte, AddressingMode)> {
//                        (0x00, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.BVC, BVC,
//                    new List<(byte, AddressingMode)> {
//                        (0x50, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.BVS, BVS,
//                    new List<(byte, AddressingMode)> {
//                        (0x70, AddressingMode.Relative)
//                    }),
//                new ExecutionDefinition(OpCode.CLC, CLC,
//                    new List<(byte, AddressingMode)> {
//                        (0x18, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.CLD, CLD,
//                    new List<(byte, AddressingMode)> {
//                        (0xD8, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.CLI, CLI,
//                    new List<(byte, AddressingMode)> {
//                        (0x58, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.CLV, CLV,
//                    new List<(byte, AddressingMode)> {
//                        (0xB8, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.CMP, CMP,
//                    new List<(byte, AddressingMode)> {
//                        (0xC9, AddressingMode.Immediate),
//                        (0xC5, AddressingMode.ZeroPage),
//                        (0xD5, AddressingMode.ZeroPageIndexedX),
//                        (0xCD, AddressingMode.Absolute),
//                        (0xDD, AddressingMode.AbsoluteIndexedX),
//                        (0xD9, AddressingMode.AbsoluteIndexedY),
//                        (0xC1, AddressingMode.IndexedXIndirect),
//                        (0xD1, AddressingMode.IndirectIndexedY)
//                    }),
//                new ExecutionDefinition(OpCode.CPX, CPX,
//                    new List<(byte, AddressingMode)> {
//                        (0xE0, AddressingMode.Immediate),
//                        (0xE4, AddressingMode.ZeroPage),
//                        (0xEC, AddressingMode.Absolute)
//                }),
//                new ExecutionDefinition(OpCode.CPY, CPY,
//                    new List<(byte, AddressingMode)> {
//                        (0xC0, AddressingMode.Immediate),
//                        (0xC4, AddressingMode.ZeroPage),
//                        (0xCC, AddressingMode.Absolute)
//                }),
//                new ExecutionDefinition(OpCode.DEC, DEC,
//                    new List<(byte, AddressingMode)> {
//                        (0xC6, AddressingMode.ZeroPage),
//                        (0xD6, AddressingMode.ZeroPageIndexedX),
//                        (0xCE, AddressingMode.Absolute),
//                        (0xDE, AddressingMode.AbsoluteIndexedX)
//                }),
//                new ExecutionDefinition(OpCode.DEX, DEX,
//                    new List<(byte, AddressingMode)> {
//                        (0xCA, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.DEY, DEY,
//                    new List<(byte, AddressingMode)> {
//                        (0x88, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.EOR, EOR,
//                    new List<(byte, AddressingMode)> {
//                        (0x49, AddressingMode.Immediate),
//                        (0x45, AddressingMode.ZeroPage),
//                        (0x55, AddressingMode.ZeroPageIndexedX),
//                        (0x4D, AddressingMode.Absolute),
//                        (0x5D, AddressingMode.AbsoluteIndexedX),
//                        (0x59, AddressingMode.AbsoluteIndexedY),
//                        (0x41, AddressingMode.IndexedXIndirect),
//                        (0x51, AddressingMode.IndirectIndexedY)
//                    }),
//                new ExecutionDefinition(OpCode.INC, INC,
//                    new List<(byte, AddressingMode)> {
//                        (0xE6, AddressingMode.ZeroPage),
//                        (0xF6, AddressingMode.ZeroPageIndexedX),
//                        (0xEE, AddressingMode.Absolute),
//                        (0xFE, AddressingMode.AbsoluteIndexedX)
//                    }),
//                new ExecutionDefinition(OpCode.INX, INX,
//                    new List<(byte, AddressingMode)> {
//                        (0xE8, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.INY, INY,
//                    new List<(byte, AddressingMode)> {
//                        (0xC8, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.JMP, JMP,
//                    new List<(byte, AddressingMode)> {
//                        (0x4C, AddressingMode.Absolute),
//                        (0x6C, AddressingMode.Indirect)
//                    }),
//                new ExecutionDefinition(OpCode.JSR, JSR,
//                    new List<(byte, AddressingMode)> {
//                        (0x20, AddressingMode.Absolute)
//                    }),
//                new ExecutionDefinition(OpCode.LDA, LDA,
//                    new List<(byte, AddressingMode)> {
//                        (0xA9, AddressingMode.Immediate),
//                        (0xA5, AddressingMode.ZeroPage),
//                        (0xB5, AddressingMode.ZeroPageIndexedX),
//                        (0xAD, AddressingMode.Absolute),
//                        (0xBD, AddressingMode.AbsoluteIndexedX),
//                        (0xB9, AddressingMode.AbsoluteIndexedY),
//                        (0xA1, AddressingMode.IndexedXIndirect),
//                        (0xB1, AddressingMode.IndirectIndexedY)
//                    }),
//                new ExecutionDefinition(OpCode.LDX, LDX,
//                    new List<(byte, AddressingMode)> {
//                        (0xA2, AddressingMode.Immediate),
//                        (0xA6, AddressingMode.ZeroPage),
//                        (0xB6, AddressingMode.ZeroPageIndexedY),
//                        (0xAE, AddressingMode.Absolute),
//                        (0xBE, AddressingMode.AbsoluteIndexedY)
//                    }),
//                new ExecutionDefinition(OpCode.LDY, LDY,
//                    new List<(byte, AddressingMode)> {
//                        (0xA0, AddressingMode.Immediate),
//                        (0xA4, AddressingMode.ZeroPage),
//                        (0xB4, AddressingMode.ZeroPageIndexedX),
//                        (0xAC, AddressingMode.Absolute),
//                        (0xBC, AddressingMode.AbsoluteIndexedX)
//                    }),
//                new ExecutionDefinition(OpCode.LSR, LSR,
//                    new List<(byte, AddressingMode)> {
//                        (0x4A, AddressingMode.Accumulator),
//                        (0x46, AddressingMode.ZeroPage),
//                        (0x56, AddressingMode.ZeroPageIndexedX),
//                        (0x4E, AddressingMode.Absolute),
//                        (0x5E, AddressingMode.AbsoluteIndexedX)
//                    }),
//                new ExecutionDefinition(OpCode.NOP, NOP,
//                    new List<(byte, AddressingMode)> {
//                        (0xEA, AddressingMode.Implied)
//                    }),
//                new ExecutionDefinition(OpCode.ORA, ORA,
//                    new List<(byte, AddressingMode)> {
//                        (0x09, AddressingMode.Immediate),
//                        (0x05, AddressingMode.ZeroPage),
//                        (0x15, AddressingMode.ZeroPageIndexedX),
//                        (0x0D, AddressingMode.Absolute),
//                        (0x1D, AddressingMode.AbsoluteIndexedX),
//                        (0x19, AddressingMode.AbsoluteIndexedY),
//                        (0x01, AddressingMode.IndexedXIndirect),
//                        (0x11, AddressingMode.IndirectIndexedY)
//                    }),
//                 new ExecutionDefinition(OpCode.PHA, PHA,
//                    new List<(byte, AddressingMode)> {
//                        (0x48, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.PHP, PHP,
//                    new List<(byte, AddressingMode)> {
//                        (0x08, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.PLA, PLA,
//                    new List<(byte, AddressingMode)> {
//                        (0x68, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.PLP, PLP,
//                    new List<(byte, AddressingMode)> {
//                        (0x28, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.ROL, ROL,
//                    new List<(byte, AddressingMode)> {
//                        (0x2A, AddressingMode.Accumulator),
//                        (0x26, AddressingMode.ZeroPage),
//                        (0x36, AddressingMode.ZeroPageIndexedX),
//                        (0x2E, AddressingMode.Absolute),
//                        (0x3E, AddressingMode.AbsoluteIndexedX)
//                    }),
//                 new ExecutionDefinition(OpCode.ROR, ROR,
//                    new List<(byte, AddressingMode)> {
//                        (0x6A, AddressingMode.Accumulator),
//                        (0x66, AddressingMode.ZeroPage),
//                        (0x76, AddressingMode.ZeroPageIndexedX),
//                        (0x6E, AddressingMode.Absolute),
//                        (0x7E, AddressingMode.AbsoluteIndexedX)
//                    }),
//                 new ExecutionDefinition(OpCode.RTI, RTI,
//                    new List<(byte, AddressingMode)> {
//                        (0x40, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.RTS, RTS,
//                    new List<(byte, AddressingMode)> {
//                        (0x60, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.SBC, SBC,
//                    new List<(byte, AddressingMode)> {
//                        (0xE9, AddressingMode.Immediate),
//                        (0xE5, AddressingMode.ZeroPage),
//                        (0xF5, AddressingMode.ZeroPageIndexedX),
//                        (0xED, AddressingMode.Absolute),
//                        (0xFD, AddressingMode.AbsoluteIndexedX),
//                        (0xF9, AddressingMode.AbsoluteIndexedY),
//                        (0xE1, AddressingMode.IndexedXIndirect),
//                        (0xF1, AddressingMode.IndirectIndexedY),
//                    }),
//                 new ExecutionDefinition(OpCode.SEC, SEC,
//                    new List<(byte, AddressingMode)> {
//                        (0x38, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.SED, SED,
//                    new List<(byte, AddressingMode)> {
//                        (0xF8, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.SEI, SEI,
//                    new List<(byte, AddressingMode)> {
//                        (0x78, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.STA, STA,
//                    new List<(byte, AddressingMode)> {
//                        (0x85, AddressingMode.ZeroPage),
//                        (0x95, AddressingMode.ZeroPageIndexedX),
//                        (0x8D, AddressingMode.Absolute),
//                        (0x9D, AddressingMode.AbsoluteIndexedX),
//                        (0x99, AddressingMode.AbsoluteIndexedY),
//                        (0x81, AddressingMode.IndexedXIndirect),
//                        (0x91, AddressingMode.IndirectIndexedY)
//                    }),
//                 new ExecutionDefinition(OpCode.STX, STX,
//                    new List<(byte, AddressingMode)> {
//                        (0x86, AddressingMode.ZeroPage),
//                        (0x96, AddressingMode.ZeroPageIndexedY),
//                        (0x8E, AddressingMode.Absolute)
//                    }),
//                 new ExecutionDefinition(OpCode.STY, STY,
//                    new List<(byte, AddressingMode)> {
//                        (0x84, AddressingMode.ZeroPage),
//                        (0x94, AddressingMode.ZeroPageIndexedX),
//                        (0x8C, AddressingMode.Absolute)
//                    }),
//                 new ExecutionDefinition(OpCode.TAX, TAX,
//                    new List<(byte, AddressingMode)> {
//                        (0xAA, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.TAY, TAY,
//                    new List<(byte, AddressingMode)> {
//                        (0xA8, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.TSX, TSX,
//                    new List<(byte, AddressingMode)> {
//                        (0xBA, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.TXA, TXA,
//                    new List<(byte, AddressingMode)> {
//                        (0x8A, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.TXS, TXS,
//                    new List<(byte, AddressingMode)> {
//                        (0x9A, AddressingMode.Implied)
//                    }),
//                 new ExecutionDefinition(OpCode.TYA, TYA,
//                    new List<(byte, AddressingMode)> {
//                        (0x98, AddressingMode.Implied)
//                    })
//            };

//            #endregion Op code mappings

//            PopulateDecoder();
//        }

//        #region Callbacks

//        /*
//         * Callbacks
//         */

//        private Func<CPU, OpCode, AddressingMode, ushort, bool> _interceptionCallback =
//            (cpu, opcode, addressingMode, resolvedAddress) => true;

//        private readonly List<Action<CPU, OpCode, AddressingMode>> _preExecutionCallbacks = new List<Action<CPU, OpCode, AddressingMode>>();

//        private Action<string, CPU> _errorCallback = null;

//        public void SetInterceptionCallback(Func<CPU, OpCode, AddressingMode, ushort, bool> callback)
//        {
//            _interceptionCallback = callback;
//        }

//        public void AddPreExecutionCallback(Action<CPU, OpCode, AddressingMode> callback)
//        {
//            _preExecutionCallbacks.Add(callback);
//        }

//        public void SetErrorCallback(Action<string, CPU> callback)
//        {
//            _errorCallback = callback;
//        }

//        #endregion Callbacks

//        #region Execution

//        // TODO BUG JSR Wil be putting wrong value on stack as we are not adjusting the PC before calling the routine!  It needs to add two!
//        // Issue with Branches to as relative is wrong - hacked BEQ to work!

//        /*
//         * Execution
//         */

//        public bool ExecuteNextInstruction()
//        {
//            Console.WriteLine("In Exec");
//            var result = true;

//            // Grab to opCode at PC
//            byte opCode = Memory.GetByte(PC);

//            // Is this a valid op code?
//            if (_opcodeIsValid[opCode])
//            {
//                Console.WriteLine("Valid");


//                // Find the decoder entry for the instruction
//                var decoded = _decoder[opCode];

//                // Calculate the actual operand using the addressing mode
//                var resolvedOperand = ResolveOperand((ushort)(PC+1), decoded.addressingMode);

//                // Run all the pre-execution callbacks
//                _preExecutionCallbacks.ForEach(pec => pec(this, decoded.opCode, decoded.addressingMode));

//                var oldPC = PC;

//                // Interception callback
//                if (_interceptionCallback(this, decoded.opCode, decoded.addressingMode, resolvedOperand))
//                {
                                                    
//                    PC += (byte)(_addressingModeToPCDelta[(ushort)decoded.addressingMode] + 1);

//                    // Interpret the instruction
//                    decoded.impl(resolvedOperand, decoded.addressingMode);
//                }
//                else
//                {
//                    if (oldPC==PC)
//                    {
//                        PC += (byte)(_addressingModeToPCDelta[(ushort)decoded.addressingMode] + 1);
//                    }
//                }
//                if (PC == oldPC) {
//                    Console.WriteLine("Adding to CP" + (byte)(_addressingModeToPCDelta[(ushort)decoded.addressingMode] + 1));

//                    // Move the PC forward based on the addressing mode
//                    PC += (byte)(_addressingModeToPCDelta[(ushort)decoded.addressingMode] + 1);
//                }

//                // Call any post-execution callbacks
//                //_postExecutionCallbacks.ForEach(cb => cb(this));
//            }
//            else
//            {
//                result = false;
//                Console.WriteLine($"Invalid opcode 0x{opCode:X2}");
//                if (_errorCallback != null)
//                {
//                    _errorCallback($"Invalid op code 0x{opCode:X2} at address 0x{PC:X2}", this);
//                }
//            }

//            return result;
//        }

//        public void ExecuteToBrk()
//        {
//            while (Memory.GetByte(PC) != 0x00)
//            {
//                if (!ExecuteNextInstruction())
//                {
//                    throw new CPUException(this);
//                }
//            }
//        }

//        #endregion Execution

//        #region Opcode implementations

//        /*
//         * Op code implementations
//         *
//         * Definitions are taken from http://www.obelisk.me.uk/6502/reference.html
//         */

//        private void UpdateFlags(byte accordingToValue, PFlags flagsToUpdate)
//        {
//            if (flagsToUpdate.HasFlag(PFlags.Z))
//            {
//                PSet(PFlags.Z, accordingToValue == 0);
//            }

//            if (flagsToUpdate.HasFlag(PFlags.N))
//            {
//                PSet(PFlags.N, (accordingToValue & 0b10000000) != 0);
//            }
//        }

//        /*
//         * Add with carry
//         *
//         * This instruction adds the contents of a memory location to the accumulator
//         * together with the carry bit. If overflow occurs the carry bit is set,
//         * this enables multiple byte addition to be performed.
//         */

//        private void ADC(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            short result = (short)(A + operandValue + (PIsSet(PFlags.C) ? 1 : 0));
//            PSet(PFlags.C, result > 0xFF);
//            PSet(PFlags.V, ((A ^ result) & (operandValue ^ result) & 0x80) != 0);
//            A = (byte)result;
//            UpdateFlags(A, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Logical and
//         *
//         * A logical AND is performed, bit by bit, on the accumulator contents
//         * using the contents of a byte of memory.
//         */

//        private void AND(ushort operand, AddressingMode addressingMode)
//        {
//            A &= GetByteValue(operand, addressingMode);
//            UpdateFlags(A, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Arithmetic shift left
//         *
//         * This operation shifts all the bits of the accumulator or memory contents one
//         * bit left. Bit 0 is set to 0 and bit 7 is placed in the carry flag. The
//         * effect of this operation is to multiply the memory contents by 2
//         * (ignoring 2's complement considerations), setting the carry if the result
//         * will not fit in 8 bits.
//         */

//        private void ASL(ushort operand, AddressingMode addressingMode)
//        {
//            if (addressingMode == AddressingMode.Accumulator)
//            {
//                PSet(PFlags.C, (A & 0b1000_0000) != 0);
//                A = (byte)(A << 1);
//                UpdateFlags(A, PFlags.N | PFlags.Z);
//            }
//            else
//            {
//                var operandValue = GetByteValue(operand, addressingMode);
//                Memory.SetByte((byte)(operandValue << 1), operand);
//                PSet(PFlags.C, (operandValue & 0b1000_0000) != 0);
//                UpdateFlags(operandValue, PFlags.N | PFlags.Z);
//            }
//        }

//        /*
//         * Branch if carry clear
//         *
//         * If the carry flag is clear then add the relative displacement to the
//         * program counter to cause a branch to a new location.
//         */

//        private void BCC(ushort operand, AddressingMode addressingMode)
//        {
//            if (!PIsSet(PFlags.C))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Branch if carry set
//         *
//         * If the carry flag is set then add the relative displacement to the
//         * program counter to cause a branch to a new location.
//         */

//        private void BCS(ushort operand, AddressingMode addressingMode)
//        {
//            if (PIsSet(PFlags.C))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Branch if equal
//         *
//         * If the zero flag is set then add the relative displacement to
//         * the program counter to cause a branch to a new location.
//         */

//        private void BEQ(ushort operand, AddressingMode addressingMode)
//        {
//            if (PIsSet(PFlags.Z))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Bit test
//         *
//         * This instructions is used to test if one or more bits are set
//         * in a target memory location. The mask pattern in A is ANDed with
//         * the value in memory to set or clear the zero flag, but the result
//         * is not kept. Bits 7 and 6 of the value from memory are copied into
//         * the N and V flags.
//         */

//        private void BIT(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            PSet(PFlags.Z, (A & operandValue) == 0);
//            PSet(PFlags.V, (operandValue & 0b0100_0000) != 0);
//            PSet(PFlags.N, (operandValue & 0b1000_0000) != 0);
//        }

//        /*
//         * Branch if minus
//         *
//         * If the negative flag is set then add the relative displacement to
//         * the program counter to cause a branch to a new location.
//         */

//        private void BMI(ushort operand, AddressingMode addressingMode)
//        {
//            if (PIsSet(PFlags.N))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Branch if not equal
//         *
//         * If the zero flag is clear then add the relative displacement
//         * to the program counter to cause a branch to a new location.
//         */

//        private void BNE(ushort operand, AddressingMode addressingMode)
//        {
//            if (!PIsSet(PFlags.Z))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Branch if positive
//         *
//         * If the negative flag is clear then add the relative displacement
//         * to the program counter to cause a branch to a new location.
//         */

//        private void BPL(ushort operand, AddressingMode addressingMode)
//        {
//            if (!PIsSet(PFlags.N))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Force Interrupt
//         *
//         * The BRK instruction forces the generation of an interrupt request.
//         * The program counter and processor status are pushed on the stack then
//         * the IRQ interrupt vector at $FFFE/F is loaded into the PC and the break
//         * flag in the status set to one.
//         *
//         * The interpretation of a BRK depends on the operating system. On the BBC
//         * Microcomputer it is used by language ROMs to signal run time errors but
//         * it could be used for other purposes (e.g. calling operating system
//         * functions, etc.).
//         */

//        private void BRK(ushort operandAddress, AddressingMode addressingMode)
//        {
//            // TODO
//            throw new NotImplementedException("BRK");
//        }

//        /*
//         * Branch if overflow clear
//         *
//         * If the overflow flag is clear then add the relative displacement to
//         * the program counter to cause a branch to a new location.
//         */

//        private void BVC(ushort operand, AddressingMode addressingMode)
//        {
//            if (!PIsSet(PFlags.V))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Branch if overflow set
//         *
//         * If the overflow flag is set then add the relative displacement to
//         * the program counter to cause a branch to a new location.
//         */

//        private void BVS(ushort operand, AddressingMode addressingMode)
//        {
//            if (PIsSet(PFlags.V))
//            {
//                // Cast to sbyte as the offset is signed
//                PC = (ushort)(PC + (sbyte)GetByteValue(operand, addressingMode));
//            }
//        }

//        /*
//         * Clear carry
//         */

//        private void CLC(ushort operandAddress, AddressingMode addressingMode)
//        {
//            PReset(PFlags.C);
//        }

//        /*
//         * Clear decimal mode
//         */

//        private void CLD(ushort operandAddress, AddressingMode addressingMode)
//        {
//            PReset(PFlags.D);
//        }

//        /*
//         * Clear interrupt disable
//         *
//         * Clears the interrupt disable flag allowing normal interrupt requests
//         * to be serviced.
//         */

//        private void CLI(ushort operandAddress, AddressingMode addressingMode)
//        {
//            PReset(PFlags.I);
//        }

//        /*
//         * Clear overflow flag
//         */

//        private void CLV(ushort operandAddress, AddressingMode addressingMode)
//        {
//            PReset(PFlags.V);
//        }

//        /*
//         * Compare
//         *
//         * This instruction compares the contents of the accumulator with another
//         * memory held value and sets the zero and carry flags as appropriate.
//         */

//        private void CMP(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            PSet(PFlags.N, A < operandValue);
//            PSet(PFlags.C, A >= operandValue);
//            PSet(PFlags.Z, A == operandValue);
//        }

//        /*
//         * Compare X register
//         *
//         * This instruction compares the contents of the X register with another
//         * memory held value and sets the zero and carry flags as appropriate.
//         */

//        private void CPX(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            PSet(PFlags.N, X < operandValue);
//            PSet(PFlags.C, X >= operandValue);
//            PSet(PFlags.Z, X == operandValue);
//        }

//        /*
//         * Compare Y register
//         *
//         * This instruction compares the contents of the Y register with another
//         * memory held value and sets the zero and carry flags as appropriate.
//         */

//        private void CPY(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            PSet(PFlags.N, Y < operandValue);
//            PSet(PFlags.C, Y >= operandValue);
//            PSet(PFlags.Z, Y == operandValue);
//        }

//        /*
//         * Decrement memory
//         *
//         * Subtracts one from the value held at a specified memory location
//         * setting the zero and negative flags as appropriate.
//         */

//        private void DEC(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            byte result = (byte)(operandValue - 1);
//            Memory.SetByte(result, operand);
//            UpdateFlags(result, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Decrement X register
//         *
//         * Subtracts one from the X register setting the zero and negative
//         * flags as appropriate.
//         */

//        private void DEX(ushort operandAddress, AddressingMode addressingMode)
//        {
//            X -= 1;
//            UpdateFlags(X, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Decrement Y register
//         *
//         * Subtracts one from the Y register setting the zero and negative
//         * flags as appropriate.
//         */

//        private void DEY(ushort operandAddress, AddressingMode addressingMode)
//        {
//            Y -= 1;
//            UpdateFlags(Y, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Exclusive or
//         *
//         * An exclusive OR is performed, bit by bit, on the accumulator
//         * contents using the contents of a byte of memory.
//         */

//        private void EOR(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            A ^= operandValue;
//            UpdateFlags(A, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Increment memory
//         *
//         * Adds one to the value held at a specified memory location
//         * setting the zero and negative flags as appropriate.
//         */

//        private void INC(ushort operand, AddressingMode addressingMode)
//        {
//            byte operandValue = GetByteValue(operand, addressingMode);
//            operandValue++;
//            Memory.SetByte(operandValue, operand);
//            UpdateFlags(operandValue, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Increment X register
//         *
//         * Adds one to the X register setting the zero and negative flags
//         * as appropriate.
//         */

//        private void INX(ushort operand, AddressingMode addressingMode)
//        {
//            X++;
//            UpdateFlags(X, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Increment Y register
//         *
//         * Adds one to the X register setting the zero and negative flags
//         * as appropriate.
//         */

//        private void INY(ushort operand, AddressingMode addressingMode)
//        {
//            Y++;
//            UpdateFlags(Y, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Jump
//         *
//         * Sets the program counter to the address specified by the operand.
//         *
//         * An original 6502 has does not correctly fetch the target address if
//         * the indirect vector falls on a page boundary (e.g. $xxFF where xx is
//         * any value from $00 to $FF). In this case fetches the LSB from $xxFF
//         * as expected but takes the MSB from $xx00. This is fixed in some later
//         * chips like the 65SC02 so for compatibility always ensure the indirect
//         * vector is not at the end of the page.
//         */

//        private void JMP(ushort operand, AddressingMode addressingMode)
//        {
//            PC = operand;
//        }

//        /*
//         * Jump to Subroutine
//         *
//         * The JSR instruction pushes the address (minus one) of the return point
//         * on to the stack and then sets the program counter to the target memory
//         * address.
//         */

//        private void JSR(ushort operand, AddressingMode addressingMode)
//        {
//            // Operand is in host machine byte order, it is always the absolute address to target
//            ushort addressToPush = (ushort)(PC - 1); // This is what the 6502 does!
//            byte operandLSB = (byte)addressToPush;
//            byte operandMSB = (byte)(addressToPush >> 8);

//            // Push the return address onto the stack
//            // MSB first to match little endian byte order as the stack grows downwards
//            PushByte(operandMSB);
//            PushByte(operandLSB);

//            PC = operand;
//        }

//        /*
//         * Load accumulator
//         *
//         * Loads a byte of memory into the accumulator setting the
//         * zero and negative flags as appropriate.
//         */

//        private void LDA(ushort operand, AddressingMode addressingMode)
//        {
//            A = GetByteValue(operand, addressingMode);
//            UpdateFlags(A, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Load X register
//         *
//         * Loads a byte of memory into the X register setting the zero
//         * and negative flags as appropriate.
//         */

//        private void LDX(ushort operand, AddressingMode addressingMode)
//        {
//            X = GetByteValue(operand, addressingMode);
//            UpdateFlags(X, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Load Y register
//         *
//         * Loads a byte of memory into the Y register setting the zero
//         * and negative flags as appropriate.
//         */

//        private void LDY(ushort operand, AddressingMode addressingMode)
//        {
//            Y = GetByteValue(operand, addressingMode);
//            UpdateFlags(Y, PFlags.N | PFlags.Z);
//        }

//        /*
//         * Logical shift right
//         *
//         * Each of the bits in A or M is shift one place to the right. The
//         * bit that was in bit 0 is shifted into the carry flag. Bit 7 is set
//         * to zero.
//         */

//        private void LSR(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * No operation
//         *
//         * The NOP instruction causes no changes to the processor other
//         * than the normal incrementing of the program counter to the next
//         * instruction.
//         */

//        private void NOP(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Logical inclusive or
//         *
//         * An inclusive OR is performed, bit by bit, on the accumulator
//         * contents using the contents of a byte of memory.
//         */

//        private void ORA(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Push accumulator
//         *
//         * Pushes a copy of the accumulator on to the stack.
//         */

//        private void PHA(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Push processor status
//         *
//         * Pushes a copy of the status flags on to the stack.
//         */

//        private void PHP(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Pull accumulator
//         *
//         * Pulls an 8 bit value from the stack and into the accumulator.
//         * The zero and negative flags are set as appropriate.
//         */

//        private void PLA(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Pull processor status
//         *
//         * Pulls an 8 bit value from the stack and into the processor flags.
//         * The flags will take on new states as determined by the value pulled.
//         */

//        private void PLP(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Rotate left
//         *
//         * Move each of the bits in either A or M one place to the left. Bit 0
//         * is filled with the current value of the carry flag whilst the old
//         * bit 7 becomes the new carry flag value.
//         */

//        private void ROL(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Rotate right
//         *
//         * Move each of the bits in either A or M one place to the right. Bit 7
//         * is filled with the current value of the carry flag whilst the old
//         * bit 0 becomes the new carry flag value.
//         */

//        private void ROR(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Return from interrupt
//         *
//         * The RTI instruction is used at the end of an interrupt processing
//         * routine. It pulls the processor flags from the stack followed by
//         * the program counter.
//         */

//        private void RTI(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Return from subroutine
//         *
//         * The RTS instruction is used at the end of a subroutine to return
//         * to the calling routine. It pulls the program counter (minus one) from the stack.
//         */

//        private void RTS(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Subtract with carry
//         *
//         * This instruction subtracts the contents of a memory location to the
//         * accumulator together with the not of the carry bit. If overflow occurs
//         * the carry bit is clear, this enables multiple byte subtraction to be
//         * performed.
//         */

//        private void SBC(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Set carry flag
//         */

//        private void SEC(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Set decimal flag
//         */

//        private void SED(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Set interrupt disable
//         */

//        private void SEI(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Store accumulator
//         *
//         * Stores the contents of the accumulator into memory.
//         */

//        private void STA(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Store X register
//         *
//         * Stores the contents of the X register into memory.
//         */

//        private void STX(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Store Y register
//         *
//         * Stores the contents of the Y register into memory.
//         */

//        private void STY(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Transfer accumulator to X
//         *
//         * Copies the current contents of the accumulator into the X
//         * register and sets the zero and negative flags as appropriate.
//         */

//        private void TAX(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Transfer accumulator to Y
//         *
//         * Copies the current contents of the accumulator into the Y
//         * register and sets the zero and negative flags as appropriate.
//         */

//        private void TAY(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Transfer stack pointer to X
//         *
//         * Copies the current contents of the stack register into
//         * the X register and sets the zero and negative flags as appropriate.
//         */

//        private void TSX(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Transfer X to accumulator
//         *
//         * Copies the current contents of the X register into the
//         * accumulator and sets the zero and negative flags as appropriate.
//         */

//        private void TXA(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Transfer X to stack pointer
//         *
//         * Copies the current contents of the X register into the stack register.
//         */

//        private void TXS(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        /*
//         * Transfer to accumulator
//         *
//         * Copies the current contents of the Y register into the accumulator
//         * and sets the zero and negative flags as appropriate.
//         */

//        private void TYA(ushort operandAddress, AddressingMode addressingMode)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion Opcode implementations

//        #region CPU state

//        /*
//         * CPU state
//         */

//        // Program counter
//        public ushort PC { get; set; }

//        // Stack pointer
//        public byte S { get; set; } = 0xFF;

//        // Accumulator
//        public byte A { get; set; }

//        // Index register X
//        public byte X { get; set; }

//        // Index register Y
//        public byte Y { get; set; }

//        // Processor status
//        public byte P { get; set; }

//        [Flags]
//        public enum PFlags : byte
//        {
//            C = 0x01, // Carry
//            Z = 0x02, // Zero result
//            I = 0x04, // Interrupt disable
//            D = 0x08, // Decimal mode
//            B = 0x10, // Break command
//            X = 0x20, // Reserved for expansion
//            V = 0x40, // Overflow
//            N = 0x80  // Negative result
//        }

//        public bool PIsSet(PFlags flags)
//        {
//            return (P & (byte)flags) == (byte)flags;
//        }

//        public void PSet(PFlags flags)
//        {
//            P = (byte)(P | (byte)flags);
//        }

//        public void PReset(PFlags flags)
//        {
//            P = (byte)(P & ~(byte)flags);
//        }

//        public void PSet(PFlags flags, bool value)
//        {
//            if (value)
//            {
//                PSet(flags);
//            }
//            else
//            {
//                PReset(flags);
//            }
//        }

//        private const ushort STACK_BASE_ADRESS = 0x100;
//        private const ushort STACK_SIZE = 0X1000;

//        public void PushByte(byte value)
//        {
//            if (S == 0)
//            {
//                // TODO
//                throw new Exception("Stack overflow");
//            }
//            Memory.SetByte(value, (ushort)(STACK_BASE_ADRESS + S));
//            S--;
//        }

//        public byte PopByte()
//        {
//            if (S == 0xFF)
//            {
//                // TODO
//                throw new Exception("Stack underflow");
//            }
//            S++;
//            return Memory.GetByte((ushort)(STACK_BASE_ADRESS + S));
//        }

//        public IAddressSpace Memory { get; }

//        #endregion CPU state

//        #region Addressing modes

//        /*
//         * Addressing modes
//         */

//        public readonly byte[] _addressingModeToPCDelta = new byte[]
//        {
//            0, // Accumulator,
//            1, // Immediate,
//            0, // Implied,
//            1, // Relative,
//            2, // Absolute,
//            1, // ZeroPage,
//            2, // Indirect,
//            2, // AbsoluteIndexedX,
//            2, // AbsoluteIndexedY,
//            1, // ZeroPageIndexedX,
//            1, // ZeroPageIndexedY,
//            1, // IndexedXIndirect,
//            1, // IndirectIndexedY
//        };

//        // Follows a value returned by ResolveOperand() to the actual value to process
//        private byte GetByteValue(ushort operand, AddressingMode addressingMode)
//        {
//            return addressingMode switch
//            {
//                AddressingMode.Accumulator => A,
//                AddressingMode.Immediate => (byte)operand,
//                AddressingMode.Implied => 0,
//                AddressingMode.Relative => (byte)operand,
//                AddressingMode.Absolute => Memory.GetByte(operand),
//                AddressingMode.ZeroPage => Memory.GetByte(operand),
//                AddressingMode.Indirect => Memory.GetByte(operand),
//                AddressingMode.AbsoluteIndexedX => Memory.GetByte(operand),
//                AddressingMode.AbsoluteIndexedY => Memory.GetByte(operand),
//                AddressingMode.ZeroPageIndexedX => Memory.GetByte(operand),
//                AddressingMode.ZeroPageIndexedY => Memory.GetByte(operand),
//                AddressingMode.IndexedXIndirect => Memory.GetByte(operand),
//                AddressingMode.IndirectIndexedY => Memory.GetByte(operand),
//                _ => throw new Exception($"Invalid addressing mode '{addressingMode}'") // TODO
//            };
//        }

//        // Resolves the address of the operand to the actual operand value that an instruction would process.
//        // Results are always in host byte order (which may or may not match 6502 byte ordering)
//        // When the operand is byte it is returned in the least significant byte of the result
//        private ushort ResolveOperand(ushort operandAddress, AddressingMode addressingMode)
//        {
//            return addressingMode switch
//            {
//                AddressingMode.Accumulator => 0, // Operand ignored
//                AddressingMode.Immediate => Memory.GetByte(operandAddress), // Operand is the byte following the op code
//                AddressingMode.Implied => 0, // Operand ignored
//                AddressingMode.Relative => Memory.GetByte(operandAddress), // Operand is the byte following the op code
//                AddressingMode.Absolute => Memory.GetWord(operandAddress), // Operand is the word following the op code
//                AddressingMode.ZeroPage => Memory.GetByte(operandAddress), // Operand is the byte following the op code
//                AddressingMode.Indirect => Memory.GetWord(Memory.GetWord(operandAddress)), // Operand is pointed to by the word following the op code
//                AddressingMode.AbsoluteIndexedX => (ushort)(Memory.GetWord(operandAddress) + X), // Operand is the word following the op code + X
//                AddressingMode.AbsoluteIndexedY => (ushort)(Memory.GetWord(operandAddress) + Y), // Operand is the word following the op code + Y
//                AddressingMode.ZeroPageIndexedX => (byte)(Memory.GetByte(operandAddress) + X), // Cast to byte to wrap to zero page
//                AddressingMode.ZeroPageIndexedY => (byte)(Memory.GetByte(operandAddress) + Y), // Cast to byte to wrap to zero page
//                AddressingMode.IndexedXIndirect => Memory.GetWord((byte)(Memory.GetByte(operandAddress) + X)), // Cast to byte to wrap to zero page
//                AddressingMode.IndirectIndexedY => (ushort)(Memory.GetWord(Memory.GetByte(operandAddress)) + Y),
//                _ => throw new Exception($"Invalid addressing mode '{addressingMode}'") // TODO
//            };
//        }

//        #endregion Addressing modes

//        public override string ToString()
//        {
//            return $"PC=0x{PC:X4}, S=0x{S:X2} A=0x{A:X2}, X=0x{X:X2}, Y=0x{Y:X2}, " +
//                $"P=0x{P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => ((PIsSet(flag)) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})";
//        }
//    }
//}