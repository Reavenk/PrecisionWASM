;; f64.div
(module
	(func (export "Test") (param f64 f64) ( result f64)
	local.get 0
	local.get 1
	f64.div
))