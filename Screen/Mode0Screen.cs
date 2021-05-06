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

    private WriteableBitmap _writeableBitmap;
    private readonly Window _window;
    private readonly Image _image;
    private readonly IAddressSpace _addressSpace;

    public Window GetWindow()
    {
        return _window;
    }

    private const int WINDOW_WIDTH = 1280;
    private const int WINDOW_HEIGHT = 1024;

    public Mode0Screen(IAddressSpace addressSpace)
    {
        _addressSpace = addressSpace;

        // Create an image control to hold the screen
        _image = new Image();
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

        // Have the image scale its content
        var transformationMatrix = _image.RenderTransform.Value;
        transformationMatrix.Scale(2, 4);
        _image.RenderTransform = new MatrixTransform(transformationMatrix);

        // Create top level window and add the image control
        _window = new Window
        {
            Width = WINDOW_WIDTH,
            Height = WINDOW_HEIGHT,
            Content = _image
        };

        // Create a writable bitmap to hold the screen
        _writeableBitmap = new WriteableBitmap(
            (int)_window.Width,
            (int)_window.Height,
            96,
            96,
            PixelFormats.Bgr32,
            null);

        // Assign the bitmap to the image control
        _image.Source = _writeableBitmap;

        _image.Stretch = Stretch.Fill;
        _image.HorizontalAlignment = HorizontalAlignment.Left;
        _image.VerticalAlignment = VerticalAlignment.Top;

        _window.Show();
    }

    public void StartScan()
    {
        Task.Run(() =>
        {
            while (true)
            {
                _window.Dispatcher.BeginInvoke(new Action(() =>
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
        fgColor |= 255 << 8;     // G
        fgColor |= 255 << 0;     // B

        int bgColor = 0;

        try
        {
            var addr = screenBaseAddr;

            // Reserve the back buffer for updates.
            _writeableBitmap.Lock();

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
                            IntPtr pBackBuffer = _writeableBitmap.BackBuffer;

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
                                    (y * _writeableBitmap.BackBufferStride) +
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
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, 640, 256));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            _writeableBitmap.Unlock();
        }
    }
}