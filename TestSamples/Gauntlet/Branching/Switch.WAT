;; Switch

(module
  (type $t0 (func))
  (type $t1 (func (param i32) (result i32)))
  (type $t2 (func (result i32)))
  (func $__wasm_call_ctors (type $t0))
  (func $Test (export "Test") (type $t1) (param $p0 i32) (result i32)
    block $B0
      get_local $p0
      i32.const 5
      i32.gt_u
      br_if $B0
      get_local $p0
      i32.const 2
      i32.shl
      i32.const 1024
      i32.add
      i32.load
      return
    end
    i32.const -1)
  (func $main (export "main") (type $t2) (result i32)
    i32.const -1)
  (table $T0 1 1 anyfunc)
  (memory $memory (export "memory") 2)
  (global $g0 (mut i32) (i32.const 66592))
  (global $__heap_base (export "__heap_base") i32 (i32.const 66592))
  (global $__data_end (export "__data_end") i32 (i32.const 1048))
  (data (i32.const 1024) "\0a\00\00\00\0d\00\00\00\11\00\00\00\14\00\00\00d\00\00\00\e8\03\00\00"))
