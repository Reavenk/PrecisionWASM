;; i64.eqz
(module
	(func (export "Test") (param i64) ( result i32 )
	local.get 0
	i64.eqz
))