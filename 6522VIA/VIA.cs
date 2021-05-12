using BbcMicro.Memory.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace BbcMicro.BbcMicro.VIA
{
    public sealed class VIA
    {
        // The least significant nibble is for writing,
        // and the most significant four bits for reading.
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
            MasterInterruptFlag = 0b1000_000 // <---
        }

        private readonly Dictionary<Key, (int row, int col)> SystemKeyToBBCKey = new Dictionary<Key, (int row, int col)>
        {
            { Key.LeftShift, (0, 0x00) }
        };

        private volatile bool _keyboardAutoscanning = true;

        private void KeyPressCallback()
        {
            // Translate windows key to BBC keycode and store
            // Maybe just if scanning
        }

        private void WriteCallback(ushort address, IAddressSpace memory)
        {
            // Enable/disable interrupts
            // Bit set = disable
            if (address == (ushort)SystemConstants.VIA.systemViaInterruptEnableRegister)
            {
            }
            else
            // Turn keyboard scanning off and on
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterB)
            {
                var viaRegisterBValue = memory.GetByte((ushort)SystemConstants.VIA.systemVIARegisterB);
                switch (viaRegisterBValue)
                {
                    // This might be wrong - I should just be looking at the relevant bite
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
            // NB we need to add a set will get recursion here!
            if (address == (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake)
            {
                // Looks like and exact match sets the high bit of a
                // and a column match the low bit of the interupt register
                /*
                 * A is key to test , on exit A has top bit set if pressed
                 */

                if (Keyboard.IsKeyDown(Key.A)) // TODO - probably need to buffer up key events.
                {
                    memory.SetByte(0x80 | 00 /* internal key number */, (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake);
                }
                else
                {
                    memory.SetByte(0x00, (ushort)SystemConstants.VIA.systemVIARegisterANoHandshake);
                }
            }
        }
    }