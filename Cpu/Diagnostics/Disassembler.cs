using BbcMicro.Memory.Abstractions;
using System;
using System.Text;

namespace BbcMicro.Cpu
{
    public sealed class Disassembler
    {
        private static Decoder _decoder = new Decoder();

        public string Disassemble(ushort address, IAddressSpace memory)
        {
            (var opCode, var addressingMode) = _decoder.Decode(memory.GetByte(address));

            var result = new StringBuilder();

            result.Append($"{opCode} ");

            ushort operandAddress = (ushort)(address + 1);
            result.Append(addressingMode switch
            {
                AddressingMode.Accumulator => "",
                AddressingMode.Immediate => $"#${memory.GetByte(operandAddress):X2}",
                AddressingMode.Implied => "",
                AddressingMode.Relative => $"${memory.GetByte(operandAddress):X2}",
                AddressingMode.Absolute => $"${memory.GetWord(operandAddress):X4}",
                AddressingMode.ZeroPage => $"${memory.GetByte(operandAddress):X2}",
                AddressingMode.Indirect => $"(${memory.GetWord(operandAddress):X4})",
                AddressingMode.AbsoluteIndexedX => $"${memory.GetWord(operandAddress):X4},X",
                AddressingMode.AbsoluteIndexedY => $"${memory.GetWord(operandAddress):X4},Y",
                AddressingMode.ZeroPageIndexedX => $"${memory.GetByte(operandAddress):X2},X",
                AddressingMode.ZeroPageIndexedY => $"${memory.GetByte(operandAddress):X2},Y",
                AddressingMode.IndexedXIndirect => $"(${memory.GetByte(operandAddress):X2},X)",
                AddressingMode.IndirectIndexedY => $"(${memory.GetByte(operandAddress):X2}),Y",
                _ => throw new Exception($"Invalid addressing mode '{addressingMode}'") // TODO
            });

            return result.ToString();
        }

        public string Disassemble(CPU cpu)
        {
            return Disassemble(cpu.PC, cpu.Memory);
        }
    }
}