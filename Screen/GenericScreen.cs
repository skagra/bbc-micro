using BbcMicro.Memory.Abstractions;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class GenericScreen
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

    private readonly int _frameSleepTime;
    private int _mode = 0;

    public GenericScreen(IAddressSpace addressSpace, int frequency = 25)
    {
        _addressSpace = addressSpace;
        _frameSleepTime = (int)(1000.0 / (double)frequency);

        // Create an image control to hold the screen
        _image = new Image();

        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

        // Create top level window and add the image control
        _window = new Window
        {
            Width = WINDOW_WIDTH,
            Height = WINDOW_HEIGHT,
            Content = _image
        };

        // Create a writable bitmap to hold the screen
        _writeableBitmap = new WriteableBitmap(
            (int)_window.Width, (int)_window.Height,
            96, 96,
            PixelFormats.Bgr32,
            null);

        // Assign the bitmap to the image control
        _image.Source = _writeableBitmap;
        _image.Stretch = Stretch.Fill;
        _image.HorizontalAlignment = HorizontalAlignment.Left;
        _image.VerticalAlignment = VerticalAlignment.Top;
        _image.SnapsToDevicePixels = true;

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

                Thread.Sleep(_frameSleepTime);
            }
        });
    }

    // Pixel colors

    private const int RED = 255 << 16;
    private const int GREEN = 255 << 8;
    private const int BLUE = 255;
    private const int BLACK = 0;

    private static readonly int[] _eightPixelsPerByteColours = new int[] {
           BLACK,              // Black
           RED | GREEN | BLUE  // White
        };

    private static readonly int[] _fourPixelsPerByteColours = new int[] {
           BLACK,             // Black
           RED,               // Red
           RED | GREEN,       // Yellow
           RED | GREEN | BLUE // White
        };

    private static readonly int[] _twoPixelsPerByteColours = new int[] {
           BLACK,              // Black
           RED,                // Red
           GREEN,              // Green
           RED | GREEN,        // Yellow
           BLUE,               // Blue
           RED | BLUE,         // Magenta
           GREEN | BLUE,       // Cyan
           RED | BLUE | GREEN, // White
           // TODO: Flashing colours
           BLACK,              // Black
           RED,                // Red
           GREEN,              // Green
           RED | GREEN,        // Yellow
           BLUE,               // Blue
           RED | BLUE,         // Magenta
           GREEN | BLUE,       // Cyan
           RED | BLUE | GREEN  // White
        };

    // Masks to extract pixel values from pixel bytes
    private static readonly byte[] _eightPixelsPerByteMasks = new byte[] {
            0b1000_0000,
            0b0100_0000,
            0b0010_0000,
            0b0001_0000,
            0b0000_1000,
            0b0000_0100,
            0b0000_0010,
            0b0000_0001
        };

    private static readonly byte[] _fourPixelsPerByteMasks = new byte[] {
            0b1000_1000,
            0b0100_0100,
            0b0010_0010,
            0b0001_0001
        };

    private static readonly byte[] _twoPixelsPerByteMasks = new byte[] {
            0b1010_1010,
            0b0101_0101
        };

    private class ModeSettings
    {
        public ushort screenBaseAddress;
        public ushort pixelsPerByte;
        public int[] colours;
        public byte[] bitMasks;
        public int numberOfXPixels;
        public int numberOfYPixels;
        public MatrixTransform transformation;
    }

    private static readonly ModeSettings[] _modes = new ModeSettings[] {
        new ModeSettings { // MODE 0 - This could become size!
            screenBaseAddress = 0x3000,
            pixelsPerByte = 8,
            bitMasks = _eightPixelsPerByteMasks,
            colours = _eightPixelsPerByteColours,
            numberOfXPixels = 640,
            numberOfYPixels = 256,
            transformation=new MatrixTransform(new Matrix(2,0,0,4,0,0))
        },
        new ModeSettings { // MODE 1
            screenBaseAddress = 0x3000,
            pixelsPerByte = 4,
            bitMasks = _fourPixelsPerByteMasks,
            colours = _fourPixelsPerByteColours,
            numberOfXPixels = 320,
            numberOfYPixels = 256,
            transformation=new MatrixTransform(new Matrix(4,0,0,4,0,0))
        },
         new ModeSettings { // MODE 2
            screenBaseAddress = 0x3000,
            pixelsPerByte = 2,
            bitMasks = _twoPixelsPerByteMasks,
            colours = _twoPixelsPerByteColours,
            numberOfXPixels = 160,
            numberOfYPixels = 256,
            transformation=new MatrixTransform(new Matrix(8,0,0,4,0,0))
        },
         new ModeSettings { // MODE 3 - TODO Scrolling is screwed
            screenBaseAddress = 0x4000,
            pixelsPerByte = 8,
            bitMasks = _eightPixelsPerByteMasks,
            colours = _eightPixelsPerByteColours,
            numberOfXPixels = 640,
            numberOfYPixels = 200,
            transformation=new MatrixTransform(new Matrix(2,0,0,4,0,0))
        },
        new ModeSettings { // MODE 4
            screenBaseAddress = 0x5800,
            pixelsPerByte = 8,
            bitMasks = _eightPixelsPerByteMasks,
            colours = _eightPixelsPerByteColours,
            numberOfXPixels = 320,
            numberOfYPixels = 256,
            transformation=new MatrixTransform(new Matrix(4,0,0,4,0,0))
        },
         new ModeSettings { // MODE 5
            screenBaseAddress = 0x5800,
            pixelsPerByte = 4,
            bitMasks = _fourPixelsPerByteMasks,
            colours = _fourPixelsPerByteColours,
            numberOfXPixels = 160,
            numberOfYPixels = 256,
            transformation=new MatrixTransform(new Matrix(8,0,0,4,0,0))
        },
         new ModeSettings { // MODE 6 - TODO Scrolling is screwed
            screenBaseAddress = 0x6000,
            pixelsPerByte = 8,
            bitMasks = _eightPixelsPerByteMasks,
            colours = _eightPixelsPerByteColours,
            numberOfXPixels = 320,
            numberOfYPixels = 200,
            transformation=new MatrixTransform(new Matrix(4,0,0,4,0,0))
        }
    };

    private int GetColor(byte pixelsByte, int bitOffsetFromMsb)
    {
        // Masked color index with values of interest in MSBs of both nibbles
        var maskedColIndex = (pixelsByte & _modes[_mode].bitMasks[bitOffsetFromMsb]) << bitOffsetFromMsb;

        int colorIndex = 0;

        switch (_modes[_mode].pixelsPerByte)
        {
            case 8:
                colorIndex = maskedColIndex >> 7;
                break;

            case 4:
                colorIndex = ((maskedColIndex & 0b1000_0000) >> 6) |
                    ((maskedColIndex & 0b0000_1000) >> 3);
                break;

            case 2:
                colorIndex = ((maskedColIndex & 0b1000_0000) >> 4) |
                    ((maskedColIndex & 0b0010_0000) >> 3) |
                    ((maskedColIndex & 0b0000_1000) >> 2) |
                    ((maskedColIndex & 0b0000_0010) >> 1);
                break;
        }

        return _modes[_mode].colours[colorIndex];
    }

    public void DrawScreen()
    {
        _mode = _addressSpace.GetByte(BbcMicro.OS.MemoryLocations.VDU_CURRENT_SCREEN_MODE);

        try
        {
            _image.RenderTransform = _modes[_mode].transformation;

            var addr = _modes[_mode].screenBaseAddress;

            // Reserve the back buffer for updates.
            _writeableBitmap.Lock();

            // 64 rows of 4 pixels = 256 y pixels
            // So set this to num pixels / pixels per byte
            // Really is the genuine number of "cell" rows
            for (int row = 0; row < _modes[_mode].numberOfYPixels / _modes[_mode].pixelsPerByte; row++)
            {
                // 80 columns of 4 pixels = 320 x pixels
                // Set this to num pixels / pixels per byte
                // Really is the genuine number of cell columns
                for (int col = 0; col < _modes[_mode].numberOfXPixels / _modes[_mode].pixelsPerByte; col++)
                {
                    // Each cell always contains 8 bytes - and always move y by 1
                    // independent of the number of bits per pixel (it is x that varies)
                    for (int pixelByte = 0; pixelByte < 8; pixelByte++)
                    {
                        unsafe
                        {
                            // The xcoord is common to all these 8 bytes
                            // column * pixels per byte
                            var xPixelByteStart = col * _modes[_mode].pixelsPerByte;

                            // Each cell always cover 8 in y dimension
                            var y = row * 8 + pixelByte;

                            // Get a pointer to the back buffer.
                            IntPtr pBackBuffer = _writeableBitmap.BackBuffer;

                            // Grab current byte from screen memory
                            var currentByte = _addressSpace.GetByte(addr);

                            // For each pixel bit in the currrent byte
                            for (var bitPos = 0; bitPos < _modes[_mode].pixelsPerByte; bitPos++)
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