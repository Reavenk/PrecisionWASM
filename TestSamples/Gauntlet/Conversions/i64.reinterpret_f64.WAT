;; i64.reinterpret_f64
(module
	(func (export "Test") (param f64) ( result i64)
	local.get 0
	i64.reinterpret_f64
))