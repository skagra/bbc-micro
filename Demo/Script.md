# Demo Script

# Demo 1 - 6502 Emulator

Going to demonstrate 6502 emulation via the debugger using
this simple *"Hello World"* assembly language programme:

* Pull up code
* Indicate segments
* Go to end and point at null terminated string
* And point at the vector and explain how it works
* Then walk through the code 
  * Note: call to OSWRCH as an example of an intercepted OS call

```
/*
 * A programme to print a string as a basic test
 * of the 6502 emulator
 */

; Processor type
            PROCESSOR 6502

; Operating system entry points
            INCLUDE "OS.asm"

; Entry point - always start of first segment
            SEG entry
            ORG $400

            JSR main
            BRK

; Main program
            SEG main
            ORG $408

; Print a null terminated stringe using OSWRCH
            SUBROUTINE
print:      LDY #$0
.loop:      LDA (printvec),Y
            BEQ .end
            JSR OSWRCH
            INY
            JMP .loop
.end:       RTS

; Main subroutine
            SUBROUTINE
main:       LDA #<hello
            STA printvec
            LDA #>hello
            STA printvec+1
            JSR print
            RTS

; Working memory
            SEG working
            ORG $500

; String to print
hello:      DC "Hello World"
            BYTE 0

; Zero page variables
            SEG.U zpvariables
            ORG $0

; Indirection for print subroutine
printvec:   DS 2
```

* Assemble it into machine code:

```
dasm BBCMicro\Asm\HelloWorldJsr.asm -f2 -IBBCMicro\Asm
```

* Dir output file `a.out`.

* Open `a.out` in binary editor and explain some of the format.
  * Segments - Little endian entry point then length
  * Point out the JSR main for example
  * And point to the inline data for "Hello World"
 
* Load the machine code into the emulator:

```
dotnet run -p Debugger a.out
```

* Pull up code beside debugger
* Describe each of the "windows"
  * Code (disassembly)
  * Processor 
  * Memory
  * Stack
  * Debugger output
  * Input
  * Programme output

* Some commands
  * `h` - help
  * `ld 408` - and compare to listing - `LDY #$0`
  * `lm 500` - and compare to listing - "Hello World" data
* Now we can single step through the programme.
  * Explain display changes as it goes
  * Make reference to output characters
  * Continue until first couple of characters printed.
* Dump a core `c` - explain what a core is, we'll come back to this later!
  * Note current state - registers and "Hello...    
* `t` - Run to return
* Exit `x`

Now run again against core

```
dotnet run -p Debugger core.bin
```

* Note we where we left off,  then start single stepping again. 
* Note the rest of the output characters.

# Demo 2 - BBC Emulator

**Ensure CAPS LOCK is on**

```
dotnet run -p BBCMicro
```

* Talk through the load and boot process.
  * Creates memory
  * Created emulated CPU
  * OS ROM paged in
  * Language (BBC Basic) ROM paged in
  * Boot (hard reset CPU => direct via 0xFFFC)
* Explain we are seeing the emulated screen.

We are now going to write a basic programme:

```
10 REM Input a number in a given range
20 REPEAT
30   PRINT "Please give me a number between 0 and 9 "
40   INPUT N
50 UNTIL N >= 0 AND N <= 9
60 PRINT "Thank You"
```

Asks for a number between 0 and 9.

```
CLS
LIST
```

You can see the stored programme.

```
RUN
```

* We are executing the programme:
* Basic programme loaded into emulator memory
  * Executed by BBC basic interpreter in ROM
  * On the BBC OS in ROM
  * Executed by emulated 6502 CPU
  * Displaying on screen via emulated 6845 CRTC
  * All implemented in C#
  * Compiled down to byte code IL
  * Interpreted and JIT'd by dotnet 
  * Running on OS Windows
  * On x86 machine code/CPU

Let's try another:

```
NEW
```

This one to calculate square roots:

```
 10 REM SQROOT
 20 REM VERSION 1.0 / 16 NOV 81
 30 REM TRADITIONAL ITERATION METHOD
 40 REM TO CALCULATE THE SQUARE ROOT
 50 REM OF A NUMBER TO 3 DECIMAL PLACES
 60 MODE 7
 70 ON ERROR GOTO 300
 80 @%=&2030A
 90 REPEAT
100 count=0
110 REPEAT
120 INPUT "What is your number ",N
130 UNTIL N>0
140 DELTA=N
150 ROOT=N/2
160 T=TIME
170 REPEAT
180 count=count+1
190 DELTA=(N/ROOT-ROOT)/2
200 ROOT=ROOT+DELTA
210 UNTIL ABS(DELTA)<0.001
220 T=TIME-T
230 PRINT
240 PRINT "Number",N
250 PRINT "Root",ROOT
260 PRINT "Iterations",count
270 PRINT "Time",T/100;" seconds"
280 PRINT''
290 UNTIL FALSE
300 @%=10:PRINT:REPORT:PRINT
```

Now show some other ROMs.

For example FORTH.

```
dotnet run -p BBCMicro Forth.ROM
```

Then add some numbers:

```
10
30
+
.
```











