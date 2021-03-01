;; f32.ceil
(module
	(func (export "Test") (param f32) ( result f32)
	local.get 0
	f32.ceil
))