;; MemoryManips_Max1.WAT

(module
	(type $t0 (func (result i32)))
  	(type $t1 (func (param i32) (result i32)))
  
  	(func $Test_Size (export "Test_Size") (type $t0) (result i32)
      (memory.size)
  	)
  
  	(func $Test_Exp (export "Test_Exp") (type $t1) (param $p0 i32) (result i32)
    	(memory.grow
      	(local.get $p0))
  	)
  
  	(memory $M0 1 1)
  	(data $d0 (i32.const 0) "I am the very model of a modern Major-Gineral\0aI've information vegetable, animal, and mineral,\0aI know the kings of England, and I quote the fights historical\0aFrom Marathon to Waterloo, in order categorical;")
)
