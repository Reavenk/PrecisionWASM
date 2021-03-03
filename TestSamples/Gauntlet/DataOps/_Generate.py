TestMemory = "I am the very model of a modern Major-Gineral\nI've information vegetable, animal, and mineral,\nI know the kings of England, and I quote the fights historical\nFrom Marathon to Waterloo, in order categorical;"

Loads = [
    "i32_load        ",
    "i64_load        ",
    "f32_load        ",
    "f64_load        ",
    "i32_load8_s     ",
    "i32_load8_u     ",
    "i32_load16_s    ",
    "i32_load16_u    ",
    "i64_load8_s     ",
    "i64_load8_u     ",
    "i64_load16_s    ",
    "i64_load16_u    ",
    "i64_load32_s    ",
    "i64_load32_u    "]
    
for t in Loads:
    t = t.strip()
    toks = t.split('_')
    
    operatorName = toks[0] + '.' + toks[1]
    if len(toks) > 2:
        operatorName += '_' + toks[2]
    
    testName = operatorName + "(gineral)"
    
    prog = ";; " + testName + "\n"
    prog += "(module\n"
    prog += "\t(memory (data \"I am the very model of a modern Major-Gineral\\nI've information vegetable, animal, and mineral,\\nI know the kings of England, and I quote the fights historical\\nFrom Marathon to Waterloo, in order categorical;\"))\n"
    prog += "\t(func (export \"Test\") (param i32) (result " + toks[0] + ")\n"
    prog += "\tlocal.get 0\n"
    prog += "\t" + operatorName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + testName )
    outFile = open(testName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()

Store = [
    "i32_store       ",
    "i64_store       ",
    "f32_store       ",
    "f64_store       ",
    "i32_store8      ",
    "i32_store16     ",
    "i64_store8      ",
    "i64_store16     ",
    "i64_store32     "]
    
for t in Store:
    t = t.strip()
    toks = t.split('_')
    
    operatorName = toks[0] + '.' + toks[1]
    testName = operatorName + "(gineral)"
    
    prog = ";; " + testName + "\n"
    prog += "(module\n"
    prog += "\t(memory (data \"I am the very model of a modern Major-Gineral\\nI've information vegetable, animal, and mineral,\\nI know the kings of England, and I quote the fights historical\\nFrom Marathon to Waterloo, in order categorical;\"))\n"
    prog += "\t(func (export \"Test\") (param i32 " + toks[0] + ")\n"
    prog += "\tlocal.get 0\n"
    prog += "\tlocal.get 1\n"
    prog += "\t" + operatorName + "\n"
    prog += "))"
    
    print("Writing minimal test for " + operatorName)
    outFile = open(testName + ".WAT", "w")
    outFile.write(prog)
    outFile.close()