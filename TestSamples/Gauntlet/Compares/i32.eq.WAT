;; i32.eq
(module
	(func (export "Test") (param i32 i32) ( result i32 )
	local.get 0
	local.get 1
	i32.eq
))