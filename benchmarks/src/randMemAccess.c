 
#include <stdlib.h>
#include <stdio.h>
  
#define uint32 unsigned int

void randMemAccess(int This[], uint32 the_len)
{
	uint32 nextPos = 0;
	uint32 checksum = 0;
	for (long i = 0; i<the_len; i++)
	{
		nextPos = This[nextPos];
		checksum += nextPos;
	}
}
 
//#define ARRAY_SIZE 250000
 
//int my_array[ARRAY_SIZE];
int *my_array;
 
uint32 fill_array(int array_size)
{
  int indx;
  uint32 checksum = 0; 
  for (indx=0; indx < array_size; ++indx)
  {
    checksum += my_array[indx] = rand() % array_size;
  }
  return checksum;
}

#include <stdlib.h>

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
 
 
