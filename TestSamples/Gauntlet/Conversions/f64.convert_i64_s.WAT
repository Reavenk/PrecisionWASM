;; f64.convert_i64_s
(module
	(func (export "Test") (param i64) ( result f64)
	local.get 0
	f64.convert_i64_s
))