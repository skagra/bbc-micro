namespace BbcMicro.Debugger
{
    public sealed class ProcessorState
    {
        public ushort PC { get; set; }

        public byte S { get; set; }

        public byte A { get; set; }

        public byte X { get; set; }

        public byte Y { get; set; }

        public byte P { get; set; }
    }
}