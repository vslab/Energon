# I am a comment, and I want to say that the variable CC will be
# the compiler to use.
CC=g++
# Hey!, I am comment number 2. I want to say that CFLAGS will be the
# options I'll pass to the compiler.
CFLAGS=-c -Wall

all: bubble heap insert merges quick selection

#hello: main.o factorial.o hello.o
#	$(CC) main.o factorial.o hello.o -o hello

bubble: bubble.c
	$(CC) bubble.c -o bubble

heap: heap.c
	$(CC) heap.c -o heap

insert: insert.c
	$(CC) insert.c -o insert

merges: merges.c
	$(CC) merges.c -o merges

quick: quick.c
	$(CC) quick.c -o quick

selection: selection.c
	$(CC) selection.c -o selection

clean:
	rm -rf *o bubble heap insert merges quick selection 
