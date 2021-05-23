using BbcMicro.Cpu;
using NLog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BbcMicro.BbcMicro.VIA
{
    public sealed class VIA
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // The least significant nibble is for writing, and the most significant four bits for reading.
        private enum RegisterBValues : byte
        {
            EnableSoundChip = 0x00,
            EnableReadSpeech = 0x01,
            EnableWriteSpeech = 0x02,
            DisableKeyboardAutoScanning = 0x03, // <---
            HardwareScrollingSetC0to0 = 0x04,
            HardwareScrollingSetC1to0 = 0x05,
            TurnOnCapsLockLED = 0x06,
            TurnOnShiftLockLED = 0x07,
            DisableSoundChip = 0x08,
            DisableReadSpeech = 0x09,
            DisableWriteSpeech = 0x0A,
            EnableKeyboardAutoScanning = 0x0B, // <---
            HardwareScrollingSetC0to1 = 0x0C,
            HardwareScrollingSetC1to1 = 0x0D,
            TurnOffCapsLockLed = 0x0E,
            TurnOffShiftLockLed = 0x0F
        }

        /*
         * DDRA
         * When reading the keyboard, DDRA is set to (%011111111). The key to read is written
         * into bits 0-6 of .systemVIARegisterANoHandshake, and the 'pressed' state of that
         * key is then read from bit 7.
         */

        private enum IFRFlags : byte
        {
            KeyPressedInterrupt = 0b0000_0001, // <---
            VerticalSyncOccurred = 0b0000_0010, // <---
            ShiftRegisterTimeout = 0b0000_0100,
            LightPenStrobeOffScreen = 0b0000_1000,
            AnalogueConversionCompleted = 0b0001_0000,
            Timer2HasTimedOut = 0b0010_0000,
            Timer1HasTimedOut = 0b0100_0000, // <---
            MasterInterruptFlag = 0b1000_0000 // <---
        }

        private enum IERFlags : byte
        {
            KeyPressedInterrupt = 0b0000_0001, // <---
            VerticalSyncOccurred = 0b0000_0010, // <---
            ShiftRegisterTimeout = 0b0000_0100,
            LightPenStrobeOffScreen = 0b0000_1000,
            AnalogueConversionCompleted = 0b0001_0000,
            Timer2HasTimedOut = 0b0010_0000,
            Timer1HasTimedOut = 0b0100_0000, // <---
            EnableInterruptFlag = 0b1000_0000 // <---
        }

        private volatile bool _keyboardAutoscanning = true;
        private volatile Key _latestKey = Key.None;

        private volatile bool _keyboardInterruptsEnabled = true;
        private volatile bool _timer1InterruptsEnabled = true;
        private volatile bool _verticalSyncInterruptsEnabled = true;

        private volatile bool _keyboardInterruptActive = false;
        private volatile bool _timer1InterruptActive = false;
        private volatile bool _verticalSyncInterruptActive = false;
        private volatile byte _registerAout = 0x00;

        private readonly CPU _cpu;

        public byte ReadIER()
        {
            byte result = 0x00;

            if (_keyboardInterruptsEnabled)
            {
                result |= (byte)IERFlags.KeyPressedInterrupt;
            }
            if (_timer1InterruptsEnabled)
            {
                result |= (byte)IERFlags.Timer1HasTimedOut;
            }
            if (_verticalSyncInterruptsEnabled)
            {
                result |= (byte)IERFlags.VerticalSyncOccurred;
            }

            return result;
        }

        public byte ReadIFR()
        {
            byte result = 0x00;

            if (_keyboardInterruptActive)
            {
                result |= (byte)IFRFlags.KeyPressedInterrupt;
            }

            if (_timer1InterruptActive)
            {
                result |= (byte)IFRFlags.Timer1HasTimedOut;
            }

            if (_verticalSyncInterruptActive)
            {
                result |= (byte)IFRFlags.VerticalSyncOccurred;
            }

            if (result != 0x0)
            {
                result |= (byte)IFRFlags.MasterInterruptFlag;
            }

            _logger.Debug($"IFR={result:X2}");

            return result;
        }

        public VIA(CPU cpu)
        {
            _cpu = cpu;
            _cpu.Memory.AddSetByteCallback(WriteCallback);
            _cpu.Memory.AddGetByteCallback(ReadCallback);

            /// cpu.Memory.SetByte(0x00, (ushort)SystemConstants.ACIA.acia6850StatusRegister, true);
        }

        private byte? ReadCallback(ushort address)
        {
            byte? result = null;

            if (address == (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister)
            {
                result = ReadIFR();
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister)
            {
                result = ReadIER();
            }
            //else
            //if (address == (ushort)SystemConstants.VIA.systemVIARegisterB)
            //{
            //    result = ReadB();
            //}
            else
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake)
            {
                result = ReadA();
            }
            return result;
        }

        private byte ReadA()
        {
            return _registerAout;
        }

        private byte ReadB()
        {
            byte result = 0x0;
            //if (_keyboardAutoscanning)
            //{
            //    result |= (byte)RegisterBValues.EnableKeyboardAutoScanning;
            //}
            //else
            //{
            //    result |= (byte)RegisterBValues.DisableKeyboardAutoScanning;
            //}
            return result;
        }

        public void KeyPressCallback(object sender, KeyEventArgs e)
        {
            _logger.Debug("Keypressed callback");

            if (_keyboardAutoscanning)
            {
                _logger.Debug("Autoscanning");

                _latestKey = e.Key;

                if (_keyboardInterruptsEnabled)
                {
                    _logger.Debug("Raising keyboard interrupt");

                    _keyboardInterruptActive = true;
                    _cpu.Memory.SetByte(0b0000_0000, (ushort)SystemConstants.ACIA.acia6850StatusRegister, true);

                    _cpu.TriggerIRQ();
                }
            }
        }

        public void StartTimers()
        {
            Task.Run(() =>
            {
                Thread.Sleep(10000);
                while (true)
                {
                    Thread.Sleep(100);
                    if (_timer1InterruptsEnabled)
                    {
                        _timer1InterruptActive = true;
                        TriggerTimer1Interrupt();
                    }
                }
            });

            Task.Run(() =>
            {
                Thread.Sleep(10000);
                while (true)
                {
                    Thread.Sleep(200);
                    if (_verticalSyncInterruptsEnabled)
                    {
                        //_verticalSyncInterruptActive = true;
                        //TriggerVsyncInterrupt();
                    }
                }
            });
        }

        public void TriggerTimer1Interrupt()

        {
            _timer1InterruptActive = true;
            _cpu.TriggerIRQ();
        }

        public void TriggerVsyncInterrupt()

        {
            _verticalSyncInterruptActive = true;
            _cpu.TriggerIRQ();
        }

        private static readonly Dictionary<Key, byte> _keyMap = new Dictionary<Key, byte> {
            /* Row 0 */
            { Key.LeftShift, 0x00 }, { Key.RightShift, 0x00 },
            /* Row 1 */
            { Key.Q,  0x10 }, {Key.D3, 0x11},
            { Key.D4, 0x12 }, {Key.D5, 0x13},
            { Key.F5, 0x14 }, {Key.D8, 0x15 },
            { Key.F8, 0x16 }, {Key.OemMinus, 0x17},
            { Key.OemPlus, 0x18 }, { Key.Left, 0x19 },
            /* Row 2 */
            { Key.F1, 0x20 }, {Key.W, 0x21 },
            { Key.E, 0x22 }, { Key.T, 0x23 },
            { Key.D7,0x24 }, {Key.I,0x25 },
            { Key.D9, 0x26 }, {Key.D0, 0x27 },
            { Key.OemTilde, 0x28}, {Key.Down, 0x29 },
            /* Row 3 */
            { Key.D1, 0x30 }, { Key.D2, 0x31 },
            { Key.D, 0x32 }, { Key.R, 0x33 },
            { Key.D6, 0x34 }, { Key.U, 0x35 },
            { Key.O, 0x36 }, { Key.P, 0x37 },
            { Key.OemOpenBrackets, 0x38 }, { Key.Up, 0x39 },
            /* Row 4 */
            { Key.CapsLock, 0x40 }, { Key.A, 0x41 },
            { Key.X, 0x42 }, { Key.F, 0x43 },
            { Key.Y, 0x44 }, { Key.J, 0x45 },
            { Key.K, 0x46 }, { Key.OemQuotes, 0x47 },
            { Key.OemPipe, 0x48 }, { Key.Return, 0x49 },
            /* Row 5 */
            { Key.RightAlt, 0x50 }, { Key.S, 0x51 },
            { Key.C, 0x52 }, { Key.G, 0x53 },
            { Key.H, 0x54 }, { Key.N, 0x55 },
            { Key.L, 0x56 }, { Key.OemSemicolon, 0x57 },
            { Key.OemCloseBrackets, 0x58 }, { Key.Delete, 0x59 },
            /* Row 6 */
            { Key.Tab, 0x60 }, { Key.Z, 0x61 },
            { Key.Space, 0x62 }, { Key.V, 0x63 },
            { Key.B, 0x64 }, { Key.M, 0x65 },
            { Key.OemComma, 0x66 }, { Key.OemPeriod, 0x67 },
            { Key.OemQuestion, 0x68 }, { Key.OemCopy, 0x69 },
            /* Row 7 */
            { Key.Escape, 0x70 }, { Key.F2, 0x71 },
            { Key.F3, 0x72 }, { Key.F4, 0x73 },
            { Key.F6, 0x74 }, { Key.F7, 0x75 },
            { Key.F9, 0x76 }, { Key.F10, 0x77 },
            { Key.OemBackslash, 0x78 }, { Key.Right, 0x79 }
        };

        private void KeyPress()
        {
            _logger.Debug("Entering KeyPress--->");

            var dataDirection =
                _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIADataDirectionRegisterA, true);
            _logger.Debug($"systemVIADataDirectionRegisterA={dataDirection:X2}");

            var targetKeyByte =
                         _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIARegisterANoHandshake, true);

            _logger.Debug($"_latestKey={_latestKey}");
            _logger.Debug($"targetKeyByte = {targetKeyByte:X2}");

            if (dataDirection == 0x7F)
            {
                // Keyboard DIP switches
                if (targetKeyByte >= 0x02 && targetKeyByte <= 0x09)
                {
                    _logger.Debug("DIP Switch");

                    _registerAout = (byte)(0x7F & targetKeyByte);
                    if (targetKeyByte >= 0x07 && targetKeyByte <= 0x09)
                    {
                        _registerAout = (byte)(0x80 | targetKeyByte);
                    }
                }
                else
                if (_keyMap.TryGetValue(_latestKey, out var latestKeyByte))
                {
                    _logger.Debug("Key found");
                    _logger.Debug($"latestKeyByte = {latestKeyByte:X2}");

                    // Specific key match
                    _registerAout = (byte)(0x7F & targetKeyByte);
                    if (targetKeyByte == latestKeyByte)
                    {
                        _logger.Debug("Exact match");
                        _registerAout = (byte)(0x80 | targetKeyByte);
                    }

                    _logger.Debug($"Setting systemVIARegisterANoHandshake to {_registerAout:X2}");

                    // Column match
                    var targetColumn = targetKeyByte & 0x0F;
                    var latestColumn = latestKeyByte & 0x0F;

                    _logger.Debug($"targetColumn = {targetColumn:X2}");
                    _logger.Debug($"latestColumn = {latestColumn:X2}");

                    _keyboardInterruptActive = false; /// NO
                    if (targetColumn == latestColumn)
                    {
                        _logger.Debug($"Column match");
                        _keyboardInterruptActive = true;
                    }
                }
            }

            _logger.Debug("<--- Leaving KeyPress");
        }

        private void ClearInterrupt()
        {
            _logger.Debug("Entering ClearInterrupt --->");

            var ifrValue = _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister, true);
            _logger.Debug($"systemVIAInterruptFlagRegister = {ifrValue:X2}");

            var clearing = (ifrValue & (byte)IFRFlags.MasterInterruptFlag) == 0;
            if (clearing)
            {
                _logger.Debug("Clearing interrupt");
                if ((ifrValue & (byte)IFRFlags.KeyPressedInterrupt) != 0)
                {
                    _logger.Debug("Clearing keyboard interrupt");
                    _keyboardInterruptActive = false;
                }
                else if ((ifrValue & (byte)IFRFlags.Timer1HasTimedOut) != 0)
                {
                    _logger.Debug("Clearing timer 1 interrupt");
                    _timer1InterruptActive = false;
                }
                else
                if ((ifrValue & (byte)IFRFlags.VerticalSyncOccurred) != 0)
                {
                    _logger.Debug("Clearing vertical sync interrupt");
                    _verticalSyncInterruptActive = false;
                }
            }

            _logger.Debug("<--- Leaving ClearInterrupt");
        }

        private void EnableOrDisableInterrupts()
        {
            _logger.Debug("Entering EnableOrDisableInterrupts --->");
            var ierValue = _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemViaInterruptEnableRegister, true);

            var enabling = (ierValue & 0x80) != 0;
            _logger.Debug($"Enabling = {enabling}");

            if ((ierValue & (byte)IERFlags.KeyPressedInterrupt) != 0)
            {
                _logger.Debug("Keyboard interrupt");
                _keyboardInterruptsEnabled = enabling;
            }
            if ((ierValue & (byte)IERFlags.Timer1HasTimedOut) != 0)
            {
                _logger.Debug("Timer 1 interrupt");
                _timer1InterruptsEnabled = enabling;
            }
            if ((ierValue & (byte)IERFlags.VerticalSyncOccurred) != 0)
            {
                _logger.Debug("Vertical Sync interrupt");
                _verticalSyncInterruptsEnabled = enabling;
            }

            _logger.Debug("<--- Leaving EnableOrDisableInterrupts --->");
        }

        private void WriteCallback(byte oldVal, byte newVal, ushort address)
        {
            if (address == (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister)
            {
                ClearInterrupt();
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister)
            {
                EnableOrDisableInterrupts();
            }
            else
            // Turn keyboard scanning off and on
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterB)
            {
                var viaRegisterBValue = _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIARegisterB, true);
                switch (viaRegisterBValue)
                {
                    // This might be wrong - I should just be looking at the relevant bit
                    case (byte)RegisterBValues.DisableKeyboardAutoScanning:
                        _keyboardAutoscanning = false;
                        break;

                    case (byte)RegisterBValues.EnableKeyboardAutoScanning:
                        _keyboardAutoscanning = true;
                        break;

                    default:
                        // We don't care about the other options for now
                        break;
                }
            }
            else
            // Read the keyboard
            // TODO: Special handling for DIP switches
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake)
            {
                KeyPress();
            }
        }
    }
}