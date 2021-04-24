			PROCESSOR	6502
			INCLUDE		"OS.asm"

; Entry point - always start of first segment
			SEG			entry
			ORG			$400

			JMP			main

; Main program
			SEG			main
			ORG			$404

			SUBROUTINE
print:		LDY			#$0
.loop:		LDA			(printvec),Y
			BEQ			.end
			JSR			OSWRCH
			INY
			JMP			.loop
.end:		RTS

			SUBROUTINE
main:		LDA			#<hello
			STA			printvec
			LDA			#>hello
			STA			printvec+1
			JSR			print
			BRK

; Working memory
			SEG			working
			ORG			$500

hello:		DC			"Hello World"
			BYTE		0

; Zero page variables
			SEG.U		zpvariables
			ORG			$0

printvec:	DS			2


