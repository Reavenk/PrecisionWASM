##################################################
##
##  Auto generate the simple WAT unit tests for 
##  wrap, trunc, extend, convert, demote, promote
##  and reinterpret operators.
##
##################################################

tyi32 = "i32";
tyi64 = "i64";
tyf32 = "f32";
tyf64 = "f64";

tests = [
    "i32_wrap_i64       ", 
    "i32_trunc_f32_s    ", 
    "i32_trunc_f32_u    ", 
    "i32_trunc_f64_s    ", 
    "i32_trunc_f64_u    ", 
    "i64_extend_i32_s   ", 
    "i64_extend_i32_u   ", 
    "i64_trunc_f32_s    ", 
    "i64_trunc_f32_u    ", 
    "i64_trunc_f64_s    ", 
    "i64_trunc_f64_u    ", 
    "f32_convert_i32_s  ", 
    "f32_convert_i32_u  ", 
    "f32_convert_i64_s  ", 
    "f32_convert_i64_u  ", 
    "f32_demote_f64     ", 
    "f64_convert_i32_s  ", 
    "f64_convert_i32_u  ", 
    "f64_convert_i64_s  ", 
    "f64_convert_i64_u  ", 
    "f64_promote_f32    ", 
    "i32_reinterpret_f32", 
    "i64_reinterpret_f64", 
    "f32_reinterpret_i32", 
    "f64_reinterpret_i64",
]
    
for t in tests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1] + '_' + toks[2]
    if len(toks) > 3:
        operandName += '_' + toks[3]
        
    prog = ";; " + operandName + "\n"
    prog += "(module\n"
    prog += "\t(func (export \"Test\") (param " + toks[2] + ") ( result " + toks[0] + ")\n"
    prog += "\tlocal.get 0\n"
    prog += "\t" + operandName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operandName)
    outFile = open(operandName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()
    
##################################################
##
##  There's a small group of signed extend operators
##  that take in the same size as the output, even if
##  they're only considering a portion of all the bytes.    
##
##################################################

selfExtendSTests = [
    "i32_extend8_s      ",
    "i32_extend16_s     ",
    "i64_extend8_s      ",
    "i64_extend16_s     ",
    "i64_extend32_s     "
]
    
for t in selfExtendSTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1] + '_' + toks[2]
        
    prog = ";; " + operandName + "\n"
    prog += "(module\n"
    prog += "\t(func (export \"Test\") (param " + toks[0] + ") ( result " + toks[0] + ")\n"
    prog += "\tlocal.get 0\n"
    prog += "\t" + operandName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operandName)
    outFile = open(operandName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()
        