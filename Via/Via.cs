using BbcMicro.Cpu;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BbcMicro.BbcMicro.VIA
{
    public sealed class VIA
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

        private volatile bool _keyboardInterruptsEnabled = false;
        private volatile bool _timer1InterruptsEnabled = false;
        private volatile bool _verticalSyncInterruptsEnabled = false;

        private volatile bool _keyboardInterruptActive = false;
        private volatile bool _timer1InterruptActive = false;
        private volatile bool _verticalSyncInterruptActive = false;

        private readonly CPU _cpu;

        public VIA(CPU cpu)
        {
            _cpu = cpu;
            _cpu.Memory.AddSetByteCallback(WriteCallback);
            _cpu.Memory.AddGetByteCallback(ReadCallback);
        }

        /*
         * System VIA, Register B ($FE40).
         *
         * https://tobylobster.github.io/mos/mos/S-s3.html#SP10
         *
         *  The bottom four bits are used for writing, and the top four bits are used for reading.
         *    (See .systemVIADataDirectionRegisterB)
         *
         *    Values 0-15 can be written to System VIA Register B (Output):
         *
         *        Value   Effect
         *        -------------------------
         *        0       Enable sound chip
         *        1       Enable Read Speech
         *        2       Enable Write Speech
         *        3       Disable Keyboard auto scanning
         *        4       Hardware scrolling - set C0=0 (See below)
         *        5       Hardware scrolling - set C1=0 (See below)
         *        6       Turn on CAPS LOCK LED
         *        7       Turn on SHIFT LOCK LED
         *        8       Disable sound chip
         *        9       Disable Read Speech
         *        10      Disable Write Speech
         *        11      Enable Keyboard auto scanning
         *        12      Hardware scrolling - set C0=1 (See below)
         *        13      Hardware scrolling - set C1=1 (See below)
         *        14      Turn off CAPS LOCK LED
         *        15      Turn off SHIFT LOCK LED
         *
         * The values of C0 and C1 together determine the start scroll address for the screen:
         *
         *        C0   C1      Screen       Used in
         *                     Address   Regular MODEs
         *        ------------------------------------
         *         0    0      $4000           3
         *         0    1      $5800          4,5
         *         1    0      $6000           6
         *         1    1      $3000         0,1,2
         *
         * When reading from this address the top four bits are read:
         *
         * bit 7:    Speech processor 'ready' signal
         * bit 6:    Speech processor 'interrupt' signal
         * bit 4-5:  joystick buttons (bit is zero when button pressed)
         */

        private void WriteB(byte viaRegisterBValue)
        {
            if ((viaRegisterBValue & (byte)RegisterBValues.DisableKeyboardAutoScanning) != 0)
            {
                _keyboardAutoscanning = false;
            }
            if ((viaRegisterBValue & (byte)RegisterBValues.EnableKeyboardAutoScanning) != 0)
            {
                _keyboardAutoscanning = true;
            }
        }

        private byte ReadB()
        {
            return 0b0011_0000;
        }

        /*
         * System VIA, Data Direction Register B ($FE42) (aka 'DDRB').
         *
         * https://tobylobster.github.io/mos/mos/S-s3.html#SP12
         *
         * When writing data into Register B (.systemVIARegisterB), the bits that are set on DDRB
         * indicate which bits are actually written into Register B. The bits that are clear on DDRB
         * are used to read from Register B.
        *
         * DDRB is only written once on startup where it is initialised to %00001111
         * (see .setUpSystemVIA) and the OS expects it to remain that way. Only the bottom four bits
         * of .systemVIARegisterB are used when writing, and only the upper four bits are read from
         * .systemVIARegisterB. See .systemVIARegisterB.
         */

        private byte ReadDDRB()
        {
            return 0b0000_1111;
        }

        /*
         * System VIA, Data Direction Register A ($FE43) (aka 'DDRA').
         *
         * https://tobylobster.github.io/mos/mos/S-s3.html#SP13
         *
         * The keyboard, sound and speech systems use Data Direction Register A. Each bit of DDRA
         * indicates whether data can be written or read on that bit when data is accessed via
         * .systemVIARegisterANoHandshake. This is similar to DDRB. Unlike DDRB, the OS modifies
         * DDRA frequently to set the appropriate bits for accessing the device (often in the IRQ
         * interrupt code). Once set, data is read or written to .systemVIARegisterANoHandshake as
         * needed. See .systemVIARegisterANoHandshake.
         *
         * Sound:    When outputting sound, DDRA is set to %11111111 meaning all bits of data
         *           that are subsequently written to .systemVIARegisterANoHandshake are output bits.
         *           (See .sendToSoundChipFlagsAreadyPushed)
         *
         * Speech:   For speech, DDRA is set to %00000000 (for reading) or %11111111 (for writing) as
         * needed. (See .readWriteSpeechProcessorPushedFlags)
         *
         * Keyboard: When reading the keyboard, DDRA is set to (%011111111). The key to read is written
         *           into bits 0-6 of .systemVIARegisterANoHandshake, and the 'pressed' state of that
         *           key is then read from bit 7.
         *           (See .interrogateKeyboard)
         *           (See .scanKeyboard)
         */

        private byte _DDRA = 0x0;

        private void WriteDDRA(byte ddraValue)
        {
            if (ddraValue != 0b0111_11111)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Warn($"DDRA set to a value other than for reading the keyboard {ddraValue:X2}");
                }
            }
            _DDRA = ddraValue;
        }

        private byte ReadDDRA()
        {
            return _DDRA;
        }

        /*
         * System VIA, Interrupt Enable Register ($FE4E) (aka 'IER').
         *
         * https://tobylobster.github.io/mos/mos/S-s3.html#SP20
         *
         * Each bit controls whether an interrupt is enabled or disabled.
         *
         * bit 0 = key pressed interrupt
         * bit 1 = vertical sync occurred
         * bit 2 = shift register timeout (unused)
         * bit 3 = light pen strobe off screen
         * bit 4 = analogue conversion completed
         * bit 5 = timer 2 timed out (used for speech)
         * bit 6 = timer 1 timed out (100Hz signal)
         * bit 7 = enable/disable interrupt value (see below)
         *
         * Writing:
         * --------
         * To enable  an interrupt, write a byte with the top bit set   and set the desired bit(s) (0-6).
         * To disable an interrupt, write a byte with the top bit clear and set the desired bit(s) (0-6).
         *
         * Reading:
         * --------
         * Bits 0-6 are read as expected.
         * Bit 7 is always set when read.
         *
         */

        private void WriteIER(byte ierValue)
        {
            var enabling = (ierValue & 0x80) != 0;

            if ((ierValue & (byte)IERFlags.KeyPressedInterrupt) != 0)
            {
                _logger.Debug($"Keyboard interrupt enabled={enabling}");
                _keyboardInterruptsEnabled = enabling;
            }
            if ((ierValue & (byte)IERFlags.Timer1HasTimedOut) != 0)
            {
                _timer1InterruptsEnabled = enabling;
            }
            if ((ierValue & (byte)IERFlags.VerticalSyncOccurred) != 0)
            {
                _verticalSyncInterruptsEnabled = enabling;
            }
        }

        private byte ReadIER()
        {
            byte result = 0x80;

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

        /*
         * https://tobylobster.github.io/mos/mos/S-s3.html#SP20
         *
         * System VIA, Interrupt Flag Register ($FE4D) (aka 'IFR').
         *
         *   bit 0 = key pressed interrupt
         *   bit 1 = vertical sync occurred
         *   bit 2 = shift register timeout (unused)
         *   bit 3 = lightpen strobe off screen
         *   bit 4 = analogue conversion completed
         *   bit 5 = timer 2 has timed out (used for speech)
         *   bit 6 = timer 1 has timed out (100Hz signal)
         *   bit 7 = (when reading) master interrupt flag (0-6 invalid if clear)
         *
         * Used in interrupt code:
         *
         * Reading
         * -------
         * If bit 7 is set then the System VIA caused the current interrupt. The remaining bits can
         * then be checked to see the exact cause.
         *
         * Writing
         * -------
         * Clear bit 7 and set a bit 0-6 to clear that interrupt.
         *
         */

        private void WriteIFR(byte ifrValue)
        {
            var clearing = (ifrValue & (byte)IFRFlags.MasterInterruptFlag) == 0;
            if (clearing)
            {
                if ((ifrValue & (byte)IFRFlags.KeyPressedInterrupt) != 0)
                {
                    _logger.Debug("Clearing keyboard interrupt");
                    _cpu.DeNotifyIRQ(IRQ_TYPE_KEYBOARD);
                    _keyboardInterruptActive = false;
                }
                else if ((ifrValue & (byte)IFRFlags.Timer1HasTimedOut) != 0)
                {
                    _cpu.DeNotifyIRQ(IRQ_TYPE_TIMER_1);
                    _timer1InterruptActive = false;
                }
                else
                if ((ifrValue & (byte)IFRFlags.VerticalSyncOccurred) != 0)
                {
                    _cpu.DeNotifyIRQ(IRQ_TYPE_VERITCAL_SYNC);
                    _verticalSyncInterruptActive = false;
                }
            }
        }

        private byte ReadIFR()
        {
            byte result = 0x00;

            if (_keyboardInterruptsEnabled || _keyboardAutoscanning || !_keyboardInterruptActive)
            {
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
            }
            else
            {
                // Column scanning
                var targetColumn = _AIncoming & 0x0F;

                _logger.Debug($"targetColumn = {targetColumn:X2}, PC={_cpu.PC:X4}");

                if (_keyMap.TryGetValue(_lastKey, out var downKeyByte))
                {
                    var _latestColumn = (downKeyByte & 0x0F);
                    _logger.Debug($"latestColumn = {_latestColumn:X2}");

                    if (_latestColumn == targetColumn)
                    {
                        _logger.Debug($"Column match");
                        result = (byte)IFRFlags.KeyPressedInterrupt;
                    }
                }
            }
            return result;
        }

        private byte _AIncoming;

        private void WriteCallback(byte newVal, byte oldVal, ushort address)
        {
            if (address == (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister)
            {
                WriteIFR(newVal);
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister)
            {
                WriteIER(newVal);
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterB)
            {
                WriteB(newVal);
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake)
            {
                _AIncoming = newVal;
                _logger.Debug($"Setting _AIncomging to {newVal}");
            }
            if (address == (ushort)SystemConstants.VIA.systemVIADataDirectionRegisterA)
            {
                WriteDDRA(newVal);
            }
        }

        private byte? ReadCallback(ushort address)
        {
            byte? result = null;

            if (address == (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister)
            {
                result = ReadIFR();
                // _logger.Debug($"Returning IFR as {result:X2}");
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister)
            {
                result = ReadIER();
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterB)
            {
                result = ReadB();
            }
            else
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake)
            {
                result = ReadA();
            }

            return result;
        }

        private byte ReadA()
        {
            return KeyPress();
        }

        private Key _lastKey = Key.None;

        private const string IRQ_TYPE_KEYBOARD = "KEYBOARD";
        private const string IRQ_TYPE_TIMER_1 = "TIMER_1";
        private const string IRQ_TYPE_VERITCAL_SYNC = "VERTICAL_SYNC";

        public void KeyPressCallback(object sender, KeyEventArgs e)
        {
            _logger.Debug("Key pressed callback");
            _logger.Debug($"_keyboardInterruptsEnabled={_keyboardInterruptsEnabled}");
            _logger.Debug($"_keyboardAutoscanning={_keyboardAutoscanning}");
            _logger.Debug($"_keyboardInterruptActive={_keyboardInterruptActive}");

            if (_keyboardAutoscanning && !_keyboardInterruptActive)
            {
                _lastKey = e.Key;
                _logger.Debug($"Setting last key to '{e.Key}'");

                if (_keyboardInterruptsEnabled)
                {
                    _logger.Debug("Raising keyboard interrupt");
                    _keyboardInterruptActive = true;
                    _cpu.NofityIRQ(IRQ_TYPE_KEYBOARD);
                }
            }
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

        // 0-9 are actually used for column scan
        // Maye specific keys are -ive (top bit set)
        private byte KeyPress()
        {
            _logger.Debug("Entering KeyPress--->");
            _logger.Debug($"PC={_cpu.PC:X4}");

            byte result = 0x0;

            _logger.Debug($"_lastKey={_lastKey}");

            var targetKeyByte = _AIncoming;
            _logger.Debug($"targetKeyByte = {targetKeyByte:X2}");

            var dataDirection = ReadDDRA();
            _logger.Debug($"systemVIADataDirectionRegisterA={dataDirection:X2}");

            if (dataDirection == 0b0111_1111)
            {
                if (targetKeyByte >= 0x02 && targetKeyByte <= 0x09)
                {
                    _logger.Debug("DIP Switch");

                    result = (byte)(0x7F & targetKeyByte);
                    if (targetKeyByte >= 0x07 && targetKeyByte <= 0x09)
                    {
                        result = (byte)(0x80 | targetKeyByte);
                    }
                }
                else
                {
                    // Specific key match
                    result = (byte)(0x7F & targetKeyByte);

                    if (_keyMap.TryGetValue(_lastKey, out var lastKeyByte))
                    {
                        if (lastKeyByte == targetKeyByte)
                        {
                            _logger.Debug("Exact match for pressed key");
                            result = (byte)(0x80 | targetKeyByte);
                        }
                    }
                }
            }

            _logger.Debug("<--- Leaving KeyPress");

            return result;
        }

        public void StartTimers()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (_timer1InterruptsEnabled)
                    {
                        _timer1InterruptActive = true;
                        _cpu.NofityIRQ(IRQ_TYPE_TIMER_1);
                    }
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(20);
                    if (_verticalSyncInterruptsEnabled)
                    {
                        _verticalSyncInterruptActive = true;
                        _cpu.NofityIRQ(IRQ_TYPE_VERITCAL_SYNC);
                    }
                }
            });
        }
    }
}