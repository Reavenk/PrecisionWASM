;; f64.promote_f32
(module
	(func (export "Test") (param f32) ( result f64)
	local.get 0
	f64.promote_f32
))