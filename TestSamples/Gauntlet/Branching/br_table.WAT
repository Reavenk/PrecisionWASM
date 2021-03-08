;; br_table

(module
	(func (export "Test")(param $ps i32) (param $p0 i32) (param $p1 i32) (param $p2 i32) (param $p3 i32) (result i32)
      	block $B0
      		block $B1
        		block $B2
         	 		block $B3
      					block $Def
            				get_local $ps
      						br_table $B0 $B1 $B2 $B3 $Def
      					end
      					i32.const -1
      					return
      				end
      				local.get $p3
      				return
      			end
      			local.get $p2
      			return
			end  
      		local.get $p1
      		return
      	end
      local.get $p0
))
