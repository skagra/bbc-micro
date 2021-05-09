using BbcMicro.Cpu;
using System.Threading;
using System.Threading.Tasks;

namespace BBCMicro
{
    public class TimerInterrupt
    {
        private readonly CPU _cpu;
        private const ushort systemVIAInterruptFlagRegister = 0XFE4D;
        private const ushort acia6850StatusRegister = 0XFE08;
        private const ushort timeClockSwitch = 0X0283;
        private const ushort timeClockA = 0X0292;
        private const ushort timeClockB = 0X0297;

        public TimerInterrupt(CPU cpu)
        {
            _cpu = cpu;
            cpu.Memory.SetByte(10, timeClockSwitch);
        }

        public void TriggerInterrupt()
        {
            _cpu.Memory.SetByte(0b0000_0000, acia6850StatusRegister);

            // 100Hz timer interrupt 0b1100_0000
            _cpu.Memory.SetByte(0b1100_0000, systemVIAInterruptFlagRegister);

            _cpu.TriggerIRQ();
        }

        public void StartTimer()
        {
            var addressSpace = _cpu.Memory;

            Task.Run(() =>
            {
                Thread.Sleep(10000);

                while (true)
                {
                    TriggerInterrupt();

                    Thread.Sleep(100);
                }
            });
        }
    }
}