;; f64.ge
(module
	(func (export "Test") (param f64 f64) ( result i32 )
	local.get 0
	local.get 1
	f64.ge
))