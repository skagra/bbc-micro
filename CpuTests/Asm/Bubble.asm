; Adapted from http://6502.org/source/sorting/bubble8.htm
	     PROCESSOR  6502
         ORG        $400

; Entry point - always start of first segment
			SEG			entry
			ORG			$400

            ; Jump to main
			JMP			main

; Main program
			SEG			main
			ORG			$404

            ; Main subroutine
			SUBROUTINE
main:		LDA			#<tosort
			STA			sortvec
			LDA			#>tosort
			STA			sortvec+1
			JSR			SORT8
			BRK

            ; Bubble sort
SORT8:      LDY #$00            ;TURN EXCHANGE FLAG OFF (= 0)
            STY exflag
            LDA (sortvec),Y     ;FETCH ELEMENT COUNT
            TAX                 ; AND PUT IT INTO X
            INY                 ;POINT TO FIRST ELEMENT IN LIST
            DEX                 ;DECREMENT ELEMENT COUNT
NXTEL:      LDA (sortvec),Y     ;FETCH ELEMENT
            INY
            CMP (sortvec),Y     ;IS IT LARGER THAN THE NEXT ELEMENT?
            BCC CHKEND
            BEQ CHKEND
                                ;YES. EXCHANGE ELEMENTS IN MEMORY
            PHA                 ; BY SAVING LOW BYTE ON STACK.
            LDA (sortvec),Y     ; THEN GET HIGH BYTE AND
            DEY                 ; STORE IT AT LOW ADDRESS
            STA (sortvec),Y
            PLA                 ;PULL LOW BYTE FROM STACK
            INY                 ; AND STORE IT AT HIGH ADDRESS
            STA (sortvec),Y
            LDA #$FF            ;TURN EXCHANGE FLAG ON (= -1)
            STA exflag
CHKEND:     DEX                 ;END OF LIST?
            BNE NXTEL           ;NO. FETCH NEXT ELEMENT
            BIT exflag          ;YES. EXCHANGE FLAG STILL OFF?
            BMI SORT8           ;NO. GO THROUGH LIST AGAIN
            RTS                 ;YES. LIST IS NOW ORDERED

; Working memory
			SEG			working
			ORG			$500

; Values to sort - length in first byte
tosort:     DS          $100

; Zero page variables
			SEG.U		zpvariables
			ORG			$0

; Indirection for print subroutine
sortvec:	DS			2
exflag:     DS          1
