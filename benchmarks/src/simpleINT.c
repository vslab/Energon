 
#include <stdlib.h>
#include <stdio.h>


int main(int argc, char *argv[])
{
	int indx;
	int array_size;
	int i = 0;
	int a = 1;
	int b = 93563 * 983; // just a  random number 
	if (argc > 1)
	{
		array_size = atoi(argv[1]);
	}
	else
	{
		array_size = 1024*124;
	}
	for (i=0; i<array_size; i++)
	{
		a=(a*a) % array_size + b/a;
	}
	printf("%i", a);
	return(0);
}
 
 
