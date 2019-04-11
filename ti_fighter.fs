FORGET -->>

: -->> 0 EMIT [COMPILE] --> ; IMMEDIATE

: Reload ( --)
    \ Reloads FIGHTER block file
    S" DSK3.FIGHTER" USE
    ;


   0 VALUE Column			\ used by DrawIt
   0 VALUE Row			\ used by DrawIt

   1 CONSTANT Fire?		\ comparison check for fire button
   2 CONSTANT Left?		\ comparison check for left
   4 CONSTANT Right?		\ comparison check for right
   8 CONSTANT Down?		\ comparison check for down
  16 CONSTANT Up?			\ comparison check for up
  13 CONSTANT ENTER		\ key code for ENTER key
 300 CONSTANT DelayTime	\ delay loop count

\ CalcObj : Current CalcObj
0 VALUE CalcObj

\ Sprite number of the kicking sprite
0 VALUE Kicker

\ Sprite number of the kickie sprite
0 VALUE Kickie

\ Calc constants, vars, and obj

8   CONSTANT MAX_KICK_TIME
8   CONSTANT MOD_MOVE_TIME
1   CONSTANT FACE_LEFT
2   CONSTANT FACE_RIGHT
20  CONSTANT COINC_TOLERANCE

\ CalcObj0 : Holds data relating to sprite 0
create (CalcObj0) 16 allot

\ CalcObj1 : Holds data relating to sprite 1
create (CalcObj1) 16 allot

\ Constant Offsets into CalcObj (Fields)
0   CONSTANT CALC_SPRITE
2   CONSTANT CALC_KICK?
4   CONSTANT CALC_KICK_TIME
6   CONSTANT CALC_DIR
8   CONSTANT CALC_ANIM_FRAME
10  CONSTANT CALC_MOVE_TIME
12  CONSTANT CALC_PREV_JOYST
14  CONSTANT CALC_OPP_SPR



HEX
\ user defined graphics
: BrickUDG ( --) \ character data for brick wall
   DATA 4 7FAA D5A8 D0A0 C080 80 DCHAR
   DATA 4 8080 8080 8080 807F 81 DCHAR
   DATA 4 FEA9 4101 0101 0101 82 DCHAR
   DATA 4 0101 0101 0101 01FE 83 DCHAR ;

: LLineUDG ( --) \ Vertical line on left side
   DATA 4 8080 8080 8080 8080 88 DCHAR ;

: RLineUDG ( --) \ Vertical line on right side
   DATA 4 0101 0101 0101 0101 89 DCHAR ;

: BLineUDG ( --) \ Horizontal line on bottom side
   DATA 4 0000 0000 0000 00FF 8A DCHAR ;

: TLineUDG ( --) \ Horizontal line on top side
   DATA 4 FF00 0000 0000 0000 8B DCHAR ;

: LBLineUDG ( --) \ Lines on left and bottom
   DATA 4 8080 8080 8080 80FF 8C DCHAR ;

: RBLineUDG ( --) \ Lines on right and bottom
   DATA 4 0101 0101 0101 01FF 8D DCHAR ;

: LTLineUDG ( --) \ Lines on left and top
   DATA 4 FF80 8080 8080 8080 8E DCHAR ;

: RTLineUDG ( --) \ Lines on right and top
   DATA 4 FF01 0101 0101 0101 8F DCHAR ;

: LBTLineUDG ( --) \ Lines on left, bottom and top
   DATA 4 FF80 8080 8080 80FF 90 DCHAR ;

: RBTLineUDG ( --) \ Lines on right, bottom and top
   DATA 4 FF01 0101 0101 01FF 91 DCHAR ;

: BTLineUDG ( --) \ Lines on bottom and top
   DATA 4 FF00 0000 0000 00FF 92 DCHAR ;

: DiagUpUDG ( --) \ Diagonal from lower left to upper right
   DATA 4 FFFE FCF8 F0E0 C080 98 DCHAR ;

: DiagDownUDG ( --) \ Diagonal from upper right to lower left
   DATA 4 FF7F 3F1F 0F07 0301 99 DCHAR ;

: P1BoxUDG ( --) \ Player 1 square for strength indicator. Color set 20
   DATA 4 FFFF FFFF FFFF FFFF A0 DCHAR ;

: P2BoxUDG ( --) \ Player2 square for strength indicator. Color set 21
   DATA 4 FFFF FFFF FFFF FFFF A8 DCHAR ;

