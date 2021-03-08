// https://webassembly.studio/
// Rand
#include <stdio.h>
#define WASM_EXPORT __attribute__((visibility("default")))

#include <stdlib.h> 

WASM_EXPORT
int main() {
  srand(50);
  return rand();
}