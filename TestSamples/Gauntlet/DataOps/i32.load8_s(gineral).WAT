;; i32.load8_s(gineral)
(module
	(memory (data "I am the very model of a modern Major-Gineral\nI've information vegetable, animal, and mineral,\nI know the kings of England, and I quote the fights historical\nFrom Marathon to Waterloo, in order categorical;"))
	(func (export "Test") (param i32) (result i32)
	local.get 0
	i32.load8_s
))