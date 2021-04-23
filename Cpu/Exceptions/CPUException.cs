using System;
using System.Runtime.Serialization;

namespace BbcMicro.Cpu.Exceptions
{
    public class CPUException : Exception
    {
        public CPUException()
        {
        }

        public CPUException(string message) : base(message)
        {
        }

        public CPUException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}