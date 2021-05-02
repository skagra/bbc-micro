﻿namespace BbcMicro.OS
{
    public class OperatingSystem
    {
        private readonly InterceptorDispatcher _interceptorDispatcher = new InterceptorDispatcher();

        public OperatingSystem(bool interceptIo = false)
        {
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