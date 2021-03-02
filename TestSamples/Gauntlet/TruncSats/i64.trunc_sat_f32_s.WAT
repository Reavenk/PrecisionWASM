;; i64.trunc_sat_f32_s
(module
	(func (export "Test") (param f32) ( result i64)
	local.get 0
	i64.trunc_sat_f32_s
))