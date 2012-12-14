@ECHO OFF

cd src

cl.exe bubble.c
cl.exe quick.c
cl.exe heap.c
cl.exe insert.c
cl.exe merges.c
cl.exe randMemAccess.c
cl.exe simpleFPU.c
cl.exe simpleINT.c
cl.exe pi.c

del *.obj
move *.exe ../

cd ..
