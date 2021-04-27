            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $400

            SUBROUTINE
            JSR main
            BRK

; Main routine
            SEG main
            ORG $408

; Save the status register
            SUBROUTINE
savep:      PHP               
            PLA
            STA savedp
            RTS

; Load the status value
            SUBROUTINE
loadp:      LDA savedp
            PHA
            PLP
            RTS

; Main subroutine
            SUBROUTINE
main:       STA asl
            STA lsr
            STA rol
            STA ror
    
            JSR savep

            JSR loadp
            ASL asl
            PHP
            PLA
            STA asl+1

            JSR loadp
            LSR lsr
            PHP
            PLA
            STA lsr+1

            JSR loadp
            ROL rol
            PHP
            PLA
            STA rol+1
            
            JSR loadp
            ROR ror
            PHP
            PLA
            STA ror+1

            RTS

; Zero page variables
            SEG.U zpvariables
            ORG $0

asl:        DS 2
lsr:        DS 2
rol:        DS 2
ror:        DS 2
savedp:     DS 1

