using System.Threading.Channels;
using System.Windows.Input;

namespace BbcMicro.WPF
{
    public sealed class WPFKeyboardEmu
    {
        private readonly Channel<WPFKeyDetails> _buffer = Channel.CreateUnbounded<WPFKeyDetails>();

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
                key == Key.OemQuotes;
        }

        public void PushToBuffer(WPFKeyDetails keyDetails)
        {
            if (IsKeyOfInterest(keyDetails.Key))
            {
                _buffer.Writer.WriteAsync(keyDetails);
            }
        }

        public WPFKeyDetails BlockAndReadKey()
        {
            var done = false;
            WPFKeyDetails result = null;

            while (!done)
            {
                _buffer.Reader.WaitToReadAsync().AsTask().Wait();
                done = _buffer.Reader.TryRead(out result);
            }

            return result;
        }
    }
}