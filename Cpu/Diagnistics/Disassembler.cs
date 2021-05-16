using BbcMicro.Memory.Abstractions;
using BbcMicro.Memory.Extensions;
using BbcMicro.Cpu.Exceptions;

namespace BbcMicro.Cpu
{
    public sealed class Disassembler
    {
        private static readonly Decoder _decoder = new Decoder();

        private readonly IAddressSpace _memory;
        private readonly string[] _symbols;

        public class DisassembledInstruction
        {
            public DisassembledInstruction(string disassembly, byte[] memory, string label)
            {
                Disassembly = disassembly;
                Memory = memory;
                Label = label;
            }

            public byte[] Memory { get; }

            public string Label { get; }

            public string Disassembly { get; }
        }

        public Disassembler(IAddressSpace addressSpace, string[] symbols = null)
        {
            _memory = addressSpace;
            _symbols = symbols;
        }

        private byte[] GetMemory(AddressingMode addressingMode, ushort address)
        {
            var memory = new byte[_decoder.GetAddressingModePCDelta(addressingMode) + 1];
            memory[0] = _memory.GetByte(address);

            for (ushort pcOffset = 1; pcOffset < memory.Length; pcOffset++)
            {
                memory[pcOffset] = _memory.GetByte((ushort)(address + pcOffset));
            }

            return memory;
        }

        private string GetSymbol(ushort address)
        {
            string symbol = null;

            if (_symbols != null)
            {
                symbol = _symbols[address];
            }

            return symbol;
        }

        private string AddrOrSymbolWord(ushort address)
        {
            return GetSymbol(address) ?? $"${address:X4}";
        }

        private string AddrOrSymbolByte(byte address)
        {
            return GetSymbol(address) ?? $"${address:X2}";
        }

        //private string GetResolvedOperand(AddressingMode addressingMode, ushort address)
        //{
        //    var operandAddress = (ushort)(address + 1);

        //    return addressingMode switch
        //    {
        //        AddressingMode.Accumulator => "",
        //        AddressingMode.Immediate => "",
        //        AddressingMode.Implied => "",
        //        AddressingMode.Relative => $"(${address + 2 + (sbyte)_memory.GetByte(operandAddress):X4})",
        //        AddressingMode.Absolute => $"(${_memory.GetByte(_memory.GetNativeWord(operandAddress)):X2})",
        //        AddressingMode.ZeroPage => $"(${_memory.GetByte(_memory.GetByte(operandAddress)):X2})",
        //        AddressingMode.Indirect => $"(${_memory.GetNativeWord(_memory.GetNativeWord(operandAddress)):X4})",
        //        AddressingMode.AbsoluteIndexedX => $"(${_memory.GetByte((ushort)(_memory.GetNativeWord(operandAddress) + _cpu.X)):X2})",
        //        AddressingMode.AbsoluteIndexedY => $"(${_memory.GetByte((ushort)(_memory.GetNativeWord(operandAddress) + _cpu.Y)):X2})",
        //        AddressingMode.ZeroPageIndexedX => $"(${_memory.GetByte((byte)(_memory.GetByte(operandAddress) + _cpu.X)):X2})",
        //        AddressingMode.ZeroPageIndexedY => $"(${_memory.GetByte((byte)(_memory.GetByte(operandAddress) + _cpu.Y)):X2})",
        //        AddressingMode.IndexedXIndirect => $"(${_memory.GetByte(_memory.GetNativeWord((byte)(_memory.GetByte(operandAddress) + _cpu.X))):X2})",
        //        AddressingMode.IndirectIndexedY => $"(${_memory.GetByte((ushort)(_memory.GetNativeWord(_memory.GetByte(operandAddress)) + _cpu.Y)):X2})",
        //        _ => throw new CPUException($"Invalid addressing mode '{addressingMode}'")
        //    };
        //}

        private string GetDisassembly(OpCode opCode, AddressingMode addressingMode, ushort address)
        {
            ushort operandAddress = (ushort)(address + 1);
            var operand = addressingMode switch
            {
                AddressingMode.Accumulator => "",
                AddressingMode.Immediate => $"#${_memory.GetByte(operandAddress):X2}",
                AddressingMode.Implied => "",
                AddressingMode.Relative => $"${_memory.GetByte(operandAddress):X2}",
                AddressingMode.Absolute => $"{AddrOrSymbolWord(_memory.GetNativeWord(operandAddress))}",
                AddressingMode.ZeroPage => $"{AddrOrSymbolByte(_memory.GetByte(operandAddress))}",
                AddressingMode.Indirect => $"({AddrOrSymbolWord(_memory.GetNativeWord(operandAddress))})",
                AddressingMode.AbsoluteIndexedX => $"{AddrOrSymbolWord(_memory.GetNativeWord(operandAddress))},X",
                AddressingMode.AbsoluteIndexedY => $"{AddrOrSymbolWord(_memory.GetNativeWord(operandAddress))},Y",
                AddressingMode.ZeroPageIndexedX => $"{AddrOrSymbolByte(_memory.GetByte(operandAddress))},X",
                AddressingMode.ZeroPageIndexedY => $"{AddrOrSymbolByte(_memory.GetByte(operandAddress))},Y",
                AddressingMode.IndexedXIndirect => $"({AddrOrSymbolByte(_memory.GetByte(operandAddress))},X)",
                AddressingMode.IndirectIndexedY => $"({AddrOrSymbolByte(_memory.GetByte(operandAddress))}),Y",
                _ => throw new CPUException($"Invalid addressing mode '{addressingMode}'")
            };

            return $"{opCode} {operand}";
        }

        public DisassembledInstruction Disassemble(ushort address)
        {
            (var opCode, var addressingMode) = _decoder.Decode(_memory.GetByte(address));

            return new DisassembledInstruction(disassembly: GetDisassembly(opCode, addressingMode, address),
                label: GetSymbol(address),
                memory: GetMemory(addressingMode, address));
        }
    }
}