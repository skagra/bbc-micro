namespace BbcMicro.OS
{
    public class OperatingSystem
    {
        private readonly InterceptorDispatcher _interceptorDispatcher = new InterceptorDispatcher();

        public OperatingSystem()
        {
            _interceptorDispatcher.AddInterceptor(EntryPoints.OSWRCH, TextOutput.OSWRCH);
        }

        public InterceptorDispatcher InterceptorDispatcher { get { return _interceptorDispatcher; } }
    }
}