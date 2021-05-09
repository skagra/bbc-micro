# BBC Micro

**This project is very much a work in progress.**

An emulator of the *BBC Model B Microcomputer*.

* `6502` CPU emulation.
  * Still to do
    * `BCD`
    * `IRQ` handling 
* Loading of binary images:
  * `DASM` assembled image files - type 1 and type 2
  * Core dump files
  * Paged ROM images
* Debugger
  * Display of:
    * Memory
    * Disassembly
    * CPU Registers
    * Stack
    * Memory changes
  * Instructions:
     * `s` - Single step in
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
* 6845 CRTC
  * Full support for rendering of MODES 0-6 
  * Very simple MODE 7 display
* Load and boot BBC OS
* Sideload BBC Basic and other language ROMs
  * Run BBC Basic applications
* Minimal tests
  * Including tests based on `DASM` assembled image files
