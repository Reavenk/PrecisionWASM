OneParamTests = [
    "i32_clz         ",
    "i32_ctz         ",
    "i32_popcnt      ",
    "i64_clz         ",
    "i64_ctz         ",
    "i64_popcnt      "]
    
for t in OneParamTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1]
        
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

TwoParamTests = [
    "i32_add         ",
    "i32_sub         ",
    "i32_mul         ",
    "i32_div_s       ",
    "i32_div_u       ",
    "i32_rem_s       ",
    "i32_rem_u       ",
    "i32_and         ",
    "i32_or          ",
    "i32_xor         ",
    "i32_shl         ",
    "i32_shr_s       ",
    "i32_shr_u       ",
    "i32_rotl        ",
    "i32_rotr        ",
    "i64_add         ",
    "i64_sub         ",
    "i64_mul         ",
    "i64_div_s       ",
    "i64_div_u       ",
    "i64_rem_s       ",
    "i64_rem_u       ",
    "i64_and         ",
    "i64_or          ",
    "i64_xor         ",
    "i64_shl         ",
    "i64_shr_s       ",
    "i64_shr_u       ",
    "i64_rotl        ",
    "i64_rotr        "]
    
for t in TwoParamTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1]
    if len(toks) > 2:
        operandName += '_' + toks[2]
        
    prog = ";; " + operandName + "\n"
    prog += "(module\n"
    prog += "\t(func (export \"Test\") (param " + toks[0] + " " + toks[0] + ") ( result " + toks[0] + ")\n"
    prog += "\tlocal.get 0\n"
    prog += "\tlocal.get 1\n"
    prog += "\t" + operandName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operandName)
    outFile = open(operandName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()