;; i64.extend32_s
(module
	(func (export "Test") (param i64) ( result i64)
	local.get 0
	i64.extend32_s
))