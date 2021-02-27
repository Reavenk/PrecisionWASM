;; f32.reinterpret_i32
(module
	(func (export "Test") (param i32) ( result f32)
	local.get 0
	f32.reinterpret_i32
))