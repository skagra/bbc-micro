using BbcMicro.Memory.Abstractions;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class Mode1Screen
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

    public Mode1Screen(IAddressSpace addressSpace)
    {
        _addressSpace = addressSpace;

        // Create an image control to hold the screen
        _image = new Image();
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

        // Have the image scale its content
        var transformationMatrix = _image.RenderTransform.Value;
        transformationMatrix.Scale(4, 4); // CHANGED SCALING PER MODE
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

    // Pixel colors
    private int[] colors = new int[] {
           0,                            // Black
           255 << 16,                    // Red
           (255 << 16) | (255<<8),       // Yellow
           (255 << 16) | (255<<8) | 255  // White
        };

    // Map of bit mask to color
    private byte[] bitMasks = new byte[] {
            0b1000_1000,
            0b0100_0100,
            0b0010_0010,
            0b0001_0001
        };

    private int GetColor(byte pixelsByte, int bitOffsetFromMsb)
    {
        // Maked color index with values of interest in MSBs of both nibbles
        var maskedColIndex = (pixelsByte & bitMasks[bitOffsetFromMsb]) << bitOffsetFromMsb;

        var colorIndex = ((maskedColIndex & 0b1000_0000) >> 6) |
            ((maskedColIndex & 0b0000_1000) >> 3);

        return colors[colorIndex];
    }

    public void DrawScreen()
    {
        ushort screenBaseAddr = 0X3000;

        try
        {
            var addr = screenBaseAddr;

            // Reserve the back buffer for updates.
            _writeableBitmap.Lock();

            // 64 rows of 4 pixels = 256 y pixels
            // So set this to num pixels / pixels per byte
            // Really is the genuine number of "cell" rows
            for (int row = 0; row < 64; row++)
            {
                // 80 columns of 4 pixels = 320 x pixels
                // Set this to num pixels / pixels per byte
                // Really is the genuine number of cell columns
                for (int col = 0; col < 80; col++)
                {
                    // Each cell always contains 8 bytes - and always move y by 1
                    // independent of the number of bits per pixel (it is x that varies)
                    for (int pixelByte = 0; pixelByte < 8; pixelByte++)
                    {
                        unsafe
                        {
                            // The xcoord is common to all these 8 bytes
                            // column * pixels per byte
                            var xPixelByteStart = col * 4;

                            // Each cell always cover 8 in y dimension
                            var y = row * 8 + pixelByte;

                            // Get a pointer to the back buffer.
                            IntPtr pBackBuffer = _writeableBitmap.BackBuffer;

                            // Grab current byte from screen memory
                            var currentByte = _addressSpace.GetByte(addr);

                            // For each pixel bit in the currrent byte
                            for (var bitPos = 0; bitPos < 4; bitPos++)
                            {
                                var color = GetColor(currentByte, bitPos);

                                var x = xPixelByteStart + bitPos;

                                // Find the address of the pixel to draw.
                                IntPtr pixAddr = pBackBuffer +
                                    (y * _writeableBitmap.BackBufferStride) +
                                    (x * 4);

                                // Plot the pixel
                                *((int*)pixAddr) = color;
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