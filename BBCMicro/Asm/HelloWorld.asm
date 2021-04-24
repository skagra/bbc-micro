			PROCESSOR	6502
			INCLUDE		"OS.asm"

; Entry point - always start of first segment
			SEG			entry
			ORG			$400
			JMP			main

; Main routine
			SEG			main
			ORG			$404
main:		LDX			#$0
loop:		LDA			hello,X
			BEQ			end
			INX
			JSR			OSWRCH
			JMP			loop
end:		BRK

; Zero page variables
			SEG			zpvariables
			ORG			$0
hello:		DC			"Hello World"
			BYTE		0


