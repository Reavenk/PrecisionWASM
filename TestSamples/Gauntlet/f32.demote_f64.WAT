;; f32.demote_f64
(module
	(func (export "Test") (param f64) ( result f32)
	local.get 0
	f32.demote_f64
))