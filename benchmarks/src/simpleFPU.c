 
#include <stdlib.h>
#include <stdio.h>
  

int main(int argc, char *argv[])
{
	int indx;
	int array_size;
	int i = 0;
	float a=0.1;
	float b=1.1;

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
		a=(a*a+b/a);
	}
	printf("%f", a);
	return(0);
}
 
 
