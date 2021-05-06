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
