@ written by user bluRose on the pokecommunity forum (https://www.pokecommunity.com/member.php?u=471720)
@ posted on this post: https://www.pokecommunity.com/showthread.php?t=351387&page=2
.text
.align 2
.thumb

set_hail_environment:
	ldr r3, weather
	ldrh r1, [r3]
	mov r2, #0x80
	mov r0, r2
	and r0, r1
	cmp r0, #0x0
	bne default
	strh r2, [r3, #0x0]
	ldr r1, unk_2023FC4	@ this address +0x10 is the animation we want to play probably
	mov r0, #0xD		@ Hail animation
	strb r0, [r1, #0x10]
	mov r3, r10
	strb r3, [r1, #0x17]
return:
	ldr r0, return1
	bx r0
default:
	ldr r0, default_weather
	bx r0

.align 2
weather:
	.word 0x02023F1C
unk_2023FC4:
	.word 0x02023FC4
default_weather:
	.word 0x0801A251
return1:
	.word 0x0801A247