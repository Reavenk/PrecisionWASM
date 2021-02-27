;; i64.extend_i32_s
(module
	(func (export "Test") (param i32) ( result i64)
	local.get 0
	i64.extend_i32_s
))