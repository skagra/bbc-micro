using System.Threading.Channels;
using System.Windows.Input;

namespace Keyboard
{
    public sealed class KeyboardEmu
    {
        private readonly Channel<Key> _buffer = Channel.CreateUnbounded<Key>();

        public void PushToBuffer(Key key)
        {
            _buffer.Writer.WriteAsync(key);
        }

        public Key BlockAndReadKey()
        {
            var done = false;
            Key result = Key.Space;

            while (!done)
            {
                _buffer.Reader.WaitToReadAsync().AsTask().Wait();
                done = _buffer.Reader.TryRead(out result);
            }

            return result;
        }
    }
}