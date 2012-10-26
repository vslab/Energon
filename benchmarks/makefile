# I am a comment, and I want to say that the variable CC will be
# the compiler to use.
CC=g++
# Hey!, I am comment number 2. I want to say that CFLAGS will be the
# options I'll pass to the compiler.
CFLAGS=-c -Wall
SRC=src
BIN=bin

all: bubble heap insert merges quick selection memtester MemoryTester

#hello: main.o factorial.o hello.o
#	$(CC) main.o factorial.o hello.o -o hello

bubble: $(SRC)/bubble.c
	$(CC) $(SRC)/bubble.c -o $(BIN)/bubble

heap: $(SRC)/heap.c
	$(CC) $(SRC)/heap.c -o $(BIN)/heap

insert: $(SRC)/insert.c
	$(CC) $(SRC)/insert.c -o $(BIN)/insert

merges: $(SRC)/merges.c
	$(CC) $(SRC)/merges.c -o $(BIN)/merges

quick: $(SRC)/quick.c
	$(CC) $(SRC)/quick.c -o $(BIN)/quick

selection: $(SRC)/selection.c
	$(CC) $(SRC)/selection.c -o $(BIN)/selection

memtester: $(SRC)/memtester.c
	gcc $(SRC)/memtester.c -o $(BIN)/memtester

MemoryTester: $(SRC)/MemoryTester.c
	gcc $(SRC)/MemoryTester.c -o $(BIN)/MemoryTester

clean:
	rm -rf *o bubble heap insert merges quick selection 
