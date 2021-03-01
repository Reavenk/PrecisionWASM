;; f64.nearest
(module
	(func (export "Test") (param f64) ( result f64)
	local.get 0
	f64.nearest
))