 
#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main(int argc, char *argv[])
{
	double pi = 0.0;
	double sum = 0.0;
	double i = 0;
	double power3 = 1;
	for (i=0; i+0.5<100000000.0; i++)
	{
		sum += 1.0/(power3*(2.0*i+1.0));
		power3 *= -3.0;
	}
	pi = sqrt(12.0)*sum;
	printf("%.99lf", pi);
	return(0);
}
 
 
