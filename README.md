# elbsms - [![Build status](https://ci.appveyor.com/api/projects/status/umtjbgudnaq4m5hf?svg=true)](https://ci.appveyor.com/project/eightlittlebits/elbsms)

## what

A Sega Master System emulator in C#

## why

I enjoy the challenge of writing emulators and the master system was one of the first consoles that I ever owned and played. 

Will it be the best emulator in the world? Probably not, I don't have the hardware knowledge (yet, maybe) to do any real investigation myself but I'll do the best with what I do know. And hey, it doesn't need to be the best.

## test results

### Z80 instruction exerciser (zexall_sdsc.sms)

	Z80 instruction exerciser

	ld hl,(nnnn).................OK
	ld sp,(nnnn).................OK
	ld (nnnn),hl.................OK
	ld (nnnn),sp.................OK
	ld <bc,de>,(nnnn)............OK
	ld <ix,iy>,(nnnn)............OK
	ld <ix,iy>,nnnn..............OK
	ld (<ix,iy>+1),nn............OK
	ld <ixh,ixl,iyh,iyl>,nn......OK
	ld a,(nnnn) / ld (nnnn),a....OK
	ldd<r> (1)...................OK
	ldd<r> (2)...................OK
	ldi<r> (1)...................OK
	ldi<r> (2)...................OK
	ld a,<(bc),(de)>.............OK
	ld (nnnn),<ix,iy>............OK
	ld <bc,de,hl,sp>,nnnn........OK
	ld <b,c,d,e,h,l,(hl),a>,nn...OK
	ld (nnnn),<bc,de>............OK
	ld (<bc,de>),a...............OK
	ld (<ix,iy>+1),a.............OK
	ld a,(<ix,iy>+1).............OK
	shf/rot (<ix,iy>+1)..........OK
	ld <h,l>,(<ix,iy>+1).........OK
	ld (<ix,iy>+1),<h,l>.........OK
	ld <b,c,d,e>,(<ix,iy>+1).....OK
	ld (<ix,iy>+1),<b,c,d,e>.....OK
	<inc,dec> c..................OK
	<inc,dec> de.................OK
	<inc,dec> hl.................OK
	<inc,dec> ix.................OK
	<inc,dec> iy.................OK
	<inc,dec> sp.................OK
	<set,res> n,(<ix,iy>+1)......OK
	bit n,(<ix,iy>+1)............OK
	<inc,dec> a..................OK
	<inc,dec> b..................OK
	<inc,dec> bc.................OK
	<inc,dec> d..................OK
	<inc,dec> e..................OK
	<inc,dec> h..................OK
	<inc,dec> l..................OK
	<inc,dec> (hl)...............OK
	<inc,dec> ixh................OK
	<inc,dec> ixl................OK
	<inc,dec> iyh................OK
	<inc,dec> iyl................OK
	ld <bcdehla>,<bcdehla>.......OK
	cpd<r>.......................OK
	cpi<r>.......................OK
	<inc,dec> (<ix,iy>+1)........OK
	<rlca,rrca,rla,rra>..........OK
	shf/rot <b,c,d,e,h,l,(hl),a>.OK
	ld <bcdexya>,<bcdexya>.......OK
	<rrd,rld>....................OK
	<set,res> n,<bcdehl(hl)a>....OK
	neg..........................OK
	add hl,<bc,de,hl,sp>.........OK
	add ix,<bc,de,ix,sp>.........OK
	add iy,<bc,de,iy,sp>.........OK
	aluop a,nn...................OK
	<adc,sbc> hl,<bc,de,hl,sp>...OK
	bit n,<b,c,d,e,h,l,(hl),a>...OK
	<daa,cpl,scf,ccf>............OK
	aluop a,(<ix,iy>+1)..........OK
	aluop a,<ixh,ixl,iyh,iyl>....OK
	aluop a,<b,c,d,e,h,l,(hl),a>.OK

## resources

* codeslinger - http://www.codeslinger.co.uk/pages/projects/mastersystem.html
* smspower documents - http://www.smspower.org/Development/Documents
* masterfudgemk2 - https://github.com/xdanieldzd/MasterFudgeMk2
* baltazar studios z80 timings - http://baltazarstudios.com/zilog-z80-undocumented-behavior/
* interrupt timings - http://www.z80.info/interrup.htm

## license

MIT License

Copyright (c) 2017-2019 David Parrott

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
