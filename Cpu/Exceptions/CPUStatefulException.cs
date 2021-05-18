using System;
using System.Linq;
using System.Text;
using static BbcMicro.Cpu.CPU;

namespace BbcMicro.Cpu.Exceptions
{
    public class CPUStatefulException : CPUException
    {
        private readonly CPU _cpu;
        private bool _dumpingCore;

        private void DumpCoreIfNeeded()
        {
            if (_dumpingCore)
            {
                CoreDumper.DumpCore(_cpu);
            }
        }

        public CPUStatefulException(CPU cpu, bool dumpCore = true)
        {
            _cpu = cpu;
            _dumpingCore = dumpCore;

            DumpCoreIfNeeded();
        }

        public CPUStatefulException(CPU cpu, string message, bool dumpCore = true) : base(message)
        {
            _cpu = cpu;
            _dumpingCore = dumpCore;

            DumpCoreIfNeeded();
        }

        public CPUStatefulException(CPU cpu, string message, Exception innerException, bool dumpCore = true) : base(message, innerException)
        {
            _cpu = cpu;
            _dumpingCore = dumpCore;

            DumpCoreIfNeeded();
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append($"{base.Message}{((_dumpingCore) ? " (Dumping core)" : "")}").AppendLine().AppendLine().
                Append("CPU").AppendLine().AppendLine().
                Append($"PC=0x{_cpu.PC:X4}, S=0x{_cpu.S:X2}").AppendLine().
                Append($"A=0x{_cpu.A:X2} X=0x{_cpu.X:X2} Y=0x{_cpu.Y:X2}").AppendLine().
                Append($"P=0x{_cpu.P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => (((_cpu.P & (byte)flag) != 0) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})").AppendLine().
                AppendLine().
                AppendLine("Memory").AppendLine().
                AppendLine(_cpu.Memory.ToString(_cpu.PC, 0x0F)).AppendLine().
                AppendLine().
                AppendLine("Stack Trace").AppendLine().
                AppendLine(StackTrace);

            if (InnerException != null)
            {
                result.AppendLine().
                    Append("Inner Exception").AppendLine().
                    AppendLine().
                    Append(InnerException);
            }

            return result.ToString();
        }
    }
}