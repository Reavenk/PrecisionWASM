;; i32.wrap_i64
(module
	(func (export "Test") (param i64) ( result i32)
	local.get 0
	i32.wrap_i64
))