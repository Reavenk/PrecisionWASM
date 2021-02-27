;; f32.convert_i32_u
(module
	(func (export "Test") (param i32) ( result f32)
	local.get 0
	f32.convert_i32_u
))