;; i32.eqz
(module
	(func (export "Test") (param i32) ( result i32 )
	local.get 0
	i32.eqz
))