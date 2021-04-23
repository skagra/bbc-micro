

start:		PHA
			PHA
			JSR			print
			BRK

print:		LDX			#$0
loop:		LDA			hello,X
			BEQ			end
			INX
			JSR			OSWRCH
			JMP			loop
			RTS

hello:		DC			"Hello World"
			BYTE		0

			Param  DW    0      ; Somewhere in ZP

       LDA   #<data1
       STA   Param
       LDA   #>data1
       STA   Param+1
       JSR   Function
       ...
       LDA   #<data2
       STA   Param
       LDA   #>data2
       STA   Param+1
       JSR   Function