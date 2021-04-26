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

main:       LDA p1
            ADC p2
            RTS

; Zero page variables
            SEG.U zpvariables
            ORG $0

p1:         DS 1
p2:         DS 1



