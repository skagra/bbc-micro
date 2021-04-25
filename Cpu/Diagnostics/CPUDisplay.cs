using System;
using System.Collections.Generic;

namespace BbcMicro.Cpu.Diagnostics
{
    public sealed class CPUDisplay
    {
        private CPU _cpu;
        private Disassembler _dis = new Disassembler();
        private ProcessorState _oldState;

        private const int BANNER_HEIGHT = 2;
        private const int CPU_STATUS_TOP = BANNER_HEIGHT + 1;
        private const int CPU_STATUS_HEIGHT = 18;
        private const int MESSAGE_BANNER_HEIGHT = 3;
        private const int MESSAGE_HEIGHT = 1;
        private const int MESSAGE_BANNER_TOP = CPU_STATUS_TOP + CPU_STATUS_HEIGHT + 1;
        private const int MESSAGE_TOP = MESSAGE_BANNER_TOP + MESSAGE_BANNER_HEIGHT;
        private const int OUTPUT_BANNER_HEIGHT = 3;
        private const int OUTPUT_BANNER_TOP = MESSAGE_TOP + MESSAGE_HEIGHT + 1;
        private const int OUTPUT_TOP = OUTPUT_BANNER_TOP + OUTPUT_BANNER_HEIGHT;

        private const int CPU_STATUS_TABLE_LEFT = 24;

        public CPUDisplay(CPU cpu)
        {
            _cpu = cpu;
            _oldState = new ProcessorState(_cpu);

            Console.Clear();
            
            RenderBanner();
            Render();
            RenderMessageBanner();
            RenderOutputBanner();

            Console.CursorTop = OUTPUT_TOP;
        }

        private void RenderMessageBanner()
        {
            Console.CursorTop = MESSAGE_BANNER_TOP;
            RenderString("Message");
            RenderString();
            RenderString("-------");
            RenderString();
            RenderString();
            RenderString("-");
            RenderString();
        }

        private void RenderOutputBanner()
        {
            Console.CursorTop = OUTPUT_BANNER_TOP;
            Console.WriteLine("Output");
            Console.WriteLine("------");
            Console.WriteLine();
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

        private readonly Decoder _decoder = new Decoder();

        public void RenderMessage(string message)
        {
            var clSafe = Console.CursorLeft;
            var ctSafe = Console.CursorTop;

            Console.CursorLeft = 0;
            Console.CursorTop = MESSAGE_TOP;
            Console.Write("                                                        ");
            Console.CursorLeft = 0;
            Console.CursorTop = MESSAGE_TOP;
            Console.Write(message);

            Console.CursorLeft = clSafe;
            Console.CursorTop = ctSafe;
        }

        private void Clear()
        {
            var clSafe = Console.CursorLeft;
            var ctSafe = Console.CursorTop;

            Console.CursorLeft = 0;
            Console.CursorTop = CPU_STATUS_TOP;

            for (int row=0; row <CPU_STATUS_HEIGHT; row++)
            {
                Console.CursorLeft = 0;
                Console.Write("                                                ");
                Console.CursorTop++;
            }

            Console.CursorLeft = clSafe;
            Console.CursorTop = ctSafe;
        }

        private void RenderBanner()
        {
            Console.WriteLine("6502 Processor Emulator");
            Console.WriteLine("-----------------------");
            Console.WriteLine();
        }

        public void Render()
        {
            (var opCode, var addressingMode) = _decoder.Decode(_cpu.Memory.GetByte(_cpu.PC));

            var pcChanged = _cpu.PC != _oldState.PC;
            var sChanged = _cpu.S != _oldState.S;
            var aChanged = _cpu.A != _oldState.A;
            var xChanged = _cpu.X != _oldState.X;
            var yChanged = _cpu.Y != _oldState.Y;
            var pChanged = _cpu.P != _oldState.P;

            Clear();
            Console.CursorVisible = false;

            var clSafe = Console.CursorLeft;
            var ctSafe = Console.CursorTop;
 
            Console.SetCursorPosition(0, CPU_STATUS_TOP);

            RenderString("Processor State"); RenderString();
            RenderString("---------------"); RenderString();
            RenderString();

            RenderString("+-----------------------+");
            RenderString();

            RenderString("| PC "); RenderWord(_cpu.PC, pcChanged);
            RenderString("  | S "); RenderByte(_cpu.S, sChanged);
            RenderString("     |");
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

            RenderString();
            RenderString("Memory"); RenderString();
            RenderString("------"); RenderString();
            RenderString();

            RenderString("+-----------------------+");
            RenderString();

            RenderString("| Dis: "); RenderString(_dis.Disassemble(_cpu)); Console.CursorLeft = CPU_STATUS_TABLE_LEFT; RenderString("|");
            RenderString();

            RenderString($"| Mem: ${_cpu.Memory.GetByte(_cpu.PC):X2}");
            var operandBytes = _decoder.GetAddressingModePCDelta(addressingMode);
            for (ushort pcOffset=0; pcOffset<operandBytes; pcOffset++)
            {
                RenderString($" ${_cpu.Memory.GetByte((ushort)(_cpu.PC+pcOffset+1)):X2}");
            }
            Console.CursorLeft = CPU_STATUS_TABLE_LEFT; RenderString("|");
            RenderString();

            RenderString("+-----------------------+"); RenderString();
            RenderString();

            Console.CursorLeft = clSafe;
            Console.CursorTop = ctSafe;
            
            Console.CursorVisible = true;

            _oldState = new ProcessorState(_cpu);
        }
    }
}