;; i64.ge_s
(module
	(func (export "Test") (param i64 i64) ( result i32 )
	local.get 0
	local.get 1
	i64.ge_s
))