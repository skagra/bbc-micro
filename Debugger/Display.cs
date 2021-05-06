using BbcMicro.ConsoleWindowing;
using BbcMicro.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BbcMicro.Debugger
{
    public sealed class Display
    {
        private const int BANNER_HEIGHT = 1;
        private const int DIS_BANNER_TOP = BANNER_HEIGHT + 1;

        private const int DIS_VP_TOP = DIS_BANNER_TOP + 2;
        private const int DIS_VP_LEFT = 0;
        private const int DIS_VP_HEIGHT = 8;
        private const int DIS_VP_WIDTH = 35;

        private const int CPU_BANNER_LEFT = DIS_VP_WIDTH + 2;
        private const int CPU_BANNER_TOP = DIS_BANNER_TOP;

        private const int CPU_VP_TOP = DIS_VP_TOP;
        private const int CPU_VP_LEFT = CPU_BANNER_LEFT;
        private const int CPU_VP_WIDTH = 23;
        private const int CPU_VP_HEIGHT = 3;

        private const int STK_BANNER_LEFT = MEM_VP_LEFT + MEM_VP_WIDTH + 2;
        private const int STK_BANNER_TOP = CPU_BANNER_TOP;

        private const int STK_VP_TOP = STK_BANNER_TOP + 2;
        private const int STK_VP_LEFT = STK_BANNER_LEFT;
        private const int STK_VP_HEIGHT = 8;
        private const int STK_VP_WIDTH = 10;

        private const int MEM_BANNER_LEFT = CPU_BANNER_LEFT;
        private const int MEM_BANNER_TOP = CPU_VP_TOP + CPU_VP_HEIGHT + 1;

        private const int MEM_VP_TOP = MEM_BANNER_TOP + 2;
        private const int MEM_VP_LEFT = CPU_VP_LEFT;
        private const int MEM_VP_HEIGHT = 2;
        private const int MEM_VP_WIDTH = 26;

        private const int RES_VP_TOP = DIS_VP_TOP + DIS_VP_HEIGHT + 1;
        private const int RES_VP_LEFT = 0;
        private const int RES_VP_WIDTH = DIS_VP_WIDTH + MEM_VP_WIDTH + STK_VP_WIDTH + 4;
        private const int RES_VP_HEIGHT = 20;

        private const int CMD_VP_TOP = RES_VP_TOP + RES_VP_HEIGHT + 1;
        private const int CMD_VP_LEFT = 3;
        private const int CMD_VP_WIDTH = RES_VP_WIDTH - 3;
        private const int CMD_VP_HEIGHT = 1;

        private const ConsoleColor GAP_COLOR = ConsoleColor.DarkGray;
        private const ConsoleColor FG_COLOR = ConsoleColor.Black;
        private const ConsoleColor BG_COLOR = ConsoleColor.White;
        private const ConsoleColor HI_FG_COLOR = ConsoleColor.Red;

        private readonly Viewport _cpuViewport =
            new Viewport(CPU_VP_TOP, CPU_VP_LEFT, CPU_VP_WIDTH, CPU_VP_HEIGHT, FG_COLOR, BG_COLOR);

        private readonly Viewport _disViewport =
            new Viewport(DIS_VP_TOP, DIS_VP_LEFT, DIS_VP_WIDTH, DIS_VP_HEIGHT, FG_COLOR, BG_COLOR, true);

        private readonly Viewport _memoryViewport =
            new Viewport(MEM_VP_TOP, MEM_VP_LEFT, MEM_VP_WIDTH, MEM_VP_HEIGHT, FG_COLOR, BG_COLOR, true);

        private readonly Viewport _resultViewport =
            new Viewport(RES_VP_TOP, RES_VP_LEFT, RES_VP_WIDTH, RES_VP_HEIGHT, FG_COLOR, BG_COLOR, true);

        private readonly Viewport _commandViewport =
            new Viewport(CMD_VP_TOP, CMD_VP_LEFT, CMD_VP_WIDTH, CMD_VP_HEIGHT, FG_COLOR, BG_COLOR);

        private readonly Viewport _stackViewport =
            new Viewport(STK_VP_TOP, STK_VP_LEFT, STK_VP_WIDTH, STK_VP_HEIGHT, FG_COLOR, BG_COLOR);

        public Display()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.SetCursorPosition(0, 0);

            WriteHeaderBanner();
            WriteDisBanner();
            WriteCPUBanner();
            WriteStackBanner();
            WriteMemoryBanner();

            _cpuViewport.Clear();
            _disViewport.Clear();
            _memoryViewport.Clear();
            _stackViewport.Clear();
            _resultViewport.Clear();
            WriteCommandPrefix();
            _commandViewport.Clear();
        }

        private void WriteCommandPrefix()
        {
            var safeFg = Console.ForegroundColor;
            var safeBg = Console.BackgroundColor;

            Console.SetCursorPosition(0, CMD_VP_TOP);
            Console.ForegroundColor = FG_COLOR;
            Console.BackgroundColor = BG_COLOR;
            Console.Write(" > ");

            Console.ForegroundColor = safeFg;
            Console.BackgroundColor = safeBg;
        }

        /*
         * Rendering utilities
         */

        private ConsoleColor HiColour(bool highlight)
        {
            return (highlight) ? HI_FG_COLOR : FG_COLOR;
        }

        private string WordStr(ushort value)
        {
            return $"${value:X4}";
        }

        private string ByteStr(byte value)
        {
            return $"${value:X2}";
        }

        /*
         * Stack viewport
         */

        private void WriteStackBanner()
        {
            Console.SetCursorPosition(STK_BANNER_LEFT, STK_BANNER_TOP);
            Console.Write("Stack");
        }

        private void WriteStack(byte[] stack)
        {
            _stackViewport.Clear();
            for (var offset = 0; offset < stack.Length && offset < STK_VP_HEIGHT; offset++)
            {
                _stackViewport.Write($" {offset:X2} ").Gap(GAP_COLOR).Write($" ${stack[offset]:X2}").NewLine();
            }
        }

        /*
         * Dissemmbly viewport
         */

        private void WriteDisBanner()
        {
            Console.SetCursorPosition(0, DIS_BANNER_TOP);
            Console.Write("Code");
        }

        public void ClearDis()
        {
            _disViewport.Clear();
        }

        public void WriteDis(ushort address, byte[] memory, string instruction)
        {
            _disViewport.Space().Write(WordStr(address)).Space().Gap(GAP_COLOR).
                Space().Write(string.Join(" ", memory.Select(b => ByteStr(b)))).
                SetLeft(21).Gap(GAP_COLOR).Space().Write(instruction). // TODO
                NewLine();
        }

        /*
         * Result viewport
         */

        public void ClearResult()
        {
            _resultViewport.Clear();
        }

        public void WriteError(string value)
        {
            _resultViewport.Space().Write(value, HI_FG_COLOR);
            _resultViewport.NewLine();
        }

        public void WriteResult(string value)
        {
            _resultViewport.Space().Write(value);
            _resultViewport.NewLine();
        }

        /*
         * CPU viewport
         */

        private ProcessorState _prevState = null;

        private void WriteCPUBanner()
        {
            Console.SetCursorPosition(CPU_BANNER_LEFT, CPU_BANNER_TOP);
            Console.Write("Processor");
        }

        public void ClearCPU()
        {
            _cpuViewport.Clear();
        }

        public void WriteCPU(ProcessorState cpuState, byte[] stack)
        {
            if (_prevState == null)
            {
                _prevState = new ProcessorState
                {
                    PC = cpuState.PC,
                    S = cpuState.S,
                    A = cpuState.A,
                    X = cpuState.X,
                    Y = cpuState.Y,
                    P = cpuState.P
                };
            }

            _cpuViewport.Clear();

            _cpuViewport.Write(" PC ").Write(WordStr(cpuState.PC), HiColour(cpuState.PC != _prevState.PC)).
                Space().Gap(GAP_COLOR).
                Write(" S ").Write(ByteStr(cpuState.S), HiColour(cpuState.S != _prevState.S)).
                NewLine();

            _cpuViewport.Write(" A ").Write(ByteStr(cpuState.A), HiColour(cpuState.A != _prevState.A)).
                Space().Gap(GAP_COLOR).
            Write(" X ").Write(ByteStr(cpuState.X), HiColour(cpuState.X != _prevState.X)).
                Space().Gap(GAP_COLOR).
            Write(" Y ").Write(ByteStr(cpuState.Y), HiColour(cpuState.Y != _prevState.Y)).
                NewLine();

            _cpuViewport.Write(" P ").Write(ByteStr(cpuState.P), HiColour(cpuState.P != _prevState.P)).
                Space();

            _cpuViewport.Write(" (");
            var flags = new List<CPU.PFlags>((CPU.PFlags[])Enum.GetValues(typeof(CPU.PFlags)));
            flags.Reverse();
            var changedFlags = (byte)(cpuState.P ^ _prevState.P);
            flags.ForEach(f =>
                _cpuViewport.Write(((cpuState.P & (byte)f) != 0) ? f.ToString() : f.ToString().ToLower(),
                    HiColour((changedFlags & (byte)f) != 0))
            );
            _cpuViewport.Write(")");

            if (_prevState.S != cpuState.S)
            {
                WriteStack(stack);
            }

            _prevState.PC = cpuState.PC;
            _prevState.S = cpuState.S;
            _prevState.A = cpuState.A;
            _prevState.X = cpuState.X;
            _prevState.Y = cpuState.Y;
            _prevState.P = cpuState.P;
        }

        /*
         * Command viewport
         */

        public void ClearCommand()
        {
            _commandViewport.Clear();
        }

        public void WriteCommand(string value)
        {
            _commandViewport.Write(value);
        }

        /*
         * Memory viewport
         */

        private void WriteMemoryBanner()
        {
            Console.SetCursorPosition(MEM_BANNER_LEFT, MEM_BANNER_TOP);
            Console.Write("Memory");
        }

        public void ClearMemory()
        {
            _memoryViewport.Clear();
        }

        public void WriteMemory(string value)
        {
            _memoryViewport.Write(value);
            _memoryViewport.NewLine();
        }

        /*
         * Top level header
         */

        private void WriteHeaderBanner()
        {
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            Console.WriteLine("6502 Processor Emulator");
        }

        //private const int SCR_VP_LEFT = STK_VP_LEFT + STK_VP_WIDTH + 2;
        //private const int SCR_VP_TOP = STK_VP_TOP;
        //private const int SCR_VP_WIDTH = 40;
        //private const int SCR_VP_HEIGHT = 25;

        //private readonly Viewport _screenViewport =
        //  new Viewport(SCR_VP_TOP, SCR_VP_LEFT, SCR_VP_WIDTH, SCR_VP_HEIGHT, FG_COLOR, BG_COLOR);

        //private const int SCREEN_BASE = 0x7C00;
        //private const int SCREEN_MEM_SIZE = 40 * 25;
        //private const int SCREEN_MEM_END = SCREEN_BASE + SCREEN_MEM_SIZE;

        //public void InitScreen(IAddressSpace memory)
        //{
        //    _screenViewport.Clear();
        //    Task.Run(() =>
        //    {
        //        while (true)
        //        {
        //            var screenBase = memory.GetNativeWord(0x350);
        //            Console.CursorVisible = false;
        //            _screenViewport.CursorLeft = 0;
        //            _screenViewport.CursorTop = 0;

        //            for (int row = 0; row < 25; row++)
        //            {
        //                for (int col = 0; col < 40; col++)
        //                {
        //                    ushort charAddr = (ushort)(screenBase + row * 40 + col);
        //                    if (charAddr > SCREEN_MEM_END + 23)
        //                    {
        //                        charAddr = (ushort)(charAddr - SCREEN_MEM_SIZE - 24);
        //                    }

        //                    var currentChar = memory.GetByte(charAddr);
        //                    if (currentChar >= 0x20 && currentChar <= 0x7E)
        //                    {
        //                        _screenViewport.Write(((char)currentChar).ToString());
        //                    }
        //                    else
        //                    {
        //                        _screenViewport.Write(" ");
        //                    }
        //                }
        //                _screenViewport.NewLine();
        //            }
        //            Thread.Sleep(100);
        //        }
        //    });
        //}
    }
}