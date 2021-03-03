;; f32.lt
(module
	(func (export "Test") (param f32 f32) ( result i32 )
	local.get 0
	local.get 1
	f32.lt
))