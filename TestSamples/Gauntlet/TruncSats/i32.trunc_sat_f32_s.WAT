;; i32.trunc_sat_f32_s
(module
	(func (export "Test") (param f32) ( result i32)
	local.get 0
	i32.trunc_sat_f32_s
))