 
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
 
#define ARRAY_SIZE 14
 
int my_array[ARRAY_SIZE];
 
uint32 fill_array()
{
  int indx;
  uint32 checksum = 0; 
  for (indx=0; indx < ARRAY_SIZE; ++indx)
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
 
int main()
{
  int indx;
  int indx2;
  uint32 checksum1;
  uint32 checksum2;

  for (indx2 = 0; indx2 < 80000; ++indx2)
  {
    checksum1 = fill_array();
    ArraySort(my_array, cmpfun, ARRAY_SIZE);
    for (indx=1; indx < ARRAY_SIZE; ++indx)
    {
      if (my_array[indx - 1] > my_array[indx])
      {
        printf("bad sort\n");
        return(1);
      }
    }
    checksum2 = 0;
    for (indx=0; indx < ARRAY_SIZE; ++indx)
    {
      checksum2 += my_array[indx];
    }
    if (checksum1 != checksum2)
    {
      printf("bad checksum %d %d\n", checksum1, checksum2);
    }
  }
 
  return(0);
}
 
 
