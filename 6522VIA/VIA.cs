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

        private readonly Dictionary<Key, (int row, int col)> SystemKeyToBBCKey = new Dictionary<Key, (int row, int col)>
        {
            { Key.LeftShift, (0, 0x00) }
        };

        private volatile bool _keyboardAutoscanning = true;
        private volatile Key _latestKey = Key.None;
        private volatile bool _keyboardInterruptsEnabled = true;

        private readonly CPU _cpu;

        public VIA(CPU cpu)
        {
            _cpu = cpu;
            _cpu.Memory.AddSetByteCallback(WriteCallback);
            // _cpu.Memory.SetByte(0, 0x027B);
        }

        private const ushort acia6850StatusRegister = 0XFE08;

        public void KeyPressCallback(object sender, KeyEventArgs e)
        {
            //if (_keyboardAutoscanning)
            //{
            // _cpu.Memory.SetByte(0b0000_0000, acia6850StatusRegister, false);

            _latestKey = e.Key;
            //if (_keyboardInterruptsEnabled)
            //{
            _cpu.Memory.SetByte((byte)(//_cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister) |
                (byte)IFRFlags.KeyPressedInterrupt |
                (byte)IFRFlags.MasterInterruptFlag),
                (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister,
                true);

            _cpu.Memory.SetByte(0b0000_0000, acia6850StatusRegister);
            _cpu.Memory.SetByte(0xFF, (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister);

            _cpu.TriggerIRQ();
            // } }
        }

        private bool flipper = false;

        public void StartTimers()
        {
            Task.Run(() =>
            {
                Thread.Sleep(10000);
                while (true)
                {
                    Thread.Sleep(100);
                    TriggerInterrupt();
                }
            });
        }

        public void TriggerInterrupt()

        {
            // Re-enable all interrupts (frig)
            //_cpu.Memory.SetByte(0xFF, (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister, false);

            _cpu.Memory.SetByte(0b0000_0000, acia6850StatusRegister, true);

            if (flipper)
            {
                // 100Hz timer interrupt 0b1100_0000
                _cpu.Memory.SetByte(0b1100_0000, (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister, true);
            }
            else
            {
                _cpu.Memory.SetByte(0b0000_0010, (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister, true);
            }
            flipper = !flipper;
            _cpu.TriggerIRQ();
        }

        // Should have addr space in the CB
        private void WriteCallback(byte oldVal, byte newVale, ushort address)
        {
            // _logger.Debug($"Write callback");
            //// Enable/disable interrupts
            //// EnableInterruptFlag - set to enable/reset to disable

            if (address == (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister)
            {
                var ierValue = _cpu.Memory.GetByte(address);
                var enabled = (ierValue & (byte)IERFlags.EnableInterruptFlag) != 0;

                if ((ierValue & (byte)IERFlags.KeyPressedInterrupt) == 0)
                {
                    _keyboardInterruptsEnabled = enabled;
                }
                _keyboardInterruptsEnabled = true;
            }
            else
            // Turn keyboard scanning off and on
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterB)
            {
                var viaRegisterBValue = _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIARegisterB);
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
            // TODO: Special handling for DIP switches NB we need to add a set will get recursion here!
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake)
            {
                /*
                 * A is key / column to test , on exit A has top bit set if pressed
                 */

                //if (!_keyboardAutoscanning)
                //{
                //var imp = _cpu.Memory.GetByte(0xFE40);
                //_logger.Debug($"{imp:X2}");
                //if (imp == 0b01111111)
                //{
                var aRegValue = _cpu.Memory.GetByte((ushort)SystemConstants.VIA.systemVIARegisterANoHandshake);

                _logger.Debug("Entering key test --->");
                _logger.Debug($"systemVIARegisterANoHandshake={aRegValue:X2}");

                // Specific key match
                if (_latestKey == Key.A && aRegValue == 0x41) // TODO - probably need to buffer up key events.
                {
                    _logger.Debug($"Latest key is an A so setting systemVIARegisterANoHandshake to {(byte)(0x80 | aRegValue):X2}");

                    _cpu.Memory.SetByte((byte)(0x80 | aRegValue),
                    (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake,
                    true);
                }
                else
                {
                    _logger.Debug($"Latest key is not an A so setting systemVIARegisterANoHandshake to {(byte)((byte)(0x7F & aRegValue)):X2}");
                    _cpu.Memory.SetByte((byte)(0x7F & aRegValue),
                        (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake,
                        true);
                }

                // Column match - seems from the code to want the LSB or the interrupt register to
                // be set! Column in Least significant bits
                var column = aRegValue & 0x0F;
                _logger.Debug("Testing for column number={0}", column);
                if (column == 1)
                {
                    _logger.Debug("Column number==1, so found and setting (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister to 0x01");
                    _cpu.Memory.SetByte(0x01, (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister, true);
                }
                else
                {
                    _logger.Debug("Column number!=1, so NOT found and setting (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister to 0x00");

                    _cpu.Memory.SetByte(0x00, (ushort)SystemConstants.VIA.systemVIAInterruptFlagRegister, true);
                }
            }
            //}
            // }
        }
    }
}