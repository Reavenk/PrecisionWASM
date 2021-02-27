;; i32.reinterpret_f32
(module
	(func (export "Test") (param f32) ( result i32)
	local.get 0
	i32.reinterpret_f32
))