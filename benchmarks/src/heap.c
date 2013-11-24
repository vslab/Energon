 
#include <stdlib.h> 
#include <stdio.h>
 
 
#define uint32 unsigned int
 
typedef int (*CMPFUN)(int, int);

 
void ArraySort(int This[], CMPFUN fun_ptr, uint32 the_len)
{
  /* heap sort */

  uint32 half;
  uint32 parent;

  if (the_len <= 1)
    return;

  half = the_len >> 1;
  for (parent = half; parent >= 1; --parent)
  {
    int temp;
    int level = 0;
    uint32 child;

    child = parent;
    /* bottom-up downheap */

    /* leaf-search for largest child path */
    while (child <= half)
    {
      ++level;
      child += child;
      if ((child < the_len) &&
          ((*fun_ptr)(This[child], This[child - 1]) > 0))
        ++child;
    }
    /* bottom-up-search for rotation point */
    temp = This[parent - 1];
    for (;;)
    {
      if (parent == child)
        break;
      if ((*fun_ptr)(temp, This[child - 1]) <= 0)
        break;
      child >>= 1;
      --level;
    }
    /* rotate nodes from parent to rotation point */
    for (;level > 0; --level)
    {
      This[(child >> level) - 1] =
        This[(child >> (level - 1)) - 1];
    }
    This[child - 1] = temp;
  }

  --the_len;
  do
  {
    int temp;
    int level = 0;
    uint32 child;

    /* move max element to back of array */
    temp = This[the_len];
    This[the_len] = This[0];
    This[0] = temp;

    child = parent = 1;
    half = the_len >> 1;

    /* bottom-up downheap */

    /* leaf-search for largest child path */
    while (child <= half)
    {
      ++level;
      child += child;
      if ((child < the_len) &&
          ((*fun_ptr)(This[child], This[child - 1]) > 0))
        ++child;
    }
    /* bottom-up-search for rotation point */
    for (;;)
    {
      if (parent == child)
        break;
      if ((*fun_ptr)(temp, This[child - 1]) <= 0)
        break;
      child >>= 1;
      --level;
    }
    /* rotate nodes from parent to rotation point */
    for (;level > 0; --level)
    {
      This[(child >> level) - 1] =
        This[(child >> (level - 1)) - 1];
    }
    This[child - 1] = temp;
  } while (--the_len >= 1);
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

