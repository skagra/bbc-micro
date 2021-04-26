            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $400
            JSR main
            BRK

main:       ASL 
            ADC #$80
            ROL 
            ASL 
            ADC #$80
            ROL 
            RTS

