			PROCESSOR	6502
			ORG			$400

OSWRCH = $FFEE
			PROCESSOR	6502
			ORG			$0000

OSWRCH = $FFEE

			LDX			#$0
loop:		LDA			hello,X
			BEQ			end
			INX
			JSR			OSWRCH
			JMP			loop
end:		BRK

hello:		DC			"Hello World"
			BYTE		0