0 CONSTANT SPR_FACE_RIGHT1

: ManRight1 ( --) \ man facing right, frame #1
   DATA 4 0302 0201 0302 0607 100 DCHAR
   DATA 4 0603 0202 0202 0203 101 DCHAR
   DATA 4 0080 8000 C048 6850 102 DCHAR
   DATA 4 40C0 4040 4040 4070 103 DCHAR ;

4 CONSTANT SPR_FACE_RIGHT2

: ManRight2 ( --) \ man facing right, frame #2
   DATA 4 0101 0100 0306 0A07 104 DCHAR
   DATA 4 0203 0202 0204 0406 105 DCHAR
   DATA 4 8040 4080 C868 5040 106 DCHAR
   DATA 4 40C0 4020 1010 101C 107 DCHAR ;

8 CONSTANT SPR_FACE_LEFT1

: ManLeft1 ( --) \ man facing left frame #1
   DATA 4 0001 0100 0312 160A 108 DCHAR
   DATA 4 0203 0202 0202 020F 109 DCHAR
   DATA 4 C040 4080 C040 60E0 10A DCHAR
   DATA 4 60C0 4040 4040 40C0 10B DCHAR ;

C CONSTANT SPR_FACE_LEFT2

: ManLeft2 ( --) \ man facing left frame #2
   DATA 4 0102 0201 1316 0A02 10C DCHAR
   DATA 4 0203 0204 0808 0838 10D DCHAR
   DATA 4 8080 8000 C060 50E0 10E DCHAR
   DATA 4 40C0 4040 4020 2060 10F DCHAR ;

10 CONSTANT SPR_KICK_RIGHT

: ManRightKick ( --) \ man facing right kicks
   DATA 4 0101 0100 0306 0A12 110 DCHAR
   DATA 4 1213 0202 0204 0406 111 DCHAR
   DATA 4 8048 4989 D162 4448 112 DCHAR
   DATA 4 50E0 0000 0000 0000 113 DCHAR ;

14 CONSTANT SPR_KICK_LEFT

: ManLeftKick ( --) \ man facing left kicks
   DATA 4 0112 9291 8B46 2212 114 DCHAR
   DATA 4 0A07 0000 0000 0000 115 DCHAR
   DATA 4 8080 8000 C060 5048 116 DCHAR
   DATA 4 48C8 4040 4020 2060 117 DCHAR ;

DECIMAL

: NextColumn ( --)	\ advance to the next column
   2 +TO Column ;

: DrawIt ( a b c d --)
  \ emits the four ascii characters on the stack to the screen as follows:
  \ ac
  \ bd
   Column 1+ Row 1+ GOTOXY EMIT \ d
   Column 1+ Row GOTOXY EMIT    \ c
   Column Row 1+ GOTOXY EMIT    \ b
   Column Row GOTOXY EMIT       \ a
   NextColumn ;

: DrawBrick ( --) \ draws a brick tile at Column & Row
   128 129 130 131 DrawIt ;

: ManSprite0 ( --) \  Defines sprite 0
    0 128 24 SPR_FACE_RIGHT1 5 SPRITE

    \ Init the CalcObj0
    0 (CalcObj0) CALC_SPRITE + !
    False (CalcObj0) CALC_KICK? + !
    0 (CalcObj0) CALC_KICK_TIME + !
    FACE_RIGHT (CalcObj0) CALC_DIR + !
    0 (CalcObj0) CALC_ANIM_FRAME + !
    0 (CalcObj0) CALC_MOVE_TIME + !
    0 (CalcObj0) CALC_PREV_JOYST + !
    1 (CalcObj0) CALC_OPP_SPR + !
    ;

: ManSprite1 ( --) \  Defines sprite 1
    1 128 200 SPR_FACE_LEFT1 2 SPRITE 

    \ Init the CalcObj1
    1 (CalcObj1) CALC_SPRITE + !
    False (CalcObj1) CALC_KICK? + !
    0 (CalcObj1) CALC_KICK_TIME + !
    FACE_LEFT (CalcObj1) CALC_DIR + !
    0 (CalcObj1) CALC_ANIM_FRAME + !
    0 (CalcObj1) CALC_MOVE_TIME + !
    0 (CalcObj1) CALC_PREV_JOYST + !
    0 (CalcObj1) CALC_OPP_SPR + !
    ;

