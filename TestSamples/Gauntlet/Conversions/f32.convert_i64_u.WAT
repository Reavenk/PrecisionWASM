;; f32.convert_i64_u
(module
	(func (export "Test") (param i64) ( result f32)
	local.get 0
	f32.convert_i64_u
))