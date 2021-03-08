//https://webassembly.studio/

#define WASM_EXPORT __attribute__((visibility("default")))
#include <stdlib.h>

WASM_EXPORT
int Test(int i_0, int i_1, int i_2, int i_3, int i_4)
{
	int r = rand();
	if(r == i_0)
		return 0;
	else if(r == i_1)
		return 1;
	else if(r == i_2)
		return 2;
	else if(r == i_3)
		return 3;
	else if(r == i_4)
		return 4;

	return -1;
}

WASM_EXPORT
int main() 
{
  srand(50);
  return Test(2, 5, 10, 15, 20);
}
