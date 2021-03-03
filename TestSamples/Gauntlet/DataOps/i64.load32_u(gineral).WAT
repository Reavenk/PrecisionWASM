;; i64.load32_u(gineral)
(module
	(memory (data "I am the very model of a modern Major-Gineral\nI've information vegetable, animal, and mineral,\nI know the kings of England, and I quote the fights historical\nFrom Marathon to Waterloo, in order categorical;"))
	(func (export "Test") (param i32) (result i64)
	local.get 0
	i64.load32_u
))