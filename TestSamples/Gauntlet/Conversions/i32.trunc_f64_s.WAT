;; i32.trunc_f64_s
(module
	(func (export "Test") (param f64) ( result i32)
	local.get 0
	i32.trunc_f64_s
))