 
#include <stdlib.h>
#include <stdio.h>
 
 
#define uint32 unsigned int
 
typedef int (*CMPFUN)(int, int);
 
 
 
void ArraySort(int This[], CMPFUN fun_ptr, uint32 the_len)
{
  /* insertion sort */

  uint32 indx;
  int cur_val;
  int prev_val;

  if (the_len <= 1)
    return;

  prev_val = This[0];

  for (indx = 1; indx < the_len; ++indx)
  {
    cur_val = This[indx];
    if ((*fun_ptr)(prev_val, cur_val) > 0)
    {
      /* out of order: array[indx-1] > array[indx] */
      uint32 indx2;
      This[indx] = prev_val; /* move up the larger item first */

      /* find the insertion point for the smaller item */
      for (indx2 = indx - 1; indx2 > 0;)
      {
        int temp_val = This[indx2 - 1];
        if ((*fun_ptr)(temp_val, cur_val) > 0)
        {
          This[indx2--] = temp_val;
          /* still out of order, move up 1 slot to make room */
        }
        else
          break;
      }
      This[indx2] = cur_val; /* insert the smaller item right here */
    }
    else
    {
      /* in order, advance to next element */
      prev_val = cur_val;
    }
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

