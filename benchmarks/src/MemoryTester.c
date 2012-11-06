#include <stdio.h>
#include <stdlib.h>
#include <time.h>

typedef int indextype;

int memtestread(int       *array,
		indextype  out_iters,
		indextype  in_iters,
		indextype  delta)
{
    int sum;

    in_iters++;

    __asm__ __volatile__(
	"pushl %%ebp\n"
	"pushl %%ebx\n"

	"xorl   %%ebp, %%ebp\n"
	"xorl   %%ebx, %%ebx\n"

	"mainloop:\n"

	"addl	%%eax, %%ebp\n"

	"movl   %%esi, %%ecx\n"

	"andl	$268435455, %%ebp\n"

	"myloop:\n"
	"subl $1,%%ecx\n"
	"jnz myloop\n"

#ifndef NOMEM
	"addl	(%%edx,%%ebp,4), %%ebx\n"
#endif

	"subl $1,%%edi\n"
	"jnz mainloop\n"

	"movl %%ebx, %%edi\n"

	"popl %%ebx\n"
	"popl %%ebp\n"

	:"=D"(sum)
	:"D"(out_iters),"a"(delta),"S"(in_iters),"d"(array)
);

    return sum;
}



int main(int argc, char **argv)
{
    int *array;
    int sum;
    char dummy;
    indextype size = 256 * 1024 * 1024;
    indextype a = 1*1024 * 1024 + 1;
    indextype out_iters, in_iters;
    indextype i;

    if (argc != 3) {
	printf ("USAGE: %s out_iters in_iters\nExample: %s 268435456 100\n",
		argv[0], argv[0]);
	return -1;
    }

    out_iters = atoi (argv[1]);
    in_iters = atoi (argv[2]);

    array = malloc (sizeof (int) * size);

#ifndef DIRECT

    srand (time (NULL));
    for(i=0; i<size; i++)
	array[i] = rand ();

    printf ("begin\r\n");
    fflush (stdout);

    scanf ("%c", &dummy);

#endif

    sum = memtestread(array, out_iters, in_iters, a);

    printf ("end\r\n");
    fflush (stdout);

    return sum;
}

