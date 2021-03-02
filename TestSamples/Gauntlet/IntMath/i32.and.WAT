;; i32.and
(module
	(func (export "Test") (param i32 i32) ( result i32)
	local.get 0
	local.get 1
	i32.and
))