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
            var ts = new Mode0Screen(addressSpace);

            Application app = new Application();

            //ts.w.KeyDown += new KeyEventHandler((sender, b) =>
            //{
            //    //DrawPixel(r.Next(256), r.Next(160));
            //});

            ts.StartScan();

            var loader = new ROMLoader();
            loader.Load("/temp/display.bin", 0x3000, addressSpace);

            r = new Random();
            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        addressSpace.SetByte((byte)r.Next(255), (ushort)(r.Next(20000) + 0x3000));
            //        Thread.Sleep(1000);
            //    }
            //});

            app.Run();
        }
    }
}