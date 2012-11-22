 
#include <stdlib.h>
#include <stdio.h>


int main(int argc, char *argv[])
{
	int indx;
	unsigned int array_size;
	unsigned int i = 0;
	unsigned int a = 1;
	unsigned int b = 93563 * 983; // just a  random number 
	if (argc > 1)
	{
		array_size = atoi(argv[1]);
	}
	else
	{
		array_size = 1024*1024;
	}
	for (i=0; i<array_size; i++)
	{
		a=(a*a+b) % array_size + b/(a+1);
	}
	printf("%i", a);
	return(0);
}
 
 
