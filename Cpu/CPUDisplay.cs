using System;
using System.Collections.Generic;

namespace BbcMicro.Cpu
{
    public sealed class CPUDisplay
    {
        private CPU _cpu;
        private Disassembler _dis = new Disassembler();
        private ProcessorState _oldState;

        public CPUDisplay(CPU cpu)
        {
            _cpu = cpu;
            _oldState = new ProcessorState(_cpu);
        }

        private sealed class ProcessorState
        {
            public ProcessorState(CPU cpu)
            {
                PC = cpu.PC;
                S = cpu.S;
                A = cpu.A;
                X = cpu.X;
                Y = cpu.Y;
                P = cpu.P;
            }

            public ushort PC { get; }

            public ushort S { get; }

            public byte A { get; }

            public byte X { get; }

            public byte Y { get; }

            public byte P { get; }
        }

        private void RenderString(string value = "\n", bool changed = false)
        {
            var savedFg = Console.ForegroundColor;
            if (changed)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write(value);
            if (changed)
            {
                Console.ForegroundColor = savedFg;
            }
        }

        private void RenderByte(byte value, bool changed = false)
        {
            RenderString($"${value:X2}", changed);
        }

        private void RenderWord(ushort value, bool changed = false)
        {
            RenderString($"${value:X4}", changed);
        }

        public void Render(OpCode opCode, AddressingMode addressingMode)
        {
            var pcChanged = _cpu.PC != _oldState.PC;
            var sChanged = _cpu.S != _oldState.S;
            var aChanged = _cpu.A != _oldState.A;
            var xChanged = _cpu.X != _oldState.X;
            var yChanged = _cpu.Y != _oldState.Y;
            var pChanged = _cpu.P != _oldState.P;

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            RenderString("Processor State"); RenderString();
            RenderString();

            RenderString("+-----------------------+");
            RenderString();

            RenderString("| PC "); RenderWord(_cpu.PC, pcChanged);
            RenderString("  | S "); RenderWord(_cpu.S, sChanged);
            RenderString("   |");
            RenderString();

            RenderString("+-------+-------+-------+");
            RenderString();

            RenderString("| A "); RenderByte(_cpu.A, aChanged);
            RenderString(" | X "); RenderByte(_cpu.X, xChanged);
            RenderString(" | Y "); RenderByte(_cpu.Y, yChanged);
            RenderString(" |");
            RenderString();

            RenderString("+-------+-------+-------+");
            RenderString();

            RenderString("| P "); RenderByte(_cpu.P, pChanged); RenderString(" (");
            var flags = new List<CPU.PFlags>((CPU.PFlags[])Enum.GetValues(typeof(CPU.PFlags)));
            flags.Reverse();

            byte pXord = (byte)(_cpu.P ^ _oldState.P);

            foreach (var flag in flags)
            {
                var changed = (pXord & (byte)flag) != 0;
                if (_cpu.PIsSet(flag))
                {
                    RenderString(flag.ToString(), changed);
                }
                else
                {
                    RenderString(flag.ToString().ToLower(), changed);
                }
            }
            RenderString(")      |");
            RenderString();

            RenderString("+-----------------------+");
            RenderString();

            RenderString("| Next: "); RenderString(_dis.Disassemble(opCode, addressingMode, _cpu.PC, _cpu)); Console.CursorLeft = 24; RenderString("|");
            RenderString();

            RenderString("+-----------------------+"); RenderString();
            RenderString();

            _oldState = new ProcessorState(_cpu);
        }
    }
}