;; i64.trunc_sat_f64_s
(module
	(func (export "Test") (param f64) ( result i64)
	local.get 0
	i64.trunc_sat_f64_s
))