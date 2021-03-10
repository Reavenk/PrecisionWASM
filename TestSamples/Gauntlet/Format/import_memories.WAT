;; import_memories

(module
  (import "foo" "bar" (func (param f32)))
  (import "foo" "mem" (memory 10 10))
  (import "foo" "table" (table 5 20 anyfunc)) ;; Initial 5, max 20
  (import "foo" "global_0" (global i32))
  (import "foo" "global_1" (global f64))
  (import "foo" "global_2" (global (mut i64)))
  (type (func (param i32) (result i32)))
  (start 1)
  (func)
  (func (type 1)
    i32.const 42
    i32.const 7
    i32.store 
  )
  (export "e" (func 1))
)
