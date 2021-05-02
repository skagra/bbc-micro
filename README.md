# BBC Micro

An enumulator of the *BBC Model B Microcomputer*.

Work to date has focused on the 6502 CPU, with current status as follows:

* `6502` CPU emulation.
  * *BCD* mode still to do.
* Loading of binary images:
  * `DASM` assembled image files - type 1 and type 2
  * Loading core dump files
  * Loading of paged ROM images.
* Debugger
  * Display of:
    * Memory
    * Disassembly
    * CPU Registers
    * Stack
    * Memory changes
    * Debugger output
    * Programme outout
  * Instructions:
     * `x` - Exit
     * `s` - Single step in
     * `o` - Single step over
     * `r` - Run
     * `t` - Return from subroutine
     * `set` - Set value of register or memory
     * `sb` - Set breakpoint
     * `lb` - List breakpoints
     * `cb` - Clear breakpoint
     * `c` - Dump core image
     * `lm` - List memory
     * `ld` - List disassembly
* OS call interception
  * Very basic text output device (approx. mimics BBC OS `OSWRCH`)
  * Very basic keyboard device (approx. mimics BBC OS `OSRDCH`)
* Very simple 6845 CRTC emulation for MODE 7 display
* Load and boot BBC OS
* Sideload BBC Basic
  * Run BBC Basic applications
* Minimal tests
  * Including tests based on `DASM` assembled image files
