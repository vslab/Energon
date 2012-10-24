 
#include <stdlib.h> 
#include <stdio.h>
 
 
#define uint32 unsigned int
 
typedef int (*CMPFUN)(int, int);
 
 
 
 
void ArraySort(int This[], CMPFUN fun_ptr, uint32 the_len)
{
  /* selection sort */

  uint32 indx;
  uint32 indx2;
  uint32 large_pos;
  int temp;
  int large;

  if (the_len <= 1)
    return;

  for (indx = the_len - 1; indx > 0; --indx)
  {
    /* find the largest number, then put it at the end of the array */
    large = This[0];
    large_pos = 0;

    for (indx2 = 1; indx2 <= indx; ++indx2)
    {
      temp = This[indx2];
      if ((*fun_ptr)(temp ,large) > 0)
      {
        large = temp;
        large_pos = indx2;
      }
    }
    This[large_pos] = This[indx];
    This[indx] = large;
  }
}
 
int *my_array;
 
uint32 fill_array(int array_size)
{
  int indx;
  uint32 checksum = 0; 
  for (indx=0; indx < array_size; ++indx)
  {
    checksum += my_array[indx] = rand();
  }
  return checksum;
}
 
int cmpfun(int a, int b)
{
  if (a > b)
    return 1;
  else if (a < b)
    return -1;
  else
    return 0;
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
 
  ArraySort(my_array, cmpfun, array_size);
 
  free(my_array);
  return(0);
}

