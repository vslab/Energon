 
#include <stdlib.h>
#include <stdio.h>
  

void randMemAccess(unsigned int This[], unsigned int the_len)
{
	unsigned int nextPos = 0;
	unsigned int checksum = 0;
	unsigned int i = 0;
	for (i = 0; i < the_len; i++)
	{
		nextPos = This[nextPos];
		checksum += nextPos;
	}
}
 
//#define ARRAY_SIZE 250000
 
//int my_array[ARRAY_SIZE];
int *my_array;
 
 unsigned int fill_array(int array_size)
{
  int indx;
  unsigned int checksum = 0; 
  for (indx=0; indx < array_size; ++indx)
  {
    checksum += my_array[indx] = rand() % array_size;
  }
  return checksum;
}


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
		array_size = 1024;
	}

	my_array = (int *) malloc(sizeof(int)*array_size);
	fill_array(array_size);
 
	randMemAccess(my_array, array_size);
 
	free(my_array);
	return(0);
}
 
 
