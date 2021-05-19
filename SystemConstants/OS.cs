namespace BbcMicro.SystemConstants
{
    internal enum OS : ushort
    {
        mosVariables = 0x0236,     // Constant value.mosVariablesMinus166

        // (Read using OSBYTE 166/167)
        romPointerTable = 0x0238,     // Constant value.extendedVectorSpace

        // (Read using OSBYTE 168/169)
        romInformationTable = 0x023A,     // Constant value.romTypeTable

        // (Read using OSBYTE 170/171)
        keyTranslationTable = 0x023C,     // Constant value.keyDataTable1 - 16

        // (Read using OSBYTE 172/173)
        vduVariablesTable = 0x023E,     // Constant value.vduVariablesStart

        // (Read using OSBYTE 174/175)

        verticalSyncCounter = 0x0240,     // decremented every vertical sync

        // (Read/Write using OSBYTE 176)
        currentInputBuffer = 0x0241,     // buffer number for current input

        // (Read/Write using OSBYTE 177)
        enableKeyboardInterruptProcessingFlag = 0x0242,     // aka 'Keyboard semaphore'

        // 0x00 = don't update key presses
        //       on 100Hz interrupts
        // 0xFF = key presses are processed
        //       as normal
        // (Read/Write using OSBYTE 178)
        defaultOSHWM = 0x0243,     // default/primary High Water Mark.

        // this is after Paged ROMs have
        // asked for their workspace memory,
        // but before soft character
        // definitions have been 'exploded'.
        // Initialised at RESET time, and
        // otherwise remains constant.
        // (Read/Write using OSBYTE 179)
        currentOSHWM = 0x0244,     // current OS High Water Mark, which

        // varies due to font explosions.
        // (Read/Write using OSBYTE 180)
        rs423Mode = 0x0245,     // RS-423 mode(1 is default) :

        //   0 = treat RS-423 input the same
        //       as the keyboard
        //   1 = ESCAPE is ignored//
        //       Soft keys are not expanded//
        //       no events are triggered
        // (Read/Write using OSBYTE 181)
        softCharacterDefinitionsSwitch = 0x0246,     // which ranges of characters are Soft

        // character definitions(i.e.the
        // value X used in * FX 20,X)
        // (Read using OSBYTE 182 but note
        // that this is incompatible with
        // the Master series)
        tapeOrROMSwitch = 0x0247,     // 0 is *TAPE, 2 = *ROM

        // (Read/Write using OSBYTE 183)
        videoULAVideoControlRegisterCopy = 0x0248,     // OS copy of video control register

        // (Read using OSBYTE 184)
        videoULAPaletteValue = 0x0249,     // value last written to the palette:

        // bits 0-3: physical colour EOR 7
        // bits 4-7: logical colour
        // (Read using OSBYTE 185)
        romNumberActiveLastBRK = 0x024A,     // ROM socket number when last BRK occurred

        // (Read using OSBYTE 186)
        basicROMNumber = 0x024B,     // ROM socket number containing BASIC or 0xFF

        // (Read using OSBYTE 187)
        adcCurrentChannel = 0x024C,     // current ADC channel number

        // (Read using OSBYTE 188)
        maximumADCChannelNumber = 0x024D,     // read maximum ADC channel number

        // (Read using OSBYTE 189)
        adcConversionType = 0x024E,    // ADC conversion type:

        // 0 = default (12 bits)
        // 8 = 8 bits
        // 12 = 12 bits
        // (Read/Write using OSBYTE 190)
        rs423ReadyFlag = 0x024F,     // bit 7 set means RS-423 is ready

        // otherwise RS-423 is busy
        // (Read/Write using OSBYTE 191)
        rs423ControlRegisterCopy = 0x0250,     // OS copy of the RS-423 control flag

        // (Read/Write using OSBYTE 192)
        videoULAFlashingColourIntervalCount = 0x0251,     // counts down time before flashing the colours

        // (Read/Write using OSBYTE 193)
        videoULAFirstFlashingColourInterval = 0x0252,    // number of frames to spend on the first flashing colour

        // (Read/Write using OSBYTE 194, but prefer OSBYTE 9)
        videoULASecondFlashingColourInterval = 0x0253,     // number of frames to spend on the second flashing colour

        // (Read/Write using OSBYTE 195, but prefer OSBYTE 10)
        keyboardAutoRepeatDelay = 0x0254,     // delay before a key held down autorepeats in centi-seconds

        // (Read/write using OSBYTE 196, but prefer OSBYTE 11)
        keyboardAutoRepeatRate = 0x0255,     // keyboard autorepeat rate in centi-seconds

        // (Read/write using OSBYTE 197, but prefer OSBYTE 12)
        execFileHandle = 0x0256,     // file handle of open EXEC file or zero if none open

        // (Read/Write using OSBYTE 198)
        spoolFileHandle = 0x0257,     // file handle of open SPOOL file or zero if none open

        // (Read/Write using OSBYTE 199)
        escapeAndBreakEffect = 0x0258,    // bit 0 set disables ESCAPE

        // bit 1 set clears memory on BREAK
        // (Read/write using OSBYTE 200)
        keyboardDisableFlag = 0x0259,     // 0=Normal

        // otherwise ignore all keys except ESCAPE
        // (Read/Write using OSBYTE 201)
        keyboardStatusFlags = 0x025A,     // bit 3 = 1 means SHIFT pressed

        // bit 4 = 0 means CAPS LOCK engaged
        // bit 5 = 0 means SHIFT LOCK engaged
        // bit 6 = 1 means CTRL pressed
        // bit 7 = 1 means SHIFT enabled
        // (Read/Write using OSBYTE 202)
        rs423HandshakeExtent = 0x025B,     // how many bytes free when dealing with a full buffer

        // (Read/Write using OSBYTE 203)
        rs423InputSuppressionFlag = 0x025C,     // non-zero value inhibits RS-423 input

        // (Read/Write using OSBYTE 204)
        tapeRS423SelectionFlag = 0x025D,     // 0 = RS-423// 0x40 = TAPE(Comes into effect when using OSBYTE 7/8 to change)

        // (Read/Write using OSBYTE 205)
        econetOSCallInterceptionFlag = 0x025E,     // bit 7 set sends OS calls through the econet vector

        // otherwise should be set to zero
        // (see.osbyteOrOSWORDTableLookup)
        // (Read/Write using OSBYTE 206)
        econetReadCharacterInterceptionFlag = 0x025F,     // read character from ECONET if bit 7 set

        // (Read/Write using OSBYTE 207)
        econetWriteCharacterInterceptionFlag = 0x0260,     // bit 7 set means direct the character to write to ECONET

        // (Read/Write using OSBYTE 208)
        speechSuppressionStatus = 0x0261,     // 0x50 to enable speech// 0x20 to disable

        // (Read/Write using OSBYTE 209)
        soundDisableFlag = 0x0262,     // non-zero disables sound

        // (Read/Write using OSBYTE 210)
        soundBELLChannel = 0x0263,     // sound channel for CTRL-G BELL

        // (Read/Write using OSBYTE 211)
        soundBELLAmplitudeEnvelope = 0x0264,     // sound amplitude/envelope for CTRL-G BELL

        // (Read/Write using OSBYTE 212)
        soundBELLPitch = 0x0265,     // sound pitch for CTRL-G BELL

        // (Read/Write using OSBYTE 213)
        soundBELLDuration = 0x0266,     // sound duration for CTRL-G BELL

        // (Read/Write using OSBYTE 214)
        startupMessageSuppressionAndBootOptions = 0x0267,     // bit 7 = 0 means ignore OS startup message

        // bit 0 = 1 means if !BOOT errors from DISC because no language found, then lock up machine
        // bit 0 = 0 means if !BOOT errors from* ROM because no language found, then lock up machine
        // (Read/Write using OSBYTE 215)
        softKeyStringLength = 0x0268,     // length of the current * KEY string being decoded

        // (Read/Write using OSBYTE 216)
        pagedModeCounter = 0x0269,     // number of lines printed since last paged mode pause

        // (Read/Write using OSBYTE 217)
        twosComplimentOfNumberOfBytesInVDUQueue = 0x026A,     // 255 - bytes in vdu queue

        // (Read/Write using OSBYTE 218)
        asciiCodeGeneratedByTABKey = 0x026B,     // ASCII value to be produced by TAB

        // (Read/Write using OSBYTE 219)
        asciiCodeThatGeneratesESCAPEAction = 0x026C,     // key's ASCII code that will generate an ESCAPE action

        // (Read/Write using OSBYTE 220)

        functionAndCursorKeyCodes = 0x026D,     // 8 bytes that determine how to interpret special keys:

        // byte range of keys
        //  0      0xC0-0xCF
        //  1      0xD0-0xDF
        //  2      0xE0-0xEF
        //  3      0xF0-0xFF
        //  4      0x80-0x8F  Function keys
        //  5      0x90-0x9F  SHIFT+Function keys
        //  6      0xA0-0xAF CTRL+Function keys
        //  7      0xB0-0xBF CTRL+SHIFT+Function keys
        //
        // 0 = ignore key
        // 1 = expand as 'soft' key
        // 2-255 = add this to base for 'ASCII' code
        //
        // note that provision is made for keypad operation
        // as codes 0xC0-0xFF cannot be generated from keyboard
        // but are recognised by OS
        //
        // (Read/Write using OSBYTE 221-228)

        escapeAction = 0x0275,     // 0 = normal ESCAPE action

        // otherwise ASCII
        // (Read/Write using OSBYTE 229)
        escapeEffects = 0x0276,     // 0 = ESCAPE cleared, EXEC file

        // closed, all buffers purged, reset
        // VDU paging counter// otherwise
        // nothing.
        // (Read/Write using OSBYTE 230)
        userVIAIRQBitMask = 0x0277,     // (Read using OSBYTE 231)

        rs423IRQBitMask = 0x0278,     // (Read using OSBYTE 232)
        systemVIAIRQBitMask = 0x0279,     // (Read using OSBYTE 233)
        tubePresentFlag = 0x027A,     // 0 = no Tube

        // 255 = Tube present
        // (Read using OSBYTE 234)
        speechSystemPresentFlag = 0x027B,     // 0 = no speech

        // 255 = SPEECH present
        // (Read using OSBYTE 235)

        characterDestinationsAvailableFlags = 0x027C,     // bit 0 - enable RS-423 driver

        // bit 1 - disable VDU driver
        // bit 2 - disable printer driver
        // bit 3 - enable printer, independent of CTRL-B/C
        // bit 4 - disable SPOOLed output
        // bit 5 - not used
        // bit 6 - disable printer driver(unless preceded by VDU 1)
        // bit 7 - not used
        // (Read using OSBYTE 236)
        cursorEditingType = 0x027D,     // 0 = enable normal cursor editing

        // 1=disable(cursor keys return codes 0x87-0x8B)
        // 2=disable(cursor keys and COPY key
        // are soft keys 11=COPY,12=LEFT,
        //      13=RIGHT,14=DOWN,15=UP)
        // (Read using OSBYTE 237)

        unused27E = 0x027E,     // [initialised to zero, otherwise unused]

        // (Read/Write using OSBYTE 238)
        unused27F = 0x027F,     // [initialised to zero, otherwise unused]

        // (Read/Write using OSBYTE 239)

        countryCode = 0x0280,     // 0 = UK

        // 1 = US(not used by this OS)
        // (Read/Write using OSBYTE 240)
        userFlag = 0x0281,     // free for use by an application(not used by the OS)

        // (Read/Write with OSBYTE 241 but prefer using OSBYTE 1)

        serialULARegisterCopy = 0x0282,     // copy of the Serial ULA control register

        // (Read using OSBYTE 242)
        timeClockSwitch = 0x0283,     // which five byte clock is in currently use(5 or 10)

        // (Read using OSBYTE 243)
        softKeyConsistencyFlag = 0x0284,     // 0 = normal// otherwise inconsistent

        // (Read/Write using OSBYTE 244)
        printerDestination = 0x0285,     // 0 = no printer output

        // 1 = parallel printer
        // 2 = serial printer
        // 3 = user printer routine
        // 4 = net printer
        // 5-255 = user printer routine
        // (Read/Write using OSBYTE 245)
        printerIgnoreCharacter = 0x0286,     // character that the printer ignores

        // (Read/Write using OSBYTE 246)
        breakInterceptJMPInstruction = 0x0287,     // }

        breakInterceptLowAddress = 0x0288,// } Three bytes form a 'JMP address' instruction, activated on BREAK
        breakInterceptHighAddress = 0x0289,     // } (Read / Write using OSBYTE 247 - 249)

        unused28A = 0x028A,//[initialised to zero but otherwise unused]

        // (Read / Write using OSBYTE 250)
        unused28B = 0x028B,//[initialised to zero but otherwise unused]

        // (Read / Write using OSBYTE 251)

        languageROMNumber = 0x028C,// ROM Number for current language

        // (Read / Write using OSBYTE 252)
        lastResetType = 0x028D,// what type of reset was last done?

        // 0    Soft reset(BREAK)
        // 1    Power on
        // 2    Hard reset(CTRL-BREAK)
        // only used during the boot sequence itself
        // (Read / Write using OSBYTE 253)
        systemAvailableRAM = 0x028E,// 0x40 = 16k(usually Model A)

        // 0x80 = 32k (usually Model B)
        // (Read using OSBYTE 254)
        startUpOptions = 0x028F,     // bits 0-2 are the initial MODE

        // bit 3 (if clear, reverses the action of SHIFT-BREAK)
        // bits 4-5 are disc drive timings
        // bits 6-7 unused
        // (Read/Write with OSBYTE 255)

        hardResetLWM = 0x0290,     // low water mark for resetting variables on a hard reset / power on reset

        // (variables are reset from here to end of page)
        vduVerticalAdjust = 0x0290,     // *TV value

        vduInterlaceValue = 0x0291,     // *TV interlace value
        timeClockA = 0x0292,     // } 5 byte clock as read by TIME
        timeClockB = 0x0297,     // } read and write alternates between

        // these two 5 byte buffers
        // (see .timeClockSwitch)
        softResetLWM = 0x029C,     // low water mark in Page 2 for

        // resetting variables on a soft reset
        // (sets variables from here to end
        // of page) - dual use
        countdownIntervalTimer = 0x029C,     // 5 byte countdown interval timer (causes an EVENT when it reaches zero)

        romTypeTable = 0x02A1,     // the type of each of the sixteen ROMs 0x02A1-0x2B0
        inkeyTimeoutCounterLow = 0x02B1,     // } 16 bit value that is decremented
        inkeyTimeoutCounterHigh = 0x02B2,     // } at 100Hz while processing a timed keyboard read

        osword0MaxLineLength = 0x02B3,     // }
        osword0MinASCIICharacter = 0x02B4,    // } copies of the values in the OSWORD 0 parameter block
        osword0MaxASCIICharacter = 0x02B5,     // }
        lowByteLastByteFromADCChannel1 = 0x02B6,     //
        lowByteLastByteFromADCChannel2 = 0x02B7,     //
        lowByteLastByteFromADCChannel3 = 0x02B8,     //
        lowByteLastByteFromADCChannel4 = 0x02B9,     //
        highByteLastByteFromADCChannel1 = 0x02BA,     //
        highByteLastByteFromADCChannel2 = 0x02BB,     //
        highByteLastByteFromADCChannel3 = 0x02BC,     //
        highByteLastByteFromADCChannel4 = 0x02BD,     //
        adcLastChannelRead = 0x02BE,     // Stores the last ADC channel read
                                         // (1-4), or zero if (a) no ADC
                                         // conversions have taken place yet,
                                         // or (b) one is pending via OSBYTE 17
                                         // or (c) if we have already read this
                                         // value using OSBYTE 128.

        eventEnabledFlags = 0x02BF,     // }
        outputBufferEmptyEventEnabled = 0x02BF,     // }
        inputBufferFullEventEnabled = 0x02C0,     // }
        characterEnteringBufferEventEnabled = 0x02C1,     // }
        adcConversionCompleteEventEnabled = 0x02C2,     // }
        startOfVSyncEventEnabled = 0x02C3,     // } 'Event enabled' flags (0 means
        intervalTimerCrossingZeroEventEnabled = 0x02C4,     // } disabled, non-zero is enabled)
        escapeConditionEventEnabled = 0x02C5,     // }
        rs423ErrorDetectedEventEnabled = 0x02C6,     // }
        econetGeneratedEventEnabled = 0x02C7,     // }
        userEventEnabled = 0x02C8,     // }

        softKeyExpansionPointer = 0x02C9,     // next byte to expand is at
                                              // (0x0B09 + .softKeyExpansionPointer)

        keyboardFirstAutorepeatCount = 0x02CA,     //
        previousKeyPressedWhenReadingLastKey = 0x02CB,     // previous key pressed when reading

        // keyboard last key
        // (see .lastKeyPressedInternal)
        previousKeyPressedWhenReadingFirstKey = 0x02CC,     // previous key pressed when reading

        // keyboard first key
        // (see .firstKeyPressedInternal)
        previousKeyPressedWhenReadingOSBYTE = 0x02CD,     // previous key pressed when reading

        // keyboard from OSBYTE 121 / 122

        soundIsUpdatingFlag = 0x02CE,     // aka 'Sound semaphore'
                                          // 0xFF if sound interrupt is updating
                                          // 0x00 otherwise

        bufferEmptyFlags = 0x02CF,     //
        keyboardBufferEmptyFlag = 0x02CF,     // }
        rs423InputBufferEmptyFlag = 0x02D0,     // }
        rs423OutputBufferEmptyFlag = 0x02D1,     // }
        printerBufferEmptyFlag = 0x02D2,     // }
        soundChannel0BufferEmptyFlag = 0x02D3,     // } Bit 7 set if buffer is empty
        soundChannel1BufferEmptyFlag = 0x02D4,     // }
        soundChannel2BufferEmptyFlag = 0x02D5,     // }
        soundChannel3BufferEmptyFlag = 0x02D6,     // }
        speechBufferEmptyFlag = 0x02D7,     // }

        bufferStartIndices = 0x02D8,     //
        keyboardBufferStartIndex = 0x02D8,     // }
        rs423InputBufferStartIndex = 0x02D9,     // }
        rs423OutputBufferStartIndex = 0x02DA,     // }
        printerBufferStartIndex = 0x02DB,     // } Offset to next byte to be removed
        soundChannel0BufferStartIndex = 0x02DC,     // } Highest location in each
        soundChannel1BufferStartIndex = 0x02DD,     // }    buffer has offset 0xFF
        soundChannel2BufferStartIndex = 0x02DE,     // }
        soundChannel3BufferStartIndex = 0x02DF,     // }
        speechBufferEmptyStartIndex = 0x02E0,     // }

        bufferEndIndices = 0x02E1,     //
        keyboardBufferEndIndex = 0x02E1,     // }
        rs423InputBufferEndIndex = 0x02E2,     // }
        rs423OutputBufferEndIndex = 0x02E3,     // }
        printerBufferEndIndex = 0x02E4,     // }
        soundChannel0BufferEndIndex = 0x02E5,     // } Offset to last byte entered in
        soundChannel1BufferEndIndex = 0x02E6,     // } each buffer
        soundChannel2BufferEndIndex = 0x02E7,     // }
        soundChannel3BufferEndIndex = 0x02E8,     // }
        speechBufferEmptyEndIndex = 0x02E9,     // }

        tapeInputCurrentBlockSizeLow = 0x02EA,     //
        tapeInputCurrentBlockSizeHigh = 0x02EB,     //

        blockFlagOfCurrentlyResidentBlock = 0x02EC,     // bit 0 = *RUN only

        // bit 6 = no data
        // bit 7 = last block
        lastCharacterOfCurrentlyResidentBlock = 0x02ED,     //

        osfileBlockStart = 0x02EE,     //
        osfileFilenameAddressLow = 0x02EE,     //
        osfileFilenameAddressHigh = 0x02EF,     //
        osfileLoadAddressLow = 0x02F0,     // the Load/Exec/Start/End addresses
        osfileLoadAddressMid1 = 0x02F1,     // are all 32 bit values. The upper
        osfileLoadAddressMid2 = 0x02F2,     // two bytes are 0xFF 0xFF by default,
        osfileLoadAddressHigh = 0x02F3,     // meaning the code is for the main
        osfileExecAddressLow = 0x02F4,     // processor. If they have other
        osfileExecAddressMid1 = 0x02F5,     // values the data is sent via the
        osfileExecAddressMid2 = 0x02F6,     // Tube to the second processor
        osfileExecAddressHigh = 0x02F7,     // (if present).
        osfileStartAddressLow = 0x02F8,     //
        osfileStartAddressMid1 = 0x02F9,     //
        osfileStartAddressMid2 = 0x02FA,     //
        osfileStartAddressHigh = 0x02FB,     //
        osfileEndAddressLow = 0x02FC,     //
        osfileEndAddressMid1 = 0x02FD,     //
        osfileEndAddressMid2 = 0x02FE,    //
        osfileEndAddressHigh = 0x02FF     //
    }
}