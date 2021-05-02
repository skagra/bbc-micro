```
10 PRINT "SEE HOW MANY SUMS YOU" 
20 PRINT "CAN DO IN 15 SECONDS" 
30 PRINT 
40 STARTTIME=TIME 
50 REPEAT 
60 F=RND(12)
60 F=RND(12)
70 G=RND(12)
80 PRINT "WHAT IS ";F;" TIMES "G; 
90 INPUT H
100 IF H=F*G THEN PRINT "CORRECT" ELSE PRINT "WRONG"
110 PRINT
120 UNTIL TIME-STARTTIME>1500
130 PRINT "TIME UP"
```

```
10 FOR ROW = 1 TO 5 
20 FOR STAR = 1 TO 10 
30 PRINT"*"; 
40 NEXT STAR 
50 FOR STRIPE = 1 TO 20 
60 PRINT "="; 
70 NEXT STRIPE 
80 PRINT 
90 NEXT ROW
100 FOR ROW = 1 TO 6
110 FOR STRIPE = 1 TO 30
120 PRINT"=";
130 NEXT STRIPE
140 PRINT
150 NEXT ROW
```

```
10 REM Input a number in a given range
20 REPEAT
30   PRINT "Please give me a number between 0 and 9 "
40   INPUT N
50 UNTIL N >= 0 AND N <= 9
60 PRINT "Thank You"

 10 REM Repeat questions until answered right first time
 20 REPEAT
 30   tries% = 0
 40   REPEAT
 50     PRINT "What is 20 * 23 + 14 * 11 ";
 60     INPUT ans%
 70     tries% = tries% + 1
 80   UNTIL ans% = 20 * 23 + 14 * 11
 90   REPEAT
100     PRINT "What is 12 + 23 * 14 + 6 / 3 ";
110     INPUT ans%
120     tries% = tries% + 1
130   UNTIL ans% = 12 + 23 * 14 + 6 / 3
140 UNTIL tries% = 2
```

```
10 REM QUADRAT
 20 REM JOHN A COLL BASED ON A PROGRAM
 30 REM BY MAX BRAMER, OPEN UNIVERSITY
 40 REM VERSION 1.0 / 16 NOV 81
 50 REM SOLVES AN EQUATION OF THE FORM
 60 REM A*X^2 + B*X + C
 70 ON ERROR GOTO 350
 80 MODE 7
 90 @%=&2020A
100 REPEAT
110 PRINT "What are the three coefficients ";
120 INPUT A,B,C
130 DISCRIM=B^2-4*A*C
140 IF DISCRIM<0 THEN PROCcomplex
150 IF DISCRIM=0 THEN PROCcoincident
160 IF DISCRIM>0 THEN PROCreal
170 PRINT'''
180 UNTIL FALSE
190 END
200
210 DEF PROCcomplex
220 PRINT "Complex roots X=";-B/(2*A);
230 PRINT " +/- "; SQR(-DISCRIM)/(2*A) "i"
240 ENDPROC
250
260 DEF PROCcoincident
270 PRINT"Co-incident roots X=";B/(2*A)
280 ENDPROC
290
300 DEF PROCreal
310 X1=(-B+SQR(DISCRIM))/(2*A)
320 X2=(-B-SQR(DISCRIM))/(2*A)
330 PRINT "Real distinct roots X=";X1;" and X=";X2
340 ENDPROC
350 @%=10:REPORT:PRINT
```

```
 10 REM SQROOT
 20 REM VERSION 1.0 / 16 NOV 81
 30 REM TRADITIONAL ITERATION METHOD
 40 REM TO CALCULATE THE SQUARE ROOT
 50 REM OF A NUMBER TO 3 DECIMAL PLACES
 60 MODE 7
 70 ON ERROR GOTO 300
 80 @%=&2030A
 90 REPEAT
100 count=0
110 REPEAT
120 INPUT "What is your number ",N
130 UNTIL N>0
140 DELTA=N
150 ROOT=N/2
160 T=TIME
170 REPEAT
180 count=count+1
190 DELTA=(N/ROOT-ROOT)/2
200 ROOT=ROOT+DELTA
210 UNTIL ABS(DELTA)<0.001
220 T=TIME-T
230 PRINT
240 PRINT "Number",N
250 PRINT "Root",ROOT
260 PRINT "Iterations",count
270 PRINT "Time",T/100;" seconds"
280 PRINT''
290 UNTIL FALSE
300 @%=10:PRINT:REPORT:PRINT
```

```
90 REM BRIAN2
100 REM (C) BRIAN R SMITH 1980
110 REM ROYAL COLLEGE OF ART, LONDON
120 REM VERSION 1.0 / 16 NOV 81
130 INPUT "NUMBER OF CYCLES e.g. 1 to 5 ",T
140 INPUT "BACKGROUND SYMBOL e.g. + ",D$
150 INPUT "MOTIF (<20 chrs.)",A$
160 INPUT "TEXT AFTER DESIGN",B$
170 CLS
180 F=1
190 READ A,G,S,C,D,N
200 H=(D-C)/N
210 X=0
220 J=1
230 X=X+S
240 Y=SIN(X)
250 Y1=1+INT((Y-C)/H+0.5)
260 I=0
270 I=I+1
280 IF I=Y1 THEN 310
290 PRINT D$;
300 GOTO 420
310 Z=Z+F
320 IF Z>0 THEN 350
330 F=-F
340 GOTO 450
350 IF Z<=LEN(A$) THEN 390
360 F=-F
370 Z=Z-1
380 GOTO 310
390 S$=LEFT$(A$,Z)
400 PRINT S$;
410 I=I+Z
420 IF I<40 THEN 270
430 PRINT
440 GOTO 230
450 J=J+1
460 IF J>T THEN 490
470 Z=Z+1
480 GOTO 310
490 FOR K=1 TO 39
500 PRINT D$;
510 NEXT K
520 PRINT
530 PRINT B$
540 DATA 0,6.4,0.2,-1,1,20
```
