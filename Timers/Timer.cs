using BbcMicro.Memory.Abstractions;
using System;

namespace BbcMicro.Timers
{
    // TODO
    // Initial timer frig
    // All sorts of threading issues here! And this would all be handled by the OS
    // once interrupts are working!
    // https://tobylobster.github.io/mos/mos/S-s11.html#SP16
    public class Timer
    {
        private const ushort timeClockSwitch = 0X0283;
        private const ushort timeClockA = 0X0292;
        private const ushort timeClockB = 0X0297;
        private readonly IAddressSpace _addressSpace;

        private long _lastMillis = DateTime.UtcNow.Ticks / 10000;

        public Timer(IAddressSpace addressSpace)
        {
            _addressSpace = addressSpace;
        }

        // BBC clock is not real time - it is zero's att boot and then increments
        private void SetClock(long value, ushort address)
        {
            _addressSpace.SetByte((byte)value, (ushort)(address + 4));
            _addressSpace.SetByte((byte)(value >> 8), (ushort)(address + 3));
            _addressSpace.SetByte((byte)(value >> 16), (ushort)(address + 2));
            _addressSpace.SetByte((byte)(value >> 24), (ushort)(address + 1));
            _addressSpace.SetByte((byte)(value >> 32), (ushort)(address));
        }

        private long GetClock(ushort address)
        {
            long result = _addressSpace.GetByte((ushort)(address + 4));
            result += (_addressSpace.GetByte((ushort)(address + 3)) << 8);
            result += (_addressSpace.GetByte((ushort)(address + 3)) << 16);
            result += (_addressSpace.GetByte((ushort)(address + 3)) << 24);
            result += (_addressSpace.GetByte((ushort)(address + 3)) << 32);

            return result;
        }

        public void Tick()
        {
            var currentMillis = DateTime.UtcNow.Ticks / 10000;

            // Clock is in 10th of a second
            if (currentMillis - _lastMillis >= 100)
            {
                _lastMillis = currentMillis;

                var whichClock = _addressSpace.GetByte(timeClockSwitch);
                if (whichClock == 5)
                {
                    long currentClock = GetClock(timeClockA);
                    currentClock += 1;
                    SetClock(currentClock, timeClockB);
                    _addressSpace.SetByte(10, timeClockSwitch);
                }
                else
                {
                    long currentClock = GetClock(timeClockB);
                    currentClock += 1;
                    SetClock(currentClock, timeClockA);
                    _addressSpace.SetByte(5, timeClockSwitch);
                }
            }
        }
    }
}