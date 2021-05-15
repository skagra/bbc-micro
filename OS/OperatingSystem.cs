using BbcMicro.Memory.Abstractions;
using BbcMicro.WPF;
using BbcMicro.SystemConstants;

namespace BbcMicro.OS
{
    public sealed class OperatingSystem
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
            addressSpace.SetByte(0x0, (ushort)VIA.systemViaInterruptEnableRegister);
        }

        public OperatingSystem(IAddressSpace addressSpace, OSMode osMode, WPFKeyboardEmu keyboardEmu = null)
        {
            InitOsValues(addressSpace);

            if (osMode == OSMode.WPF)
            {
                var keyboardInput = new KeyboardInput(keyboardEmu);
                _interceptorDispatcher.AddInterceptor(EntryPoints.INTERROGATE_KEYBOARD, keyboardInput.INTERROGATE_KEYBOARD_WFP);
                // _interceptorDispatcher.AddInterceptor(EntryPoints.OSRDCH, keyboardInput.OSRDCH_WPF);
            }
            else
            {
                var keyboardInput = new KeyboardInput();
                _interceptorDispatcher.AddInterceptor(EntryPoints.INTERROGATE_KEYBOARD, keyboardInput.INTERROGATE_KEYBOARD_CONSOLE);
                _interceptorDispatcher.AddInterceptor(EntryPoints.OSRDCH, keyboardInput.OSRDCH_CONSOLE);
            }

            if (osMode == OSMode.Debug)
            {
                _interceptorDispatcher.AddInterceptor(EntryPoints.OSWRCH, TextOutput.OSWRCH);
                _interceptorDispatcher.AddInterceptor(EntryPoints.OSASCI, TextOutput.OSASCI);
            }
        }

        public InterceptorDispatcher InterceptorDispatcher { get { return _interceptorDispatcher; } }
    }
}