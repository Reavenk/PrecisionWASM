;; f64.reinterpret_i64
(module
	(func (export "Test") (param i64) ( result f64)
	local.get 0
	f64.reinterpret_i64
))