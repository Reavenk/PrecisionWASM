;; host_sum4
(module
  (import "import" "sum4" (func (param i32 i64 f32 f64) (result i32)))
  (func (export "AddFour") (param i32 i64 f32 f64) (result i32)
    local.get 0
    local.get 1
    local.get 2
    local.get 3
    call 0
  )
)
