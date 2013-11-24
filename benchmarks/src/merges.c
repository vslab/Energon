 
#include <stdlib.h> 
#include <stdio.h>
 
 
#define uint32 unsigned int
 
typedef int (*CMPFUN)(int, int);
 
 
 
#define INSERTION_SORT_BOUND 8 /* boundary point to use insertion sort */
 
void ArraySort(int This[], CMPFUN fun_ptr, uint32 the_len)
{
  uint32 span;
  uint32 lb;
  uint32 ub;
  uint32 indx;
  uint32 indx2;
 
  if (the_len <= 1)
    return;
 
  span = INSERTION_SORT_BOUND;
 
  /* insertion sort the first pass */
  { 
    int prev_val;
    int cur_val;
    int temp_val;

    for (lb = 0; lb < the_len; lb += span)
    {
      if ((ub = lb + span) > the_len) ub = the_len;

      prev_val = This[lb];
 
      for (indx = lb + 1; indx < ub; ++indx)
      {
        cur_val = This[indx];

        if ((*fun_ptr)(prev_val, cur_val) > 0)
        {
          /* out of order: array[indx-1] > array[indx] */
          This[indx] = prev_val; /* move up the larger item first */

          /* find the insertion point for the smaller item */
          for (indx2 = indx - 1; indx2 > lb;)
          {
            temp_val = This[indx2 - 1];
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
  }

  /* second pass merge sort */
  {
    uint32 median;
    int* aux;

    aux = (int*) malloc(sizeof(int) * the_len / 2);
 
    while (span < the_len)
    {
      /* median is the start of second file */
      for (median = span; median < the_len;)
      {
        indx2 = median - 1;
        if ((*fun_ptr)(This[indx2], This[median]) > 0)
        {
          /* the two files are not yet sorted */
          if ((ub = median + span) > the_len)
          {
            ub = the_len;
          }

          /* skip over the already sorted largest elements */
          while ((*fun_ptr)(This[--ub], This[indx2]) >= 0)
          {
          }

          /* copy second file into buffer */
          for (indx = 0; indx2 < ub; ++indx)
          {
            *(aux + indx) = This[++indx2];
          }
          --indx;
          indx2 = median - 1;
          lb = median - span;
          /* merge two files into one */
          for (;;)
          {
            if ((*fun_ptr)(*(aux + indx), This[indx2]) >= 0)
            {
              This[ub--] = *(aux + indx);
              if (indx > 0) --indx;
              else
              {
                /* second file exhausted */
                for (;;)
                {
                  This[ub--] = This[indx2];
                  if (indx2 > lb) --indx2;
                  else goto mydone; /* done */
                }
              }
            }
            else
            {
              This[ub--] = This[indx2];
              if (indx2 > lb) --indx2;
              else
              {
                /* first file exhausted */
                for (;;)
                {
                  This[ub--] = *(aux + indx);
                  if (indx > 0) --indx;
                  else goto mydone; /* done */
                }
              }
            }
          }
        }
        mydone:
        median += span + span;
      }
      span += span;
    }
 
    free(aux);
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

