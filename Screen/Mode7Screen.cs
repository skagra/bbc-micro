using BbcMicro.ConsoleWindowing;
using BbcMicro.Memory.Abstractions;
using BbcMicro.Memory.Extensions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Screen
{
    public class Mode7Screen
    {
        private const int SCREEN_BASE = 0x7C00;
        private const int SCREEN_MEM_SIZE = 40 * 25;
        private const int SCREEN_MEM_END = SCREEN_BASE + SCREEN_MEM_SIZE;

        private readonly IAddressSpace _memory;
        private readonly int _scanSleep;

        private readonly Viewport _screenViewport;

        public Mode7Screen(IAddressSpace memory, int frequencyHz = 10, int top = 0, int left = 0)
        {
            _memory = memory;
            _scanSleep = 1000 / frequencyHz;

            _screenViewport = new Viewport(top, left, 40, 25, ConsoleColor.White, ConsoleColor.Black);
        }

        public void Draw()
        {
            var screenBase = _memory.GetNativeWord(0x350);
            Console.CursorVisible = false;

            var buffer = new StringBuilder();

            for (int row = 0; row < 25; row++)
            {
                buffer.Clear();

                _screenViewport.CursorLeft = 0;
                _screenViewport.CursorTop = row;

                for (int col = 0; col < 40; col++)
                {
                    ushort charAddr = (ushort)(screenBase + row * 40 + col);
                    if (charAddr > SCREEN_MEM_END + 23)
                    {
                        charAddr = (ushort)(charAddr - SCREEN_MEM_SIZE - 24);
                    }

                    var currentChar = _memory.GetByte(charAddr);

                    if (currentChar >= 0x20 && currentChar <= 0x7E)
                    {
                        buffer.Append((char)currentChar);
                    }
                    else
                    {
                        buffer.Append(" ");
                    }
                }
                _screenViewport.Write(buffer.ToString());
            }
        }

        public void StartScan()
        {
            _screenViewport.Clear();

            Task.Run(() =>
            {
                while (true)
                {
                    Draw();
                    Thread.Sleep(_scanSleep);
                }
            });
        }
    }
}