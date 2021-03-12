;; host_multi

(module
  (import "import" "DoThing" (func (param i32 i64 f32 f64) (result i32 i64 f32 f64)))
  (func (export "Test") (param i32 i64 f32 f64) (result i32 i64 f32 f64)
    local.get 0
    local.get 1
    local.get 2
    local.get 3
    call 0
  )
)