: DefineGraphics ( --)
  \ define the user defined graphics 
  \ and set colours of character sets
   1 GMODE FALSE SSCROLL !
   32 0 DO I 1 14 COLOR LOOP
   16 1 15 COLOR  	\ brick colour
   17 1 8  COLOR  	\ shrine color
   18 1 8  COLOR  	\ shrine color
   19 8 14  COLOR  	\ shrine corner color
   20 5 14 COLOR  	\ Player 1 strength box (Purple)
   21 2 14  COLOR  	\ Player 2 strength box (Green)
   BrickUDG 
   LLineUDG RLineUDG BLineUDG TLineUDG
   LBLineUDG RBLineUDG LTLineUDG RTLineUDG
   LBTLineUDG RBTLineUDG BTLineUDG 
   DiagUpUDG DiagDownUDG
   P1BoxUDG P2BoxUDG
   ManRight1 ManRight2
   ManLeft1 ManLeft2
   ManRightKick ManLeftKick
   ;

: BrickRows ( --)
    \ draws rows of bricks at top and bottom of screen
    0 TO Column
    20 TO Row
    2 0 DO
    16 0 DO DrawBrick LOOP
       2 +TO Row 0 TO Column
    LOOP
    ;

: ShintoShrine ( x y --) \ (x,y) upper left corner of shrine
    \ draws the shrine
    TO Column
    TO Row

    \ Top line of big beam
    Column Row GOTOXY 142 EMIT
    15 1 DO Column I + Row GOTOXY 139 EMIT LOOP
    Column 15 + Row GOTOXY 143 EMIT

    \ Bottom line of big beam
    1 +TO Row
    Column Row GOTOXY 153 EMIT 
    15 1 DO Column I + Row GOTOXY 138 EMIT LOOP
    Column 15 + Row GOTOXY 152 EMIT 

    \ Left Beam
    1 +TO Row
    3 +TO Column
    9 0 DO Column Row I + GOTOXY 136 EMIT 137 EMIT LOOP

    \ Right Beam
    8 +TO Column
    9 0 DO Column Row I + GOTOXY 136 EMIT 137 EMIT LOOP

    \ Small beam
    -10 +TO Column
    1 +TO Row
    Column Row GOTOXY 144 EMIT
    13 1 DO Column I + Row GOTOXY 146 EMIT LOOP
    Column 13 + Row GOTOXY 145 EMIT
    ;

: Delay ( delay--) 
  \ simple delay loop
   0 DO LOOP ;

: ClrKey ( --)
    \ Wait for no keypress and key? to return -1
    BEGIN
        -1 KEY? <> WHILE
    REPEAT
    ;

: IncMoveTime ( --)
    \ Increments the CALC_MOVE_TIME of current CalcObj
    \ Called from Calc
    1 CalcObj CALC_MOVE_TIME + +!
    CalcObj CALC_MOVE_TIME + @ MOD_MOVE_TIME MOD 0=
    IF
        \ Change the animation frame
        CalcObj CALC_ANIM_FRAME + @ 0=
        IF
            1 CalcObj CALC_ANIM_FRAME + !
        ELSE
            0 CalcObj CALC_ANIM_FRAME + !
        THEN
    THEN
    ;

: MoveLeft ( --)
    \ Handle joystick moving left
    \ Called from Calc
    CalcObj CALC_KICK? + not
    IF
        \ If not kicking
        CalcObj @ 0 -1 SPRVEC    \ Move left
        FACE_LEFT CalcObj CALC_DIR + !
        IncMoveTime
    THEN
    ;

: MoveRight ( --)
    \ Handle joystick moving right
    \ Called from Calc
    CalcObj CALC_KICK? + not
    IF
        \ If not kicking
        CalcObj @ 0 1 SPRVEC    \ Move right
        FACE_RIGHT CalcObj CALC_DIR + !
        IncMoveTime
    THEN
    ;

