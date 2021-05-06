using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;
using BbcMicro.Memory;
using OS.Image;

namespace WriteableBitmapDemo
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Random r = new Random();

            var addressSpace = new FlatAddressSpace();
            var screen = new Mode1Screen(addressSpace);

            Application app = new Application();

            screen.StartScan();

            var loader = new ROMLoader();
            loader.Load("/temp/display.bin", 0x3000, addressSpace);

            app.Run();
        }
    }
}