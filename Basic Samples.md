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
140 UNTIL tries% = 2;
```