: Calc ( joy_stick_data calc_obj --)
    \ Save addr of calc_obj
    TO CalcObj

    \ Get joy_stick_data, calc SPRVEC
    DUP Fire? AND Fire? =
    CalcObj CALC_PREV_JOYST + @ Fire? AND Fire? <>
    AND
    IF
        \ Fire (Kick) button is pressed 
        True CalcObj CALC_KICK? + !
        0 CalcObj CALC_KICK_TIME + !
        CalcObj @ 0 0 SPRVEC    \ Stop movement
    ELSE DUP Left? AND Left? =
    IF
        MoveLeft
    ELSE DUP Right? AND Right? =
    IF
        MoveRight
    ELSE
        \ Default
        CalcObj @ 0 0 SPRVEC \ Stop moving
    THEN THEN THEN

    \ Save joyst value
    CalcObj CALC_PREV_JOYST + !


    \ Check if Kick has timed out
    CalcObj CALC_KICK? + @
    IF
        \ Kick in progress
        \ Inc the KICK_TIME
        1 CalcObj CALC_KICK_TIME + +!
        CalcObj CALC_KICK_TIME + @ MAX_KICK_TIME >=
        IF
            \ Kick has timed out
            False CalcObj CALC_KICK? + !
        ELSE
            \ Stop while kicking
            CalcObj @ 0 0 SPRVEC    \ Stop movement
        THEN
    THEN

    \ Set the correct Sprite
    CalcObj CALC_KICK? + @
    IF
        \ Kicking
        CalcObj CALC_DIR + @ FACE_LEFT =
        IF
            \ Kick Left
            CalcObj @ SPR_KICK_LEFT SPRPAT
        ELSE
            \ Kick Right
            CalcObj @ SPR_KICK_RIGHT SPRPAT
        THEN
    ELSE
        \ Not Kicking
        CalcObj CALC_DIR + @ FACE_LEFT =
        IF
            \ Facing Left
            CalcObj CALC_ANIM_FRAME + @ 0=
            IF
                CalcObj @ SPR_FACE_LEFT1 SPRPAT
            ELSE
                CalcObj @ SPR_FACE_LEFT2 SPRPAT
            THEN
        ELSE
            \ Facing Right
            CalcObj CALC_ANIM_FRAME + @ 0=
            IF
                CalcObj @ SPR_FACE_RIGHT1 SPRPAT
            ELSE
                CalcObj @ SPR_FACE_RIGHT2 SPRPAT
            THEN
        THEN
    THEN

    ;

: ProcessKick ( calc_obj --)
    \ calc_obj is doing the kicking

    \ Save addr of calc_obj
    TO CalcObj
    CalcObj @ TO Kicker
    CalcObj CALC_OPP_SPR + @ TO Kickie

    CalcObj CALC_DIR + @ FACE_LEFT = \ kicker is facing left
    Kickie SPRLOC? SWAP DROP 
    Kicker SPRLOC? SWAP DROP 
    < \ kickie is to the left of kicker
    AND \ and the kicker is facing left
    IF
        \ send Kickie flying to the left
        Kickie 0 -25 SPRVEC \ Move left
    ELSE
    CalcObj CALC_DIR + @ FACE_RIGHT = \ kicker is facing right
    Kickie SPRLOC? SWAP DROP 
    Kicker SPRLOC? SWAP DROP 
    > \ kickie is to the right of kicker
    AND \ and kicker is facing right
    IF
        \ send Kickie flying to the right
        Kickie 0 25 SPRVEC \ Move right
    THEN
    THEN
    ;

: CheckCollision ( --)
    COINC_TOLERANCE 0 1 COINC
    IF
        \ Collision.  
        0 0 0 SPRVEC \ Stop moving
        1 0 0 SPRVEC \ Stop moving

        \ Check if sprite one is kicking
        (CalcObj1) CALC_KICK? + @
        IF
            (CalcObj1) ProcessKick
        THEN

        \ Check if sprite zero is kicking
        (CalcObj0) CALC_KICK? + @
        IF
            (CalcObj0) ProcessKick
        THEN


    THEN
;


: MainLoop ( --)
    \ Main game loop. Continue until key is pressed
    BEGIN -1 KEY? = WHILE
        \ Get joystick data
        1 JOYST
        0 JOYST

        \ Calc new sprites and positions
        (CalcObj0) Calc
        (CalcObj1) Calc

        \ Check collision
        CheckCollision

        \  Move the sprites
        0 4 SPRMOV

        \ Sleep for a bit
        DelayTime Delay
    REPEAT
    ;

: Fighter ( --)
    \ entry point of game
    DefineGraphics BrickRows
    9 8 ShintoShrine
    3 MAGNIFY
    ManSprite0
    ManSprite1
    30 23 GOTOXY
    ClrKey
    MainLoop
    ;

