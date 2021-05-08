using System.Threading.Channels;
using System.Windows.Input;

namespace BbcMicro.WPF
{
    public sealed class WPFKeyboardEmu
    {
        private readonly Channel<byte> _buffer = Channel.CreateUnbounded<byte>();

        private static bool IsKeyOfInterest(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) ||
                ((key >= Key.A) && (key <= Key.Z)) ||
                key == Key.Space ||
                key == Key.Back ||
                key == Key.Return ||
                key == Key.Divide ||
                key == Key.OemMinus ||
                key == Key.OemPlus ||
                key == Key.OemComma ||
                key == Key.OemPeriod ||
                key == Key.Oem1 ||
                key == Key.Oem3 ||
                key == Key.OemQuestion |
                key == Key.OemOpenBrackets ||
                key == Key.Oem6 ||
                key == Key.OemQuotes ||
                key == Key.Escape;
        }

        public void PushToBuffer(WPFKeyDetails keyDetails)
        {
            if (IsKeyOfInterest(keyDetails.Key))
            {
                _buffer.Writer.WriteAsync(TranslateKey(keyDetails));
            }
        }

        public void PushToBuffer(string value)
        {
            foreach (var chr in value)
            {
                _buffer.Writer.WriteAsync((byte)chr);
            }
        }

        public byte BlockAndReadKey()
        {
            var done = false;
            byte result = 0;

            while (!done)
            {
                _buffer.Reader.WaitToReadAsync().AsTask().Wait();
                done = _buffer.Reader.TryRead(out result);
            }

            return result;
        }

        private byte TranslateKey(WPFKeyDetails keyDetails)
        {
            var key = keyDetails.Key;
            var modifiers = keyDetails.Modifiers;
            byte result = 0;

            // Numerics
            if (key >= Key.D0 && key <= Key.D9)
            {
                if (!(modifiers.HasFlag(ModifierKeys.Shift)))
                {
                    result = (byte)('0' + (key - Key.D0));
                }
                else
                {
                    switch (key)
                    {
                        case Key.D1:
                            result = (byte)'!';
                            break;

                        case Key.D2:
                            result = (byte)'"';
                            break;

                        case Key.D3:
                            result = (byte)96;
                            break;

                        case Key.D4:
                            result = (byte)'$';
                            break;

                        case Key.D5:
                            result = (byte)'%';
                            break;

                        case Key.D6:
                            result = (byte)'^';
                            break;

                        case Key.D7:
                            result = (byte)'&';
                            break;

                        case Key.D8:
                            result = (byte)'*';
                            break;

                        case Key.D9:
                            result = (byte)'(';
                            break;

                        case Key.D0:
                            result = (byte)')';
                            break;
                    }
                }
            }
            // Alphabetics
            if (key >= Key.A && key <= Key.Z)
            {
                if (!(modifiers.HasFlag(ModifierKeys.Shift) ^ keyDetails.CapsLock))
                {
                    result = (byte)('a' + (key - Key.A));
                }
                else
                {
                    result = (byte)('A' + (key - Key.A));
                }
            }
            else
            if (key == Key.Space)
            {
                result = 32;
            }
            else
            if (key == Key.Back)
            {
                result = 127;
            }
            else
            if (key == Key.Return)
            {
                result = 13;
            }
            if (key == Key.OemPeriod)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'>';
                }
                else
                {
                    result = (byte)'.';
                }
            }
            else
            if (key == Key.OemComma)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'<';
                }
                else
                {
                    result = (byte)',';
                }
            }
            if (key == Key.OemPlus)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'+';
                }
                else
                {
                    result = (byte)'=';
                }
            }
            else
            if (key == Key.OemMinus)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'_';
                }
                else
                {
                    result = (byte)'-';
                }
            }
            else
            if (key == Key.OemQuestion)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'?';
                }
                else
                {
                    result = (byte)'/';
                }
            }
            else
            if (key == Key.Oem1)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)':';
                }
                else
                {
                    result = (byte)';';
                }
            }
            else
            if (key == Key.Oem3)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'@';
                }
                else
                {
                    result = (byte)'\'';
                }
            }
            else
            if (key == Key.OemOpenBrackets)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'{';
                }
                else
                {
                    result = (byte)'[';
                }
            }
            else
            if (key == Key.Oem6)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'}';
                }
                else
                {
                    result = (byte)']';
                }
            }
            else
            if (key == Key.OemQuotes)
            {
                if (modifiers.HasFlag(ModifierKeys.Shift))
                {
                    result = (byte)'~';
                }
                else
                {
                    result = (byte)'#';
                }
            }
            else
            if (key == Key.Escape)
            {
                result = 27;
            }

            return result;
        }
    }
}