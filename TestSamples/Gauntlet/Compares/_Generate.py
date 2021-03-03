OneParamTests = [
    "i32_eqz         ",
    "i64_eqz         "]
    
for t in OneParamTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1]
        
    prog = ";; " + operandName + "\n"
    prog += "(module\n"
    prog += "\t(func (export \"Test\") (param " + toks[0] + ") ( result i32 )\n"
    prog += "\tlocal.get 0\n"
    prog += "\t" + operandName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operandName)
    outFile = open(operandName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()

TwoParamTests = [
    "i32_eq          ",
    "i32_ne          ",
    "i32_lt_s        ",
    "i32_lt_u        ",
    "i32_gt_s        ",
    "i32_gt_u        ",
    "i32_le_s        ",
    "i32_le_u        ",
    "i32_ge_s        ",
    "i32_ge_u        ",
    "i64_eq          ",
    "i64_ne          ",
    "i64_lt_s        ",
    "i64_lt_u        ",
    "i64_gt_s        ",
    "i64_gt_u        ",
    "i64_le_s        ",
    "i64_le_u        ",
    "i64_ge_s        ",
    "i64_ge_u        ",
    "f32_eq          ",
    "f32_ne          ",
    "f32_lt          ",
    "f32_gt          ",
    "f32_le          ",
    "f32_ge          ",
    "f64_eq          ",
    "f64_ne          ",
    "f64_lt          ",
    "f64_gt          ",
    "f64_le          ",
    "f64_ge          "]
    
for t in TwoParamTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1]
    if len(toks) > 2:
        operandName += '_' + toks[2]
        
    prog = ";; " + operandName + "\n"
    prog += "(module\n"
    prog += "\t(func (export \"Test\") (param " + toks[0] + " " + toks[0] + ") ( result i32 )\n"
    prog += "\tlocal.get 0\n"
    prog += "\tlocal.get 1\n"
    prog += "\t" + operandName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operandName)
    outFile = open(operandName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()