;; i64.and
(module
	(func (export "Test") (param i64 i64) ( result i64)
	local.get 0
	local.get 1
	i64.and
))