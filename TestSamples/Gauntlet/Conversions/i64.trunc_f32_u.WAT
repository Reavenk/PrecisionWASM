;; i64.trunc_f32_u
(module
	(func (export "Test") (param f32) ( result i64)
	local.get 0
	i64.trunc_f32_u
))