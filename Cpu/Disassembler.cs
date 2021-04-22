using System;
using System.Collections.Generic;
using System.Text;

namespace BbcMicro.Cpu
{
    public sealed class Disassembler
    {
        public string Disassemble(OpCode opCode, AddressingMode addressingMode, ushort address, CPU cpu)
        {
            var result = new StringBuilder();

            result.Append($"{opCode} ");

            var memory = cpu.Memory;
            ushort operandAddress = (ushort)(cpu.PC + 1);
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
    }
}