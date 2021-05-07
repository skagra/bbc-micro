using BbcMicro.Cpu;
using NLog;
using System.Collections.Generic;

namespace BbcMicro.Diagnostics
{
    public sealed class CpuMonitor
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<ushort, string> _monitors = new Dictionary<ushort, string>();

        public CpuMonitor(CPU cpu)
        {
            cpu.AddPreExecutionCallback(Callaback);
        }

        private void Callaback(CPU cpu, OpCode opCode, AddressingMode addressingMode)
        {
            if (_monitors.TryGetValue(cpu.PC, out var tag))
            {
                _logger.Debug($"{tag} {cpu}");
            }
        }

        public void AddMonitor(string tag, ushort address)
        {
            _monitors[address] = tag;
        }
    }
}