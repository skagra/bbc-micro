using System;
using System.Windows;
using BbcMicro.Memory;
using OS.Image;

namespace WriteableBitmapDemo
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var addressSpace = new FlatAddressSpace();
            var screen = new GenericScreen(addressSpace);

            Application app = new Application();

            screen.StartScan();

            var loader = new ROMLoader();
            loader.Load("/temp/display.bin", 0x3000, addressSpace);

            app.Run();
        }
    }
}