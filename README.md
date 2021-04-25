# BBC Micro

An enumulator of the *BBC Model B Microcomputer*.

Work to date has focused on the 6502 CPU, with current status as follows:

* *6502 CPU* - Most opcodes have been implemented, with the following still to do:
  * Interrupt handling 
  * *BCD* mode
  * `BRK` - Is currently coded to throw an exception
* Loading of binary images:
  * `DASM` assembled image files - type 1 and type 2
  * Loading from core dump files
* Execution of binary images:
  * Display of CPU state
  * Single stepping
  * Triggering of core dumps
* OS call interception
  * Very basic text output device (approx. mimics BBC OS `OSWRCH`)
  * Very basic keyboard device (approx. mimics BBC OS `OSRDCH`)
* Minimal tests
  * Including tests based on `DASM` assembled image files
