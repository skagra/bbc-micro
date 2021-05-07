using BbcMicro.Cpu;
using NLog;
using System.Collections.Generic;

namespace BbcMicro.Diagnostics
{
    public sealed class CpuAddressMonitor
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly List<(string tag, ushort address)> _monitors = new List<(string tag, ushort address)>();

        public CpuAddressMonitor(CPU cpu)
        {
            cpu.AddPreExecutionCallback(Callaback);
        }

        private void Callaback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            foreach (var monitor in _monitors)
            {
                if (cpu.PC == monitor.address)
                {
                    _logger.Debug($"{monitor.tag} {cpu}");
                    break;
                }
            }
        }

        public void AddMonitor(string tag, ushort address)
        {
            _monitors.Add((tag: tag, address: address));
        }
    }
}