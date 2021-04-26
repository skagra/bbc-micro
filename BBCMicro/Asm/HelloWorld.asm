            PROCESSOR 6502
            INCLUDE "OS.asm"

; Entry point - always start of first segment
            SEG entry
            ORG $400

            JSR main
            BRK

; Main routine
            SEG main
            ORG $408

main:       LDX #$0
loop:       LDA hello,X
            BEQ end
            INX
            JSR OSWRCH
            JMP loop
end:        RTS

; Zero page variables
            SEG zpvariables
            ORG $0
hello:      DC "Hello World"
            BYTE 0
    