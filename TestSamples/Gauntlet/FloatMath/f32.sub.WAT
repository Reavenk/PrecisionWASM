;; f32.sub
(module
	(func (export "Test") (param f32 f32) ( result f32)
	local.get 0
	local.get 1
	f32.sub
))