using System;
using System.Collections.Generic;
using System.Text;

namespace BbcMicro.Cpu
{
    public sealed class Decoder
    {
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

        private sealed class DecoderDefinition
        {
            public DecoderDefinition(OpCode opCode, List<(byte opCodeByte, AddressingMode addressingMode)> codesAndModes)
            {
                OpCode = opCode;
                CodesAndModes = codesAndModes;
            }

            public OpCode OpCode { get; }

            public Action<ushort, AddressingMode> Impl { get; }

            public List<(byte opCode, AddressingMode addressingMode)> CodesAndModes { get; }

        }

        private readonly List<DecoderDefinition> _decoderDefinitions =  new List<DecoderDefinition>()
            {
                new DecoderDefinition(OpCode.ADC,
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
                new DecoderDefinition(OpCode.AND,
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
                new DecoderDefinition(OpCode.ASL,
                    new List<(byte, AddressingMode)> {
                        (0x0A, AddressingMode.Accumulator),
                        (0x06, AddressingMode.ZeroPage),
                        (0x16, AddressingMode.ZeroPageIndexedX),
                        (0x0E, AddressingMode.Absolute),
                        (0x1E, AddressingMode.AbsoluteIndexedX)
                    }),
                new DecoderDefinition(OpCode.BCC,
                    new List<(byte, AddressingMode)> {
                        (0x90, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.BCS,
                    new List<(byte, AddressingMode)> {
                        (0xB0, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.BEQ,
                    new List<(byte, AddressingMode)> {
                        (0xF0, AddressingMode.Relative)
                    }),
                 new DecoderDefinition(OpCode.BIT,
                    new List<(byte, AddressingMode)> {
                        (0x24, AddressingMode.ZeroPage),
                        (0x2C, AddressingMode.Absolute)
                    }),
                new DecoderDefinition(OpCode.BMI,
                    new List<(byte, AddressingMode)> {
                        (0x30, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.BNE,
                    new List<(byte, AddressingMode)> {
                        (0xD0, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.BPL,
                    new List<(byte, AddressingMode)> {
                        (0x10, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.BRK,
                    new List<(byte, AddressingMode)> {
                        (0x00, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.BVC,
                    new List<(byte, AddressingMode)> {
                        (0x50, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.BVS,
                    new List<(byte, AddressingMode)> {
                        (0x70, AddressingMode.Relative)
                    }),
                new DecoderDefinition(OpCode.CLC,
                    new List<(byte, AddressingMode)> {
                        (0x18, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.CLD,
                    new List<(byte, AddressingMode)> {
                        (0xD8, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.CLI,
                    new List<(byte, AddressingMode)> {
                        (0x58, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.CLV,
                    new List<(byte, AddressingMode)> {
                        (0xB8, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.CMP,
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
                new DecoderDefinition(OpCode.CPX,
                    new List<(byte, AddressingMode)> {
                        (0xE0, AddressingMode.Immediate),
                        (0xE4, AddressingMode.ZeroPage),
                        (0xEC, AddressingMode.Absolute)
                }),
                new DecoderDefinition(OpCode.CPY,
                    new List<(byte, AddressingMode)> {
                        (0xC0, AddressingMode.Immediate),
                        (0xC4, AddressingMode.ZeroPage),
                        (0xCC, AddressingMode.Absolute)
                }),
                new DecoderDefinition(OpCode.DEC,
                    new List<(byte, AddressingMode)> {
                        (0xC6, AddressingMode.ZeroPage),
                        (0xD6, AddressingMode.ZeroPageIndexedX),
                        (0xCE, AddressingMode.Absolute),
                        (0xDE, AddressingMode.AbsoluteIndexedX)
                }),
                new DecoderDefinition(OpCode.DEX,
                    new List<(byte, AddressingMode)> {
                        (0xCA, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.DEY,
                    new List<(byte, AddressingMode)> {
                        (0x88, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.EOR,
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
                new DecoderDefinition(OpCode.INC,
                    new List<(byte, AddressingMode)> {
                        (0xE6, AddressingMode.ZeroPage),
                        (0xF6, AddressingMode.ZeroPageIndexedX),
                        (0xEE, AddressingMode.Absolute),
                        (0xFE, AddressingMode.AbsoluteIndexedX)
                    }),
                new DecoderDefinition(OpCode.INX,
                    new List<(byte, AddressingMode)> {
                        (0xE8, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.INY,
                    new List<(byte, AddressingMode)> {
                        (0xC8, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.JMP,
                    new List<(byte, AddressingMode)> {
                        (0x4C, AddressingMode.Absolute),
                        (0x6C, AddressingMode.Indirect)
                    }),
                new DecoderDefinition(OpCode.JSR,
                    new List<(byte, AddressingMode)> {
                        (0x20, AddressingMode.Absolute)
                    }),
                new DecoderDefinition(OpCode.LDA,
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
                new DecoderDefinition(OpCode.LDX,
                    new List<(byte, AddressingMode)> {
                        (0xA2, AddressingMode.Immediate),
                        (0xA6, AddressingMode.ZeroPage),
                        (0xB6, AddressingMode.ZeroPageIndexedY),
                        (0xAE, AddressingMode.Absolute),
                        (0xBE, AddressingMode.AbsoluteIndexedY)
                    }),
                new DecoderDefinition(OpCode.LDY,
                    new List<(byte, AddressingMode)> {
                        (0xA0, AddressingMode.Immediate),
                        (0xA4, AddressingMode.ZeroPage),
                        (0xB4, AddressingMode.ZeroPageIndexedX),
                        (0xAC, AddressingMode.Absolute),
                        (0xBC, AddressingMode.AbsoluteIndexedX)
                    }),
                new DecoderDefinition(OpCode.LSR,
                    new List<(byte, AddressingMode)> {
                        (0x4A, AddressingMode.Accumulator),
                        (0x46, AddressingMode.ZeroPage),
                        (0x56, AddressingMode.ZeroPageIndexedX),
                        (0x4E, AddressingMode.Absolute),
                        (0x5E, AddressingMode.AbsoluteIndexedX)
                    }),
                new DecoderDefinition(OpCode.NOP,
                    new List<(byte, AddressingMode)> {
                        (0xEA, AddressingMode.Implied)
                    }),
                new DecoderDefinition(OpCode.ORA,
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
                 new DecoderDefinition(OpCode.PHA,
                    new List<(byte, AddressingMode)> {
                        (0x48, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.PHP,
                    new List<(byte, AddressingMode)> {
                        (0x08, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.PLA,
                    new List<(byte, AddressingMode)> {
                        (0x68, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.PLP,
                    new List<(byte, AddressingMode)> {
                        (0x28, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.ROL,
                    new List<(byte, AddressingMode)> {
                        (0x2A, AddressingMode.Accumulator),
                        (0x26, AddressingMode.ZeroPage),
                        (0x36, AddressingMode.ZeroPageIndexedX),
                        (0x2E, AddressingMode.Absolute),
                        (0x3E, AddressingMode.AbsoluteIndexedX)
                    }),
                 new DecoderDefinition(OpCode.ROR,
                    new List<(byte, AddressingMode)> {
                        (0x6A, AddressingMode.Accumulator),
                        (0x66, AddressingMode.ZeroPage),
                        (0x76, AddressingMode.ZeroPageIndexedX),
                        (0x6E, AddressingMode.Absolute),
                        (0x7E, AddressingMode.AbsoluteIndexedX)
                    }),
                 new DecoderDefinition(OpCode.RTI,
                    new List<(byte, AddressingMode)> {
                        (0x40, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.RTS,
                    new List<(byte, AddressingMode)> {
                        (0x60, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.SBC,
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
                 new DecoderDefinition(OpCode.SEC,
                    new List<(byte, AddressingMode)> {
                        (0x38, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.SED,
                    new List<(byte, AddressingMode)> {
                        (0xF8, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.SEI,
                    new List<(byte, AddressingMode)> {
                        (0x78, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.STA,
                    new List<(byte, AddressingMode)> {
                        (0x85, AddressingMode.ZeroPage),
                        (0x95, AddressingMode.ZeroPageIndexedX),
                        (0x8D, AddressingMode.Absolute),
                        (0x9D, AddressingMode.AbsoluteIndexedX),
                        (0x99, AddressingMode.AbsoluteIndexedY),
                        (0x81, AddressingMode.IndexedXIndirect),
                        (0x91, AddressingMode.IndirectIndexedY)
                    }),
                 new DecoderDefinition(OpCode.STX,
                    new List<(byte, AddressingMode)> {
                        (0x86, AddressingMode.ZeroPage),
                        (0x96, AddressingMode.ZeroPageIndexedY),
                        (0x8E, AddressingMode.Absolute)
                    }),
                 new DecoderDefinition(OpCode.STY,
                    new List<(byte, AddressingMode)> {
                        (0x84, AddressingMode.ZeroPage),
                        (0x94, AddressingMode.ZeroPageIndexedX),
                        (0x8C, AddressingMode.Absolute)
                    }),
                 new DecoderDefinition(OpCode.TAX,
                    new List<(byte, AddressingMode)> {
                        (0xAA, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.TAY,
                    new List<(byte, AddressingMode)> {
                        (0xA8, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.TSX,
                    new List<(byte, AddressingMode)> {
                        (0xBA, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.TXA,
                    new List<(byte, AddressingMode)> {
                        (0x8A, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.TXS,
                    new List<(byte, AddressingMode)> {
                        (0x9A, AddressingMode.Implied)
                    }),
                 new DecoderDefinition(OpCode.TYA,
                    new List<(byte, AddressingMode)> {
                        (0x98, AddressingMode.Implied)
                    })
            };

        private readonly bool[] _opcodeIsValid = new bool[256];
        private readonly (OpCode opCode,  AddressingMode addressingMode)[] _decoder =  new (OpCode opCode, AddressingMode)[256];

        private void PopulateDecoder()
        {
            foreach (var decoderDefinition in _decoderDefinitions)
            {
                foreach (var codeAndMode in decoderDefinition.CodesAndModes)
                {
                    if (_opcodeIsValid[codeAndMode.opCode])
                    {
                        throw new Exception($"Duplicate op code {decoderDefinition.OpCode} 0x{codeAndMode.opCode:X2}"); // TODO
                    }
                    else
                    {
                        _opcodeIsValid[codeAndMode.opCode] = true;
                    }
                    _decoder[codeAndMode.opCode] = (opCode: decoderDefinition.OpCode, addressingMode: codeAndMode.addressingMode);
                }
            }
        }

        public Decoder()
        {
            PopulateDecoder();
        }

        public byte GetAddressingModePCDelta(AddressingMode addressingMode)
        {
            return _addressingModeToPCDelta[(byte)addressingMode];
        }

        public bool IsValid(byte opCodeByte)
        {
            return _opcodeIsValid[opCodeByte];
        }

        public (OpCode opCode, AddressingMode addressingMode) Decode(byte opCodeByte)
        {
            return _decoder[opCodeByte];
        }
    }
}
