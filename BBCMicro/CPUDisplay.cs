using BbcMicro.ConsoleWindowing;
using BbcMicro.Cpu;
using System;
using System.Collections.Generic;

namespace BbcMicro
{
    public sealed class CPUDisplay
    {
        private const int BANNER_HEIGHT = 1;
        private const int DIS_BANNER_TOP = BANNER_HEIGHT + 1;

        private const int DIS_VP_TOP = DIS_BANNER_TOP + 2;
        private const int DIS_VP_LEFT = 0;
        private const int DIS_VP_HEIGHT = 10;
        private const int DIS_VP_WIDTH = 29;

        private const int REG_BANNER_LEFT = DIS_VP_WIDTH + 2;
        private const int REG_BANNER_TOP = DIS_BANNER_TOP;

        private const int REG_VP_TOP = DIS_VP_TOP;
        private const int REG_VP_LEFT = REG_BANNER_LEFT;
        private const int REG_VP_WIDTH = 17;
        private const int REG_VP_HEIGHT = 3;

        private const int MSG_BANNER_LEFT = REG_BANNER_LEFT;
        private const int MSG_BANNER_TOP = REG_VP_TOP + REG_VP_HEIGHT + 1;

        private const int MSG_VP_TOP = MSG_BANNER_TOP + 2;
        private const int MSG_VP_LEFT = REG_VP_LEFT;
        private const int MSG_VP_HEIGHT = 4;
        private const int MSG_VP_WIDTH = 26;

        private CPU _cpu;
        private Disassembler _dis = new Disassembler();
        private ProcessorState _oldState;
        private readonly Decoder _decoder = new Decoder();

        private readonly Viewport _regViewport =
            new Viewport(REG_VP_TOP, REG_VP_LEFT, REG_VP_WIDTH, REG_VP_HEIGHT, ConsoleColor.Black, ConsoleColor.White);

        private readonly Viewport _disViewport =
            new Viewport(DIS_VP_TOP, DIS_VP_LEFT, DIS_VP_WIDTH, DIS_VP_HEIGHT, ConsoleColor.Black, ConsoleColor.White, true);

        private readonly Viewport _messageViewport =
            new Viewport(MSG_VP_TOP, MSG_VP_LEFT, MSG_VP_WIDTH, MSG_VP_HEIGHT, ConsoleColor.Black, ConsoleColor.White, true);

        public CPUDisplay(CPU cpu)
        {
            _cpu = cpu;
            _oldState = new ProcessorState(_cpu);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            RenderBanner();
            RenderDisBanner();
            RenderRegBanner();
            RenderMessageBanner();

            _regViewport.Clear();
            _disViewport.Clear();
            _messageViewport.Clear();

            Update();
        }

        public void ClearMessage()
        {
            _messageViewport.Clear();
        }

        private void RenderDisBanner()
        {
            Console.SetCursorPosition(0, DIS_BANNER_TOP);
            Console.Write("Code");
        }

        private void RenderRegBanner()
        {
            Console.SetCursorPosition(REG_BANNER_LEFT, REG_BANNER_TOP);
            Console.Write("Processor");
        }

        private void RenderMessageBanner()
        {
            Console.SetCursorPosition(MSG_BANNER_LEFT, MSG_BANNER_TOP);
            Console.Write("Messages");
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

        private void WriteString(string value, Viewport viewport, bool changed = false)
        {
            if (changed)
            {
                viewport.Write(value, ConsoleColor.Red, viewport.DefaultBackgroundColour);
            }
            else
            {
                viewport.Write(value);
            }
        }

        private void WriteByte(byte value, Viewport viewport, bool changed = false)
        {
            WriteString($"${value:X2}", viewport, changed);
        }

        private void WriteWord(ushort value, Viewport viewport, bool changed = false)
        {
            WriteString($"${value:X4}", viewport, changed);
        }

        public void WriteMessage(string message)
        {
            _messageViewport.Write(message);
            _messageViewport.NewLine();
        }

        private void RenderBanner()
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.WriteLine("6502 Processor Emulator");
        }

        public void Update()
        {
            (var opCode, var addressingMode) = _decoder.Decode(_cpu.Memory.GetByte(_cpu.PC));

            var pcChanged = _cpu.PC != _oldState.PC;
            var sChanged = _cpu.S != _oldState.S;
            var aChanged = _cpu.A != _oldState.A;
            var xChanged = _cpu.X != _oldState.X;
            var yChanged = _cpu.Y != _oldState.Y;
            var pChanged = _cpu.P != _oldState.P;

            Console.CursorVisible = false;

            _regViewport.Clear();

            WriteString("PC ", _regViewport); WriteWord(_cpu.PC, _regViewport, pcChanged);
            WriteString(" S ", _regViewport); WriteByte(_cpu.S, _regViewport, sChanged);
            _regViewport.NewLine();

            WriteString("A ", _regViewport); WriteByte(_cpu.A, _regViewport, aChanged);
            WriteString(" X ", _regViewport); WriteByte(_cpu.X, _regViewport, xChanged);
            WriteString(" Y ", _regViewport); WriteByte(_cpu.Y, _regViewport, yChanged);
            _regViewport.NewLine();

            WriteString("P ", _regViewport); WriteByte(_cpu.P, _regViewport, pChanged);
            WriteString(" (", _regViewport);
            var flags = new List<CPU.PFlags>((CPU.PFlags[])Enum.GetValues(typeof(CPU.PFlags)));
            flags.Reverse();

            byte pXord = (byte)(_cpu.P ^ _oldState.P);

            foreach (var flag in flags)
            {
                var changed = (pXord & (byte)flag) != 0;
                if (_cpu.PIsSet(flag))
                {
                    WriteString(flag.ToString(), _regViewport, changed);
                }
                else
                {
                    WriteString(flag.ToString().ToLower(), _regViewport, changed);
                }
            }
            WriteString(")", _regViewport);

            _disViewport.Write($"${_cpu.PC:X4}");
            _disViewport.Write($" ${_cpu.Memory.GetByte(_cpu.PC):X2}");
            var operandBytes = _decoder.GetAddressingModePCDelta(addressingMode);
            for (ushort pcOffset = 0; pcOffset < operandBytes; pcOffset++)
            {
                _disViewport.Write($" ${_cpu.Memory.GetByte((ushort)(_cpu.PC + pcOffset + 1)):X2}");
            }
            _disViewport.Write($" {_dis.Disassemble(_cpu)}");
            _disViewport.NewLine();

            Console.CursorVisible = true;

            _oldState = new ProcessorState(_cpu);
        }
    }
}