;; multi_Add10

(module
  (func (export "Test_Add10") (param i32 i64 f32 f64) (result i32 i64 f32 f64)
    local.get 0
    i32.const 10
    i32.add
    
    local.get 1
    i64.const 10
    i64.add
    
    local.get 2
    f32.const 10.0
    f32.add
    
    local.get 3
    f64.const 10
    f64.add
  )
)
