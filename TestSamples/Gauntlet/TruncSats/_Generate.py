OneParamTests = [
    "i32_trunc_sat_f32_s",
    "i32_trunc_sat_f32_u",
    "i32_trunc_sat_f64_s",
    "i32_trunc_sat_f64_u",
    "i64_trunc_sat_f32_s",
    "i64_trunc_sat_f32_u",
    "i64_trunc_sat_f64_s",
    "i64_trunc_sat_f64_u"]
    
    
for t in OneParamTests:
    t = t.strip()
    toks = t.split('_')
    
    operandName = toks[0] + '.' + toks[1] + '_' + toks[2] + '_' + toks[3] + '_' + toks[4]
        
    prog = ";; " + operandName + "\n"
    prog += "(module\n"
    prog += "\t(func (export \"Test\") (param " + toks[3] + ") ( result " + toks[0] + ")\n"
    prog += "\tlocal.get 0\n"
    prog += "\t" + operandName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operandName)
    outFile = open(operandName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()