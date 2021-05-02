using BbcMicro.Memory.Abstractions;

namespace BbcMicro.OS
{
    public class OperatingSystem
    {
        private readonly InterceptorDispatcher _interceptorDispatcher = new InterceptorDispatcher();

        private void InitOsValues(IAddressSpace addressSpace)
        {
            // Flag a hard reset
            //
            // On power on: 6522 System VIA IER bits 0 to 6 will all be clear
            // On BREAK   : 6522 System VIA IER bits 0 to 6 at least one bit will be set
            //
            // .systemVIAInterruptEnableRegister
            // https://tobylobster.github.io/mos/mos/S-s3.html#SP20
            // Bit 7 needs to be clear to flag a hard reset
            // https://tobylobster.github.io/mos/mos/S-s10.html#SP3
            addressSpace.SetByte(0x0, MemoryLocations.SYSTEM_VIA_INTERRUPT_ENABLE_REGISTER);
        }

        public OperatingSystem(IAddressSpace addressSpace, bool interceptIo = false)
        {
            InitOsValues(addressSpace);

            _interceptorDispatcher.AddInterceptor(EntryPoints.OSRDCH, Keyboard.OSRDCH);
            _interceptorDispatcher.AddInterceptor(EntryPoints.INTERROGATE_KEYBOARD, Keyboard.INTERROGATE_KEYBOARD);
            if (interceptIo)
            {
                _interceptorDispatcher.AddInterceptor(EntryPoints.OSWRCH, TextOutput.OSWRCH);
                _interceptorDispatcher.AddInterceptor(EntryPoints.OSASCI, TextOutput.OSASCI);
            }
            //_interceptorDispatcher.AddInterceptor(EntryPoints.VDUCHR, TextOutput.VDUCHR);
        }

        public InterceptorDispatcher InterceptorDispatcher { get { return _interceptorDispatcher; } }
    }
}