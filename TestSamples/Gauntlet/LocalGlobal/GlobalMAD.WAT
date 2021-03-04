;; GlobalMAD

(module
  (import "env" "add" (global (mut i32)))
  (import "env" "mul" (global (mut i32)))
  (import "env" "res" (global (mut i32)))
  (func (export "Test") (param i32)
    local.get 0
    global.get 0
    i32.mul
    global.get 1
    i32.add
    global.set 2
))
