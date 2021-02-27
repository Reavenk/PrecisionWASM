;; f64.convert_i32_s
(module
	(func (export "Test") (param i32) ( result f64)
	local.get 0
	f64.convert_i32_s
))