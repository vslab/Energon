 
#include <stdlib.h> 
#include <stdio.h>
 
 
#define uint32 unsigned int
 
typedef int (*CMPFUN)(int, int);
 
 
 
 
void ArraySort(int This[], CMPFUN fun_ptr, uint32 ub)
{
  /* bubble sort */

  uint32 indx;
  uint32 indx2;
  int temp;
  int temp2;
  int flipped;

  if (ub <= 1)
    return;

  indx = 1; 
  do
  {
    flipped = 0;
    for (indx2 = ub - 1; indx2 >= indx; --indx2)
    {
      temp = This[indx2];
      temp2 = This[indx2 - 1];
      if ((*fun_ptr)(temp2, temp) > 0)
      {
        This[indx2 - 1] = temp;
        This[indx2] = temp2;
        flipped = 1;
      }
    }
  } while ((++indx < ub) && flipped);
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
 
 
