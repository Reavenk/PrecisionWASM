;; sum_4

(module
  (func (export "Test") (param i32 i64 f32 f64) (result i32)
    local.get 0
    i64.extend_i32_s
   
    local.get 1
    
    local.get 2
    i64.trunc_f32_s
    
    local.get 3
    i64.trunc_f64_s
    
    i64.add
    i64.add
    i64.add
    
    i32.wrap_i64
  )
)
