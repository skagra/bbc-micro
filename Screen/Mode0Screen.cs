using BbcMicro.Memory.Abstractions;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class Mode0Screen
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private WriteableBitmap writeableBitmap;
    private Window w;
    private Image i;
    private readonly IAddressSpace _addressSpace;

    public Window GetWindow()
    {
        return w;
    }

    public Mode0Screen(IAddressSpace addressSpace)
    {
        _addressSpace = addressSpace;

        i = new Image();
        RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(i, EdgeMode.Aliased);
        var m = i.RenderTransform.Value;

        m.Scale(2, 4);
        i.RenderTransform = new MatrixTransform(m);

        w = new Window
        {
            Width = 1280,
            Height = 1024,
            Content = i,
            BorderThickness = new Thickness(0)
        };

        writeableBitmap = new WriteableBitmap(

            (int)w.Width,
            (int)w.Height,
            96,
            96,
            PixelFormats.Bgr32,
            null);

        i.Source = writeableBitmap;

        i.Stretch = Stretch.Fill;
        i.HorizontalAlignment = HorizontalAlignment.Left;
        i.VerticalAlignment = VerticalAlignment.Top;

        w.Show();
    }

    public void StartScan()
    {
        Task.Run(() =>
        {
            while (true)
            {
                w.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DrawScreen();
                }));

                Thread.Sleep(100);
            }
        });
    }

    private static readonly byte[] _masks =
    {
        0b1000_0000,
        0b0100_0000,
        0b0010_0000,
        0b0001_0000,
        0b0000_1000,
        0b0000_0100,
        0b0000_0010,
        0b0000_0001
    };

    public void DrawScreen()
    {
        ushort screenBaseAddr = 0X3000;

        // Pixel colours

        int fgColor = 255 << 16; // R
        fgColor |= 255 << 8;   // G
        fgColor |= 255 << 0;   // B

        int bgColor = 0;

        try
        {
            var addr = screenBaseAddr;

            // Reserve the back buffer for updates.
            writeableBitmap.Lock();

            for (int row = 0; row < 32; row++)
            {
                for (int col = 0; col < 80; col++)
                {
                    for (int pixelByte = 0; pixelByte < 8; pixelByte++)
                    {
                        unsafe
                        {
                            var xPixelByteStart = col * 8;
                            // var y = 255 - (row * 8 + pixelByte);
                            var y = (row * 8 + pixelByte);
                            // Get a pointer to the back buffer.
                            IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                            // Grab current byte from screen memory
                            var currentByte = _addressSpace.GetByte(addr);

                            // For each pixel bit in the currrent byte
                            for (var bitPos = 0; bitPos < 8; bitPos++)
                            {
                                // Is the bit for the current pixed set?
                                var pixelIsSet = (currentByte & _masks[bitPos]) != 0;

                                var x = xPixelByteStart + bitPos;

                                // Find the address of the pixel to draw.
                                IntPtr pixAddr = pBackBuffer +
                                    (y * writeableBitmap.BackBufferStride) +
                                    (x * 4);

                                // Plot the pixel as either foreground or background
                                // depending on whether the pixel is set
                                if (pixelIsSet)
                                {
                                    *((int*)pixAddr) = fgColor;
                                }
                                else
                                {
                                    *((int*)pixAddr) = bgColor;
                                }
                            }
                        }
                        addr++;
                    }
                }
            }
            // Invalidate the whole rect
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, 640, 256));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            writeableBitmap.Unlock();
        }
    }

    public void DrawPixel(int x, int y)
    {
        int column = (int)x;
        int row = (int)y;

        try
        {
            // Reserve the back buffer for updates.
            writeableBitmap.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                // Find the address of the pixel to draw.
                pBackBuffer += row * writeableBitmap.BackBufferStride;
                pBackBuffer += column * 4;

                // Compute the pixel's color.
                int color_data = 255 << 16; // R
                color_data |= 0 << 8;   // G
                color_data |= 0 << 0;   // B

                // Assign the color data to the pixel.
                *((int*)pBackBuffer) = color_data;
            }

            // Specify the area of the bitmap that changed.
            writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            writeableBitmap.Unlock();
        }
    }
}