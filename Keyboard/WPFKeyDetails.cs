using System.Windows.Input;

namespace BbcMicro.WPF
{
    public sealed class WPFKeyDetails
    {
        public Key Key { get; set; }

        public ModifierKeys Modifiers { get; set; }

        public bool CapsLock { get; set; }
    }
}