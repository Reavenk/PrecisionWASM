OneParamTests = [
    "f32_abs         ",
    "f32_neg         ",
    "f32_ceil        ",
    "f32_floor       ",
    "f32_trunc       ",
    "f32_nearest     ",
    "f32_sqrt        ",
    "f64_abs         ",
    "f64_neg         ",
    "f64_ceil        ",
    "f64_floor       ",
    "f64_trunc       ",
    "f64_nearest     ",
    "f64_sqrt        "]
    
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
    "f32_add         ",
    "f32_sub         ",
    "f32_mul         ",
    "f32_div         ",
    "f32_min         ",
    "f32_max         ",
    "f32_copysign    ",
    "f64_add         ",
    "f64_sub         ",
    "f64_mul         ",
    "f64_div         ",
    "f64_min         ",
    "f64_max         ",
    "f64_copysign    "]
    
for t in TwoParamTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1]
        
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