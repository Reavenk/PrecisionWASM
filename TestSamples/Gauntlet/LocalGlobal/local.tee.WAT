;; local.tee
;;
;; Add both parameters (to validate the second was initially different)
;; Set the 2nd parameter with the first, but use tee to leave the value on the stack.
;; Add the second parameter on the stack.
;;
;; The results should be the two parameters added up, and then the current first
;; and second parameter, but they should now both be the first parameter value.

(module
	(func (export "Testi32") (param i32 i32) (result i32 i32 i32)
		local.get 0
		local.get 1
		i32.add
		local.get 0
		local.tee 1
		local.get 1)
  
	(func (export "Testi64") (param i64 i64) (result i64 i64 i64)
		local.get 0
		local.get 1
		i64.add
		local.get 0
		local.tee 1
		local.get 1)
		
	(func (export "Testf32") (param f32 f32) (result f32 f32 f32)
		local.get 0
		local.get 1
		f32.add
		local.get 0
		local.tee 1
		local.get 1)
		
	(func (export "Testf64") (param f64 f64) (result f64 f64 f64)
		local.get 0
		local.get 1
		f64.add
		local.get 0
		local.tee 1
		local.get 1)
)