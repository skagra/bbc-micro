using BbcMicro.Memory.Abstractions;
using System.Collections.Generic;
using NLog;

namespace BbcMicro.Diagnostics
{
    public sealed class MemoryMonitor
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private class MonitorPoint
        {
            public ushort start;
            public ushort end;
            public string tag;
        }

        private readonly List<MonitorPoint> _monitorPoints = new List<MonitorPoint>();

        public MemoryMonitor(IAddressSpace addressSpace)
        {
            addressSpace.AddSetByteCallback((nVal, oVal, addr) =>
            {
                foreach (var mon in _monitorPoints)
                {
                    if (addr >= mon.start && addr <= mon.end)
                    {
                        _logger.Debug($"{mon.tag} {addr:X4} <- {nVal:X2} ({oVal:X2})");
                    }
                }
            });
        }

        public void AddMonitor(string tag, ushort addr)
        {
            AddMonitor(tag, addr, addr);
        }

        public void AddMonitor(string tag, ushort start, ushort end)
        {
            _monitorPoints.Add(new MonitorPoint { start = start, end = end, tag = tag });
        }
    }
}