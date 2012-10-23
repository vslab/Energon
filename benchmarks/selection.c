 
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
 
#define ARRAY_SIZE 14
 
int my_array[ARRAY_SIZE];
 
void fill_array()
{
  int indx;

  for (indx=0; indx < ARRAY_SIZE; ++indx)
  {
    my_array[indx] = rand();
  }
  /* my_array[ARRAY_SIZE - 1] = ARRAY_SIZE / 3; */
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

  for (indx2 = 0; indx2 < 80000; ++indx2)
  { 
    fill_array();
    ArraySort(my_array, cmpfun, ARRAY_SIZE);
    for (indx=1; indx < ARRAY_SIZE; ++indx)
    {
      if (my_array[indx - 1] > my_array[indx])
      {
        printf("bad sort\n");
        return(1);
      }
    }
  }

  return(0);
}
 
 
