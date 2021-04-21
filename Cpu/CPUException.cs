using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using static BbcMicro.Cpu.CPU;

namespace BbcMicro.Cpu
{
    public class CPUException : Exception
    {
        private ushort PC { get; set; }

        private byte S { get; set; }

        private byte A { get; set; }

        private byte X { get; set; }

        private byte Y { get; set; }

        private byte P { get; set; }

        private String _memory { get; set; }

        private void CopyState(CPU cpu)
        {
            PC = cpu.PC;
            S = cpu.S;
            A = cpu.A;
            X = cpu.X;
            Y = cpu.Y;
            P = cpu.P;

            _memory = cpu.Memory.ToString(cpu.PC, 0x0F);
        }

        public CPUException(CPU cpu)
        {
            CopyState(cpu);
        }

        public CPUException(CPU cpu, string message) : base(message)
        {
            CopyState(cpu);
        }

        public CPUException(CPU cpu, string message, Exception innerException) : base(message, innerException)
        {
            CopyState(cpu);
        }

        protected CPUException(CPU cpu, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CopyState(cpu);
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append($"{base.Message}").AppendLine().AppendLine().
                Append("CPU").AppendLine().
                Append($"PC=0x{PC:X4}, S=0x{S:X2}").AppendLine().
                Append($"A=0x{A:X2} X=0x{X:X2} Y=0x{Y:X2}").AppendLine().
                Append($"P=0x{P:X2} ({string.Join("", ((PFlags[])Enum.GetValues(typeof(PFlags))).ToList().Select(flag => (((P & (byte)flag) != 0) ? flag.ToString() : flag.ToString().ToLower())).Reverse())})").AppendLine().
                AppendLine().
                AppendLine("Memory").AppendLine().
                AppendLine(_memory).AppendLine().
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

// Core dump
// Flags representation