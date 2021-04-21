using System;
using System.Collections.Generic;
using System.Linq;

namespace BbcMicro.Cpu
{
    public sealed class CPU
    {
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

        private readonly List<Action<CPU>> _preExecutionCallbacks = new List<Action<CPU>>();
        private readonly List<Action<CPU>> _postExecutionCallbacks = new List<Action<CPU>>();
        private Action<string, CPU> _errorCallback = null;

        public IAddressSpace Memory { get; }

        private readonly List<ExecutionDefinition> _executionDefinitions;

        private (Action<ushort, AddressingMode> impl, AddressingMode addressingMode)[] _decoder = new (Action<ushort, AddressingMode>, AddressingMode)[256];
        private bool[] _opcodeIsValid = new bool[256];

        private void PopulateDecoder()
        {
            var validation = new bool[246];

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

        public CPU(IAddressSpace addressSpace)
        {
            Memory = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));

            _executionDefinitions = new List<ExecutionDefinition>()
            {
                new ExecutionDefinition(LDA,
                    new List<(byte, AddressingMode)> {
                        (0xA9, AddressingMode.Immediate),
                        (0xA5, AddressingMode.ZeroPage),
                        (0xB5, AddressingMode.ZeroPageIndexedX),
                        (0xAD, AddressingMode.Absolute),
                        (0xBD, AddressingMode.AbsoluteIndexedX),
                        (0xB9, AddressingMode.AbsoluteIndexedY),
                        (0xA1, AddressingMode.IndexedXIndirect),
                        (0xB1, AddressingMode.IndirectIndexedY),
                    }),
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
                    })
            };

            PopulateDecoder();
        }

        // Load accumulator
        private void LDA(ushort operandAddress, AddressingMode addressingMode)
        {
            A = Memory.GetByte(operandAddress);
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        // Add with carry
        private void ADC(ushort operandAddress, AddressingMode addressingMode)
        {
            byte adcOperandByte = Memory.GetByte(operandAddress);
            short addWithCarryResult = (short)(A + adcOperandByte + (PIsSet(PFlags.C) ? 1 : 0));
            PSet(PFlags.C, addWithCarryResult > 0xFF);
            PSet(PFlags.V, ((A ^ addWithCarryResult) & (adcOperandByte ^ addWithCarryResult) & 0x80) != 0);
            A = (byte)addWithCarryResult;
            UpdateFlags(A, PFlags.N | PFlags.Z);
        }

        public void AddPreExecutionCallback(Action<CPU> callback)
        {
            _preExecutionCallbacks.Add(callback);
        }

        public void AddPostExecutionCallback(Action<CPU> callback)
        {
            _postExecutionCallbacks.Add(callback);
        }

        public void SetErrorCallback(Action<CPU> callback)
        {
            _errorCallback = callback;
        }

        // Program counter
        public ushort PC { get; set; }

        // Stack pointer
        public byte S { get; set; }

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

        /*
         * Execution
         */

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

        public override string ToString()
        {
            return $"PC=0x{PC:X4}, S=0x{S:X2} A=0x{A:X2}, X=0x{X:X2}, Y=0x{Y:X2}, " +
                $"P=0x{P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => ((PIsSet(flag)) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})";
        }
    }
}