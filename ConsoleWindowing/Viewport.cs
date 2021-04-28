using System;

namespace BbcMicro.ConsoleWindowing
{
    public sealed class Viewport
    {
        private int ViewportTop { get; }

        private int ViewportLeft { get; }

        private int ViewportWidth { get; }

        private int ViewportHeight { get; }

        public ConsoleColor DefaultForegroundColour { get; }

        public ConsoleColor DefaultBackgroundColour { get; }

        public int CursorTop { get; set; }

        public int CursorLeft { get; set; }

        private readonly string _fillString;

        private readonly bool _autoScroll;

        public Viewport(int top, int left, int width, int height,
            ConsoleColor defaultForeground = ConsoleColor.Black,
            ConsoleColor defaultBackground = ConsoleColor.White,
            bool autoScroll = false)
        {
            ViewportTop = top;
            ViewportLeft = left;
            ViewportWidth = width;
            ViewportHeight = height;

            DefaultForegroundColour = defaultForeground;
            DefaultBackgroundColour = defaultBackground;

            _autoScroll = autoScroll;

            _fillString = new string(' ', width);

            Clear();
        }

        private int _savedConsoleCursorLeft;
        private int _savedConsoleCursorTop;
        private ConsoleColor _savedConsoleForeground;
        private ConsoleColor _savedConsoleBackground;

        private void SaveConsoleState()
        {
            _savedConsoleCursorLeft = Console.CursorLeft;
            _savedConsoleCursorTop = Console.CursorTop;
            _savedConsoleForeground = Console.ForegroundColor;
            _savedConsoleBackground = Console.BackgroundColor;
        }

        private void RestoreConsoleState()
        {
            Console.CursorLeft = _savedConsoleCursorLeft;
            Console.CursorTop = _savedConsoleCursorTop;
            Console.ForegroundColor = _savedConsoleForeground;
            Console.BackgroundColor = _savedConsoleBackground;
        }

        public Viewport ResetCursor()
        {
            CursorLeft = 0;
            CursorTop = 0;

            return this;
        }

        public Viewport NewLine()
        {
            CursorLeft = 0;
            CursorTop++;

            return this;
        }

        public Viewport Clear()
        {
            SaveConsoleState();

            Console.BackgroundColor = DefaultBackgroundColour;
            for (var line = 0; line < ViewportHeight; line++)
            {
                Console.CursorLeft = ViewportLeft;
                Console.CursorTop = ViewportTop + line;
                Console.Write(_fillString);
            }
            CursorTop = 0;
            CursorLeft = 0;

            RestoreConsoleState();

            return this;
        }

        public Viewport Scroll()
        {
            SaveConsoleState();

            Console.MoveBufferArea(ViewportLeft, ViewportTop + 1, ViewportWidth, ViewportHeight - 1,
                ViewportLeft, ViewportTop);

            Console.CursorLeft = ViewportLeft;
            Console.CursorTop = ViewportTop + ViewportHeight - 1;
            Console.BackgroundColor = DefaultBackgroundColour;
            Console.Write(_fillString);
            CursorLeft = 0;

            RestoreConsoleState();

            return this;
        }

        public Viewport Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            SaveConsoleState();
            if (CursorTop == ViewportHeight)
            {
                if (_autoScroll)
                {
                    Scroll();
                }
                CursorTop = ViewportHeight - 1;
            }
            Console.CursorTop = CursorTop + ViewportTop;
            Console.CursorLeft = CursorLeft + ViewportLeft;
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            CursorLeft = Console.CursorLeft - ViewportLeft;
            RestoreConsoleState();

            return this;
        }

        public Viewport Write(string text)
        {
            Write(text, DefaultForegroundColour, DefaultBackgroundColour);

            return this;
        }
    }
}