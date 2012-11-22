 
#include <stdlib.h>
#include <stdio.h>


int main(int argc, char *argv[])
{
	int indx;
	int array_size;
	if (argc > 1)
	{
		array_size = atoi(argv[1]);
	}
	else
	{
		array_size = 1024*124;
	}
	int a = 1;
	int b = 93563 * 983; // justa  random number 
	for (int i=0; i<array_size; i++)
	{
		a=(a*a) % array_size + b/a;
	}
	printf("%i", a);
	return(0);
}
 
 
