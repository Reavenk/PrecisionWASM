;; ForLoop

(module
  (type $t0 (func))
  (type $t1 (func (param i32 i32) (result i32)))
  (type $t2 (func (result i32)))
  (type $t3 (func (param i32)))
  (func $__wasm_call_ctors (type $t0))
  (func $Test (export "Test") (type $t1) (param $p0 i32) (param $p1 i32) (result i32)
    (local $l0 i32) (local $l1 i32) (local $l2 i32)
    i32.const 50
    set_local $l0
    i32.const 50
    call $srand
    block $B0
      block $B1
        call $rand
        i32.const 15
        i32.and
        tee_local $l1
        i32.eqz
        br_if $B1
        i32.const 50
        set_local $l0
        i32.const 0
        set_local $l2
        loop $L2
          call $rand
          get_local $l0
          i32.add
          tee_local $l0
          get_local $p0
          i32.eq
          br_if $B0
          get_local $l2
          i32.const 1
          i32.add
          tee_local $l2
          get_local $l1
          i32.lt_u
          br_if $L2
        end
      end
      get_local $l0
      return
    end
    get_local $p1)
  (func $main (export "main") (type $t2) (result i32)
    (local $l0 i32) (local $l1 i32) (local $l2 i32) (local $l3 i32)
    i32.const 50
    set_local $l0
    i32.const 50
    call $srand
    block $B0
      call $rand
      i32.const 15
      i32.and
      tee_local $l1
      i32.eqz
      br_if $B0
      i32.const 50
      set_local $l2
      i32.const 0
      set_local $l3
      loop $L1
        i32.const 10
        set_local $l0
        call $rand
        get_local $l2
        i32.add
        tee_local $l2
        i32.const 10
        i32.eq
        br_if $B0
        get_local $l2
        set_local $l0
        get_local $l3
        i32.const 1
        i32.add
        tee_local $l3
        get_local $l1
        i32.lt_u
        br_if $L1
      end
    end
    get_local $l0)
  (func $srand (type $t3) (param $p0 i32)
    i32.const 0
    get_local $p0
    i32.const -1
    i32.add
    i64.extend_u/i32
    i64.store offset=1024)
  (func $rand (type $t2) (result i32)
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
