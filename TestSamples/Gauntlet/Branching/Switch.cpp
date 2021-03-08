// Switch

#define WASM_EXPORT __attribute__((visibility("default")))

WASM_EXPORT
int Test(int idx)
{
  switch(idx)
  {
    case 0:
      return 10;
    case 1:
      return 13;
    case 2:
      return 17;
    case 3:
      return 20;
    case 4:
      return 100;
    case 5:
      return 1000;
    default:
      return -1;
  }
}

WASM_EXPORT
int main() {
  return Test(10);
}
