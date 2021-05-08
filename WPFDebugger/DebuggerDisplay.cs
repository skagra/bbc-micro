using BbcMicro.Cpu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace BbcMicro.WPFDebugger
{
    public sealed class DebuggerDisplay
    {
        private const double BORDER_THICKNESS = 1;
        private const double TITLE_FONT_SIZE = 20;
        private const double MAIN_FONT_SIZE = 16;
        private const int SCROLL_BUFFER_SIZE = 500;
        private const int STANDARD_MARGIN_SIZE = 10;
        private const int STANDARD_PADDING_SIZE = 10;

        private readonly Thickness _zeroThickness = new Thickness(0);
        private readonly Thickness _standardMargin = new Thickness(STANDARD_MARGIN_SIZE);
        private readonly Thickness _standardPadding = new Thickness(STANDARD_PADDING_SIZE);
        private readonly Thickness _borderThickness = new Thickness(BORDER_THICKNESS);
        private readonly FontFamily _mainFont = new FontFamily("Courier New");
        private readonly SolidColorBrush _fgBrush = Brushes.White;
        private readonly SolidColorBrush _bgBrush = Brushes.Black;
        private readonly SolidColorBrush _fgChangedBrush = Brushes.Yellow;
        private readonly SolidColorBrush _fgErrorBrush = Brushes.Red;
        private readonly SolidColorBrush _controlBackground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        private readonly Brush _backgroundedBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70));

        private Window _window;

        private FlowDocument _mainMessageArea;
        private ListView _disDisplay;
        private ListView _memDisplay;
        private ListView _stackDisplay;
        private TextBlock _pcDisplay;
        private TextBlock _sDisplay;
        private TextBlock _aDisplay;
        private TextBlock _xDisplay;
        private TextBlock _yDisplay;
        private TextBlock _pDisplay;
        private TextBox _inputBox;

        #region UI Creation

        /*
         * Utility methods --->
         */

        private TextBox MakeTitle(string title)
        {
            return new TextBox
            {
                Text = title,
                FontFamily = _mainFont,
                Background = _bgBrush,
                Foreground = _fgBrush,
                FontWeight = FontWeights.Bold,
                FontSize = TITLE_FONT_SIZE,
                Margin = new Thickness(STANDARD_MARGIN_SIZE, STANDARD_MARGIN_SIZE, STANDARD_MARGIN_SIZE, 0),
                BorderThickness = _zeroThickness
            };
        }

        private StackPanel StackWithTitle(string title, UIElement child)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            stackPanel.Children.Add(MakeTitle(title));
            stackPanel.Children.Add(child);
            return stackPanel;
        }

        /*
         * <--- Utility methods
         */

        /*
         * CPU Registers --->
         */

        private (TextBlock textBlock, Border container) CreateCpuReg()
        {
            var textBlock = new TextBlock
            {
                FontFamily = _mainFont,
                Background = _controlBackground,
                Foreground = _fgBrush,
                FontSize = MAIN_FONT_SIZE,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = _standardPadding
            };

            var border = new Border
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = textBlock,
                BorderBrush = _fgBrush,
                BorderThickness = _borderThickness
            };

            return (textBlock: textBlock, container: border);
        }

        private StackPanel CreateCpuDisplay()
        {
            var cpuGrid = new Grid
            {
                ShowGridLines = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = _standardMargin
            };

            cpuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cpuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cpuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cpuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cpuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cpuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cpuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cpuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            cpuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            (var pcText, var pcBorder) = CreateCpuReg();
            _pcDisplay = pcText;
            cpuGrid.Children.Add(pcBorder);
            Grid.SetRow(pcBorder, 0);
            Grid.SetColumn(pcBorder, 0);
            Grid.SetColumnSpan(pcBorder, 3);

            (var sText, var sBorder) = CreateCpuReg();
            _sDisplay = sText;
            cpuGrid.Children.Add(sBorder);
            Grid.SetRow(sBorder, 0);
            Grid.SetColumn(sBorder, 3);
            Grid.SetColumnSpan(sBorder, 3);

            (var aText, var aBorder) = CreateCpuReg();
            _aDisplay = aText;
            cpuGrid.Children.Add(aBorder);
            Grid.SetRow(aBorder, 1);
            Grid.SetColumn(aBorder, 0);
            Grid.SetColumnSpan(aBorder, 2);

            (var xText, var xBorder) = CreateCpuReg();
            _xDisplay = xText;
            cpuGrid.Children.Add(xBorder);
            Grid.SetRow(xBorder, 1);
            Grid.SetColumn(xBorder, 2);
            Grid.SetColumnSpan(xBorder, 2);

            (var yText, var yBorder) = CreateCpuReg();
            _yDisplay = yText;
            cpuGrid.Children.Add(yBorder);
            Grid.SetRow(yBorder, 1);
            Grid.SetColumn(yBorder, 4);
            Grid.SetColumnSpan(yBorder, 2);

            (var pText, var pBorder) = CreateCpuReg();
            _pDisplay = pText;
            cpuGrid.Children.Add(pBorder);
            Grid.SetRow(pBorder, 2);
            Grid.SetColumn(pBorder, 0);
            Grid.SetColumnSpan(pBorder, 6);

            return StackWithTitle("CPU", cpuGrid);
        }

        /*
         * <--- CPU Registers
         */

        /*
         * Main message window --->
         */

        private FlowDocumentScrollViewer CreateMessageDisplay()
        {
            _mainMessageArea = new FlowDocument
            {
                FontFamily = _mainFont,
                Background = _controlBackground,
                Foreground = _fgBrush,
                FontSize = MAIN_FONT_SIZE
            };

            var mainMessageScroller = new FlowDocumentScrollViewer
            {
                Document = _mainMessageArea,
                Margin = _standardMargin,
                BorderBrush = _fgBrush,
                BorderThickness = _borderThickness
            };

            return mainMessageScroller;
        }

        /*
         * <-- Main message window
         */

        /*
         * Disassembly window --->
         */

        private StackPanel CreateDisDisplay()
        {
            _disDisplay = new ListView
            {
                FontFamily = _mainFont,
                Background = _controlBackground,
                Foreground = _fgBrush,
                FontSize = MAIN_FONT_SIZE,
                Margin = _standardMargin,
                Height = 280
            };

            return StackWithTitle("Disassembly", _disDisplay);
        }

        /*
         * <--- Disassembly window
         */

        /*
         * Stack window --->
         */

        private StackPanel CreateStackDisplay()
        {
            _stackDisplay = new ListView
            {
                FontFamily = _mainFont,
                Background = _controlBackground,
                Foreground = _fgBrush,
                FontSize = MAIN_FONT_SIZE,
                Margin = _standardMargin,
                Height = 107
            };

            return StackWithTitle("Stack", _stackDisplay); ;
        }

        /*
         * <--- Stack window
         */

        /*
         * Memory window --->
         */

        private StackPanel CreateMemDisplay()
        {
            _memDisplay = new ListView
            {
                FontFamily = _mainFont,
                Background = _controlBackground,
                Foreground = _fgBrush,
                FontSize = MAIN_FONT_SIZE,
                Margin = _standardMargin,
                Height = 280
            };

            return StackWithTitle("Memory", _memDisplay);
        }

        /*
         * <--- Memory window
         */

        private TextBox CreateInputBox()
        {
            _inputBox = new TextBox
            {
                BorderBrush = _fgBrush,
                BorderThickness = _borderThickness,
                FontFamily = _mainFont,
                FontSize = MAIN_FONT_SIZE,
                Background = _controlBackground,
                Foreground = _fgBrush,
                Height = MAIN_FONT_SIZE + STANDARD_PADDING_SIZE,
                Margin = new Thickness(STANDARD_MARGIN_SIZE, 0, STANDARD_MARGIN_SIZE, 0),
                Padding = new Thickness(STANDARD_PADDING_SIZE, 0, STANDARD_PADDING_SIZE, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            _inputBox.KeyDown += new KeyEventHandler((sender, e) =>
            {
                if (e.Key == Key.Return)
                {
                    var textBox = ((TextBox)sender);
                    var text = textBox.Text.Trim();
                    if (text.Length > 0)
                    {
                        AddMessage(text);
                    }
                    textBox.Clear();
                    e.Handled = true;

                    Task.Run(() =>
                    {
                        foreach (var cb in _callbacks)
                        {
                            cb(text, this);
                        }
                    });
                }
            });

            return _inputBox;
        }

        private List<Action<string, DebuggerDisplay>> _callbacks = new List<Action<string, DebuggerDisplay>>();

        public void AddCommandCallback(Action<string, DebuggerDisplay> callback)
        {
            _callbacks.Add(callback);
        }

        /*
         * Overall UI set up --->
         */

        private void SetUpDisplay()
        {
            var cpuStack = CreateCpuDisplay();
            var messageDisplay = CreateMessageDisplay();
            var disStack = CreateDisDisplay();
            var stackStack = CreateStackDisplay();
            var memStack = CreateMemDisplay();
            var inputBox = CreateInputBox();

            var mainGrid = new Grid { Background = _bgBrush };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.6, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 330 });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });

            mainGrid.Children.Add(disStack);
            Grid.SetRow(disStack, 0);
            Grid.SetColumn(disStack, 0);

            var psStack = new StackPanel();
            psStack.Children.Add(cpuStack);
            psStack.Children.Add(stackStack);

            mainGrid.Children.Add(psStack);
            Grid.SetRow(psStack, 0);
            Grid.SetColumn(psStack, 1);

            mainGrid.Children.Add(memStack);
            Grid.SetRow(memStack, 0);
            Grid.SetColumn(memStack, 2);

            mainGrid.Children.Add(messageDisplay);
            Grid.SetRow(messageDisplay, 1);
            Grid.SetColumn(messageDisplay, 0);
            Grid.SetColumnSpan(messageDisplay, 3);

            var outerGrid = new Grid
            {
                Margin = _zeroThickness
            };
            outerGrid.RowDefinitions.Add(new RowDefinition());
            outerGrid.RowDefinitions.Add(new RowDefinition { MaxHeight = 40 });
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition());

            outerGrid.Children.Add(mainGrid);
            Grid.SetColumn(mainGrid, 0);
            Grid.SetRow(mainGrid, 0);

            outerGrid.Children.Add(inputBox);
            Grid.SetColumn(inputBox, 0);
            Grid.SetRow(inputBox, 1);

            _window = new Window
            {
                Title = "BBC Microcomputer Emulator Debugger",
                Content = outerGrid,
                Background = _bgBrush,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 1100,
                Height = 800,
                // TODO
                //   Icon = new BitmapImage(new Uri("pack://application:,,,/BbcMicro.WPFDebugger;component/debuggericon.png", UriKind.RelativeOrAbsolute)),
                SnapsToDevicePixels = true
            };

            _window.Closing += new CancelEventHandler((source, e) =>
            {
                _window.Hide();
                e.Cancel = true;
            });
        }

        /*
         * <--- Overall UI set up
         */

        #endregion UI Creation

        #region UI Behaviour

        public void Background()
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                _disDisplay.Background = _backgroundedBrush;
                _memDisplay.Background = _backgroundedBrush;
                _stackDisplay.Background = _backgroundedBrush;
                _mainMessageArea.Background = _backgroundedBrush;
                _pcDisplay.Background = _backgroundedBrush;
                _sDisplay.Background = _backgroundedBrush;
                _aDisplay.Background = _backgroundedBrush;
                _xDisplay.Background = _backgroundedBrush;
                _yDisplay.Background = _backgroundedBrush;
                _pDisplay.Background = _backgroundedBrush;
            }));
        }

        public void Foreground()
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                _disDisplay.Background = _controlBackground;
                _memDisplay.Background = _controlBackground;
                _stackDisplay.Background = _controlBackground;
                _mainMessageArea.Background = _controlBackground;
                _pcDisplay.Background = _controlBackground;
                _sDisplay.Background = _controlBackground;
                _aDisplay.Background = _controlBackground;
                _xDisplay.Background = _controlBackground;
                _yDisplay.Background = _controlBackground;
                _pDisplay.Background = _controlBackground;
            }));
        }

        public class RefString
        {
            private string _val;

            public RefString(string val)
            {
                _val = val;
            }

            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj);
            }

            public override string ToString()
            {
                return _val;
            }
        }

        public void AddDis(ushort address, byte[] memory, string dis)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disDisplay.Items.Count > SCROLL_BUFFER_SIZE)
                {
                    _disDisplay.Items.RemoveAt(0);
                }
                var formattedMemory = string.Join(" ", memory.Select(m => $"0x{m:X2}"));
                _disDisplay.Items.Add(new RefString($"0x{address,-7:X4} {formattedMemory,-16} {dis}"));
                _disDisplay.UpdateLayout();
                _disDisplay.ScrollIntoView(_disDisplay.Items.GetItemAt(_disDisplay.Items.Count - 1));
            }));
        }

        public void AddMem(byte oldVal, byte newVal, ushort address)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_memDisplay.Items.Count > SCROLL_BUFFER_SIZE)
                {
                    _memDisplay.Items.RemoveAt(0);
                }

                _memDisplay.Items.Add(new RefString($"0x{address:X4} <- 0x{newVal:X2} (0x{oldVal:X2})"));

                _memDisplay.ScrollIntoView(_memDisplay.Items.GetItemAt(_memDisplay.Items.Count - 1));
            }));
        }

        public void UpdateStack(byte[] stack)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                _stackDisplay.Items.Clear();
                for (int i = 0; i < stack.Length; i++)
                {
                    _stackDisplay.Items.Add(new RefString($"{i,3} 0x01{(0xFF - stack.Length + i + 1):X2} 0x{stack[i]:X2}"));
                }
            }));
        }

        public DebuggerDisplay()
        {
            SetUpDisplay();
            _window.Show();
        }

        public void Show()
        {
            _window.Show();
        }

        public void Hide()
        {
            _window.Hide();
        }

        private void ParagraphLoaded(object sender, RoutedEventArgs e)
        {
            Paragraph paragraph = (Paragraph)sender;
            paragraph.BringIntoView();
            paragraph.Loaded -= ParagraphLoaded;
        }

        public void AddMessage(string message, bool emphasis = false, bool error = false)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                Paragraph p = new Paragraph();
                p.Margin = _zeroThickness;

                var run = new Run(message);

                if (emphasis)
                {
                    run.FontWeight = FontWeights.Bold;
                }

                if (error)
                {
                    run.Foreground = _fgErrorBrush;
                }

                p.Inlines.Add(run);

                _mainMessageArea.Blocks.Add(p);

                p.Loaded += new RoutedEventHandler(ParagraphLoaded);
            }));
        }

        private ProcessorState _prevState;

        private void SetRegister(string value, bool changed, TextBlock control)
        {
            control.Inlines.Clear();
            AppendRegister(value, changed, control);
        }

        private void AppendRegister(string value, bool changed, TextBlock control)
        {
            control.Inlines.Add(GetRun(value, changed));
        }

        private Run GetRun(string value, bool changed)
        {
            var result = new Run(value);
            if (changed)
            {
                result.Foreground = _fgChangedBrush;
            }
            else
            {
                result.Foreground = _fgBrush;
            }
            return result;
        }

        public void UpdateCpu(ProcessorState cpuState, byte[] stack)
        {
            if (_prevState == null)
            {
                _prevState = new ProcessorState
                {
                    PC = cpuState.PC,
                    S = cpuState.S,
                    A = cpuState.A,
                    X = cpuState.X,
                    Y = cpuState.Y,
                    P = cpuState.P
                };
            }

            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetRegister($"PC 0x{cpuState.PC:X4}", cpuState.PC != _prevState.PC, _pcDisplay);
                SetRegister($"S 0x{cpuState.S:X2}", cpuState.S != _prevState.S, _sDisplay);
                SetRegister($"A 0x{cpuState.A:X2}", cpuState.A != _prevState.A, _aDisplay);
                SetRegister($"X 0x{cpuState.X:X2}", cpuState.X != _prevState.X, _xDisplay);
                SetRegister($"Y 0x{cpuState.Y:X2}", cpuState.Y != _prevState.Y, _yDisplay);
                SetRegister($"P 0x{cpuState.P:X2} ", cpuState.P != _prevState.P, _pDisplay);

                var flags = new List<CPU.PFlags>((CPU.PFlags[])Enum.GetValues(typeof(CPU.PFlags)));
                flags.Reverse();
                var changedFlags = (byte)(cpuState.P ^ _prevState.P);

                flags.ForEach(f =>
                    AppendRegister(((cpuState.P & (byte)f) != 0) ?
                        f.ToString() : f.ToString().ToLower(),
                        (changedFlags & ((byte)f)) != 0,
                        _pDisplay)
                    );
                _prevState = new ProcessorState
                {
                    PC = cpuState.PC,
                    S = cpuState.S,
                    A = cpuState.A,
                    X = cpuState.X,
                    Y = cpuState.Y,
                    P = cpuState.P
                };
            }));
        }

        #endregion UI Behaviour
    }
}