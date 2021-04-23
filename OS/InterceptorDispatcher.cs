using BbcMicro.Cpu;
using System;
using System.Collections.Generic;

namespace BbcMicro.OS
{
    public sealed class InterceptorDispatcher
    {
        private readonly Dictionary<ushort, Func<CPU, OpCode, AddressingMode, ushort, bool>> _interceptors =
            new Dictionary<ushort, Func<CPU, OpCode, AddressingMode, ushort, bool>>();

        public void AddInterceptor(ushort vector, Func<CPU, OpCode, AddressingMode, ushort, bool> interceptor)
        {
            _interceptors.Add(vector, interceptor);
        }

        public bool Dispatch(CPU cpu, OpCode opCode, AddressingMode addressingMode, ushort operand)
        {
            var handled = false;

            if (opCode == OpCode.JSR)
            {
                Func<CPU, OpCode, AddressingMode, ushort, bool> interceptor;

                if (_interceptors.TryGetValue(operand, out interceptor))
                {
                    handled = interceptor(cpu, opCode, addressingMode, operand);
                }
            }

            return handled;
        }
    }
}