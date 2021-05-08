using BbcMicro.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace BbcMicro.WPFDebugger
{
    public sealed class Debugger
    {
        private Window _window;

        private FlowDocument _mainMessageArea;
        private FlowDocumentScrollViewer _mainMessageScroller;
        private ListView _disDisplay;
        private ListView _memDisplay;

        private readonly FontFamily _mainFont = new FontFamily("Courier New");
        private readonly SolidColorBrush _fgBrush = Brushes.White;
        private readonly SolidColorBrush _bgBrush = Brushes.Black;
        private readonly SolidColorBrush _changedBrush = Brushes.Yellow;
        private readonly SolidColorBrush _errorBrush = Brushes.Red;

        private double _mainFontSize = 16;

        private TextBlock _pcDisplay;
        private TextBlock _sDisplay;
        private TextBlock _aDisplay;
        private TextBlock _xDisplay;
        private TextBlock _yDisplay;
        private TextBlock _pDisplay;

        private (TextBlock text, Border border) CreateCpuReg()
        {
            var textBlock = new TextBlock
            {
                FontFamily = _mainFont,
                Background = _bgBrush,
                Foreground = _fgBrush,
                FontSize = _mainFontSize,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5)
            };

            var border = new Border
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = textBlock,
                Margin = new Thickness(1)
            };

            return (textBlock, border);
        }

        private (Grid cpuGrid, StackPanel container) CreateCpuDisplay()
        {
            var cpuGrid = new Grid
            {
                ShowGridLines = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brushes.White,
                Margin = new Thickness(10)
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

            return (cpuGrid: cpuGrid, container: StackWithTitle("CPU", cpuGrid));
        }

        private FlowDocumentScrollViewer CreateMessageDisplay()
        {
            _mainMessageArea = new FlowDocument
            {
                FontFamily = _mainFont,
                Background = _bgBrush,
                Foreground = _fgBrush,
                FontSize = _mainFontSize
            };

            _mainMessageScroller = new FlowDocumentScrollViewer
            {
                Document = _mainMessageArea,
            };

            return _mainMessageScroller;
        }

        public void AddDis(ushort address, byte[] memory, string dis)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disDisplay.Items.Count > 500)
                {
                    _disDisplay.Items.RemoveAt(0);
                }
                var formattedMemory = string.Join(" ", memory.Select(m => $"0x{m:X2}"));
                _disDisplay.Items.Add($"0x{address,-7:X4} {formattedMemory,-16} {dis}");

                _disDisplay.ScrollIntoView(_disDisplay.Items.GetItemAt(_disDisplay.Items.Count - 1));
            }));
        }

        private TextBox MakeTitle(string title)
        {
            return new TextBox
            {
                Text = title,
                FontFamily = _mainFont,
                Background = _bgBrush,
                Foreground = _fgBrush,
                FontWeight = FontWeights.Bold,
                FontSize = 20,
                Margin = new Thickness(10, 10, 10, 0),
                BorderThickness = new Thickness(0)
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

        private (ListView disView, StackPanel container) CreateDisDisplay()
        {
            var disView = new ListView
            {
                FontFamily = _mainFont,
                Background = _bgBrush,
                Foreground = _fgBrush,
                FontSize = _mainFontSize,
                Margin = new Thickness(10),
                Height = 200
            };

            return (disView: disView, container: StackWithTitle("Disassembly", disView));
        }

        private (ListView memView, StackPanel container) CreateMemDisplay()
        {
            var memView = new ListView
            {
                FontFamily = _mainFont,
                Background = _bgBrush,
                Foreground = _fgBrush,
                FontSize = _mainFontSize,
                Margin = new Thickness(10),
                Height = 100
            };

            return (memView: memView, container: StackWithTitle("Memory", memView)); ;
        }

        public void AddMem(byte oldVal, byte newVal, ushort address)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disDisplay.Items.Count > 500)
                {
                    _disDisplay.Items.RemoveAt(0);
                }

                _memDisplay.Items.Add($"0x{address:X4} <- 0x{newVal:X2} (0x{oldVal:X2})");

                _memDisplay.ScrollIntoView(_memDisplay.Items.GetItemAt(_memDisplay.Items.Count - 1));
            }));
        }

        private void SetUpDisplay()
        {
            (var cpuControl, var cpuStack) = CreateCpuDisplay();
            var messageDisplay = CreateMessageDisplay();
            (var disControl, var disStack) = CreateDisDisplay();
            (var memControl, var memStack) = CreateMemDisplay();
            _disDisplay = disControl;
            _memDisplay = memControl;

            var mainGrid = new Grid { Background = _bgBrush };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 300 });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });

            mainGrid.Children.Add(disStack);
            Grid.SetRow(disStack, 0);
            Grid.SetColumn(disStack, 0);

            var pmStack = new StackPanel();
            pmStack.Children.Add(cpuStack);
            pmStack.Children.Add(memStack);

            mainGrid.Children.Add(pmStack);
            Grid.SetRow(pmStack, 0);
            Grid.SetColumn(pmStack, 1);

            mainGrid.Children.Add(messageDisplay);
            Grid.SetRow(messageDisplay, 1);
            Grid.SetColumn(messageDisplay, 0);
            Grid.SetColumnSpan(messageDisplay, 2);

            _window = new Window
            {
                Title = "BBC Microcomputer Emulator Debugger",
                Content = mainGrid,
                Background = Brushes.Black
            };
        }

        public Debugger()
        {
            SetUpDisplay();
            _window.Show();
        }

        private void ParagraphLoaded(object sender, RoutedEventArgs e)
        {
            Paragraph paragraph = (Paragraph)sender;
            paragraph.FontSize = _mainFontSize;
            paragraph.BringIntoView();
            paragraph.Loaded -= ParagraphLoaded;
        }

        public void AddMessage(string message, bool emphasis = false, bool error = false)
        {
            _window.Dispatcher.BeginInvoke(new Action(() =>
            {
                Paragraph p = new Paragraph();
                p.Margin = new Thickness(0);

                var run = new Run(message);

                if (emphasis)
                {
                    run.FontWeight = FontWeights.Bold;
                }

                if (error)
                {
                    run.Foreground = _errorBrush;
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
                result.Foreground = _changedBrush;
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
                SetRegister($"Y 0x{cpuState.PC:X2}", cpuState.Y != _prevState.Y, _yDisplay);
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
            }));
        }
    }
}