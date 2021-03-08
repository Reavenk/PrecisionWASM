// https://webassembly.studio/
// ForLoop

#define WASM_EXPORT __attribute__((visibility("default")))
#include <stdlib.h>

WASM_EXPORT
int Test(int stop, int stopVal)
{
  srand(50);
  int ret = 50;
  int iters = rand() & 0x0f;
  for(int i = 0; i < iters; ++i)
  {
    ret += rand();
    if(ret == stop)
      return stopVal;
  }
  return ret;
}

WASM_EXPORT
int main() 
{
  return Test(10, 10);
}
