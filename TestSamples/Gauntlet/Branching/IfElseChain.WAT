;; IfElseChain

(module
  (type $t0 (func))
  (type $t1 (func (param i32 i32 i32 i32 i32) (result i32)))
  (type $t2 (func (result i32)))
  (type $t3 (func (param i32)))
  (func $__wasm_call_ctors (type $t0))
  (func $Test (export "Test") (type $t1) (param $p0 i32) (param $p1 i32) (param $p2 i32) (param $p3 i32) (param $p4 i32) (result i32)
    (local $l0 i32) (local $l1 i32)
    i32.const 0
    set_local $l0
    block $B0
      call $rand
      tee_local $l1
      get_local $p0
      i32.eq
      br_if $B0
      i32.const 1
      set_local $l0
      get_local $l1
      get_local $p1
      i32.eq
      br_if $B0
      i32.const 2
      set_local $l0
      get_local $l1
      get_local $p2
      i32.eq
      br_if $B0
      i32.const 3
      set_local $l0
      get_local $l1
      get_local $p3
      i32.eq
      br_if $B0
      i32.const 4
      i32.const -1
      get_local $l1
      get_local $p4
      i32.eq
      select
      set_local $l0
    end
    get_local $l0)
  (func $main (export "main") (type $t2) (result i32)
    (local $l0 i32) (local $l1 i32) (local $l2 i32)
    i32.const 50
    call $srand
    block $B0
      block $B1
        block $B2
          block $B3
            call $rand
            tee_local $l0
            i32.const -2
            i32.add
            tee_local $l1
            i32.const 13
            i32.gt_u
            br_if $B3
            i32.const 0
            set_local $l2
            block $B4
              get_local $l1
              br_table $B0 $B3 $B3 $B4 $B3 $B3 $B3 $B3 $B2 $B3 $B3 $B3 $B3 $B1 $B0
            end
            i32.const 1
            return
          end
          i32.const 4
          i32.const -1
          get_local $l0
          i32.const 20
          i32.eq
          select
          return
        end
        i32.const 2
        return
      end
      i32.const 3
      set_local $l2
    end
    get_local $l2)
  (func $srand (export "srand") (type $t3) (param $p0 i32)
    i32.const 0
    get_local $p0
    i32.const -1
    i32.add
    i64.extend_u/i32
    i64.store offset=1024)
  (func $rand (export "rand") (type $t2) (result i32)
    (local $l0 i64)
    i32.const 0
    i32.const 0
    i64.load offset=1024
    i64.const 6364136223846793005
    i64.mul
    i64.const 1
    i64.add
    tee_local $l0
    i64.store offset=1024
    get_local $l0
    i64.const 33
    i64.shr_u
    i32.wrap/i64)
  (table $T0 1 1 anyfunc)
  (memory $memory (export "memory") 2)
  (global $g0 (mut i32) (i32.const 66576))
  (global $__heap_base (export "__heap_base") i32 (i32.const 66576))
  (global $__data_end (export "__data_end") i32 (i32.const 1032))
  (data (i32.const 1024) "\00\00\00\00\00\00\00\00"))
