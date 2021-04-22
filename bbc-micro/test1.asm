			PROCESSOR	6502
			ORG			$0000
	
			LDX			#$0
loop:		LDA			hello,X
			BEQ			end
			INX
			JSR			$FFFE
			JMP			loop
end:		BRK

hello:		DC			"Hi"
			BYTE		0