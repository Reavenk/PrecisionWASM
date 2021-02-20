// MIT License
// 
// Copyright (c) 2021 Pixel Precision, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;

namespace PxPre.WASM
{
    public class Parser
    {
        public class FunctionParse
        {
            public string exportName = null;
            public string fnName = "";
            public List<string> paramTys = new List<string>();
            public List<string> resultTys = new List<string>();
            public List<byte> bytecode = new List<byte>();
        }

        public static byte [] ParseAndCompile(string lowlevstr)
        { 
            List<string> tokens = new List<string>();

            int line = 1;

            for(int i = 0; i < lowlevstr.Length; )
            { 
                char c = lowlevstr[i];
                if(c == '(' || c == ')' )
                { 
                    tokens.Add(c.ToString());
                    ++i;
                    continue;
                }
                else if(c == ' ' || c == '\t' || c == '\r')
                { 
                    ++i;
                    continue;
                }
                else if(c == '\n')
                { 
                    ++i;
                    ++line;
                    continue;
                }
                else if(c == '\"')
                { 
                    // We want to also include the quotes to declare that it's a 
                    // string type. 
                    //
                    // Anything that wants the pure string value needs
                    // to strip out the quotes themselves. Anything that does can
                    // use the asertion that if it starts with a ", there's also an
                    // ending ".
                    int startIdx = i;
                    ++i;

                    while(i < lowlevstr.Length && lowlevstr[i] != '\"')
                        ++i;

                    if(startIdx == i + 1)
                    { 
                        throw new System.Exception(""); // TODO: Error message
                    }

                    ++i;
                    tokens.Add(lowlevstr.Substring(startIdx, i - startIdx));
                }
                else
                { 
                    int startIdx = i;
                    ++i;
                    while(i < lowlevstr.Length)
                    { 
                        c = lowlevstr[i];

                        if (c == ' ' || c == '\n' || c == '\r' || c == '\t' || c == ')')
                            break;

                        ++i;

                    }

                    tokens.Add(lowlevstr.Substring(startIdx, i - startIdx));
                }
            }

            List<byte> retLst = new List<byte>();

            if(tokens.Count == 0)
                return null;

            if(tokens.Count > 4 && tokens[0] == "(" && tokens[1] == "module")
            {
                int idx = 0;
                // Full blown parsing
                return GatherModule(retLst, tokens, ref idx);
            }
            else
            { 
                // Scriptlet parsing
                foreach(string t in tokens)
                {
                    if(t == "i32.get" || t == "f32.get" || t == "i64.get" || t == "f64.get")
                        throw new System.Exception(""); // TODO: Error message

                    int idx = 0;
                    GatherFunction(retLst, tokens, ref idx);
                }
            }

            return retLst.ToArray();
        }

        public static byte [] GatherModule(List<byte> outBytecode, List<string> tokens, ref int idx)
        {
            if(tokens[idx + 0] != "(" || tokens[idx + 1] != "module")
                throw new System.Exception(); // TODO: Error message

            idx += 2;

            List<FunctionParse> fnsParsed = new List<FunctionParse>();

            while(true)
            { 
                string next = tokens[idx];
                if(next == ")")
                {
                    ++idx;
                    break;
                }

                if(tokens[idx] != "(")
                    throw new System.Exception(""); // TODO: Error message

                if (tokens[idx + 1] == "func")
                {
                    FunctionParse fnp = GatherFunction( outBytecode, tokens, ref idx);
                    fnsParsed.Add(fnp);
                }
            }

            return null;
        }

        public static FunctionParse GatherFunction(List<byte> outBytecode, List<string> tokens, ref int idx)
        { 
            if(tokens[idx + 0] != "(" || tokens[idx + 1] != "func")
                throw new System.Exception(); // TODO: Error message

            FunctionParse ret = new FunctionParse();

            idx += 2;

            if (tokens[idx][0] == '$')
            {
                ret.fnName = tokens[idx];
                ++idx;
            }

            while (true)
            { 
                if(tokens[idx] != "(")
                    break;

                if(tokens[idx + 1] == "export")
                {
                    idx += 2;
                    while(true)
                    { 
                        if(idx >= tokens.Count)
                            throw new System.Exception();//

                        if(tokens[idx] == ")")
                        {
                            ++idx;
                            break;
                        }

                        if(string.IsNullOrEmpty(ret.exportName) == false)
                            throw new System.Exception();

                        ret.exportName = tokens[idx];
                        ++idx;
                    }
                }
                else if(tokens[idx + 1] == "param")
                { 
                    idx += 2;
                    while(true)
                    { 
                        if(idx >= tokens.Count)
                            throw new System.Exception(); //

                        if(tokens[idx] == ")")
                        { 
                            ++idx;
                            break;
                        }

                        ret.paramTys.Add( tokens[idx] );
                        ++idx;
                    }
                }
                else if(tokens[idx + 1] == "result")
                { 
                    idx += 2;
                    while(true)
                    { 
                        if(idx >= tokens.Count)
                            throw new System.Exception();

                        if(tokens[idx] == ")")
                        { 
                            ++idx;
                            break;
                        }

                        ret.resultTys.Add( tokens[idx] );
                        ++idx;
                    }
                }
            }

            GatherFunctionInternal(ret.bytecode, tokens, ref idx);

            if (tokens[idx] != ")")
                throw new System.Exception();


            return ret;
        }

        public static bool InsertIntLEB32FromString(string val, List<byte> output, bool unsigned)
        {
            // https://en.wikipedia.org/wiki/LEB128
            if (unsigned == true)
            { 
                uint nval;
                if(uint.TryParse(val, out nval) == true)
                    return false;

                do
                { 
                    byte b = (byte)(nval & ((1 << 7)-1));
                    nval >>= 7;

                    if(nval != 0)
                        b |= (1 << 7);

                    output.Add(b);
                }
                while(nval != 0);

                return true;
            }
            else
            { 
                int nval;
                if(int.TryParse(val, out nval) == true)
                    return false;

                bool more = true;
                bool neg = nval < 0;

                while(more == true)
                { 
                    byte b = (byte)(nval & ((1 <<7)-1));
                    nval >>= 7;

                    if(neg == true)
                        nval |= (~0 << (32 - 7));

                    if((nval == 0 && (b & (1<<7)) == 0 ) || (nval != -1 && (b & (1<<7)) != 0))
                        more = false;
                    else
                        b |= (1 << 7);

                    output.Add(b);
                }

                return true;
            }
        }

        public static bool InsertIntLEB64FromString(string val, List<byte> output, bool unsigned)
        {
            // A copy of InsertIntLEB64FromString except with longs

            // https://en.wikipedia.org/wiki/LEB128
            if (unsigned == true)
            {
                ulong nval;
                if (ulong.TryParse(val, out nval) == true)
                    return false;

                do
                {
                    byte b = (byte)(nval & ((1 << 7) - 1));
                    nval >>= 7;

                    if (nval != 0)
                        b |= (1 << 7);

                    output.Add(b);
                }
                while (nval != 0);

                return true;
            }
            else
            {
                long nval;
                if (long.TryParse(val, out nval) == true)
                    return false;

                bool more = true;
                bool neg = nval < 0;

                while (more == true)
                {
                    byte b = (byte)(nval & ((1 << 7) - 1));
                    nval >>= 7;

                    if (neg == true)
                        nval |= (~0 << (64 - 7));

                    if ((nval == 0 && (b & (1 << 7)) == 0) || (nval != -1 && (b & (1 << 7)) != 0))
                        more = false;
                    else
                        b |= (1 << 7);

                    output.Add(b);
                }

                return true;
            }
        }

        public static bool InsertFloat32FromString(string val, List<byte> output)
        {
            float f;
            if(float.TryParse(val, out f) == false)
                return false;

            output.AddRange(System.BitConverter.GetBytes(f));
            return true;
        }

        public static bool InsertFloat64FromString(string val, List<byte> output)
        {
            double d;
            if(double.TryParse(val, out d) == false)
                return false;

            output.AddRange( System.BitConverter.GetBytes(d));
            return true;
        }


        public static void GatherFunctionInternal(List<byte> outBytecode, List<string> tokens, ref int idx)
        {
            while (idx < tokens.Count)
            {
                if (tokens[idx] == ")")
                    return;

                string op = tokens[idx];
                ++idx;

                switch(op)
                { 
                    case "unreachable":
                        outBytecode.Add((byte)Instruction.unreachable);
                        break;

                    case "nop":
                        outBytecode.Add((byte)Instruction.nop);
                        break;

                    case "block":
                        outBytecode.Add((byte)Instruction.block);
                        break;

                    case "loop":
                        outBytecode.Add((byte)Instruction.loop);
                        break;

                    case "if":
                        outBytecode.Add((byte)Instruction.ifblock);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true); // TODO: multiple things
                        ++idx;
                        break;

                    case "else":
                        outBytecode.Add((byte)Instruction.elseblock);
                        break;

                    case "br":
                        outBytecode.Add((byte)Instruction.br);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true); // !TODO: labelidx
                        ++idx;
                        break;

                    case "br_if":
                        outBytecode.Add((byte)Instruction.br_if);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true); // !TODO: labelidx
                        ++idx;
                        break;

                    case "br_table":
                        outBytecode.Add((byte)Instruction.br_table);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true); // !TODO: vec(labelidx)
                        ++idx;
                        break;

                    case "return":
                        outBytecode.Add((byte)Instruction.returnblock);
                        break;

                    case "call":
                        outBytecode.Add((byte)Instruction.call);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true); // !TODO: funcidx
                        ++idx;
                        break;

                    case "call_indirect":
                        outBytecode.Add((byte)Instruction.call_indirect);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true); // !TODO: typeidx
                        ++idx;
                        break;

                    case "local.get":
                        outBytecode.Add((byte)Instruction.local_get);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "local.set":
                        outBytecode.Add((byte)Instruction.local_set);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "local.tee":
                        outBytecode.Add((byte)Instruction.local_tee);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "global.get":
                        outBytecode.Add((byte)Instruction.global_get);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "global.set":
                        outBytecode.Add((byte)Instruction.global_set);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.load":
                        outBytecode.Add((byte)Instruction.i32_load);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load":
                        outBytecode.Add((byte)Instruction.i64_load);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "f32.load":
                        outBytecode.Add((byte)Instruction.f32_load);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "f64.load":
                        outBytecode.Add((byte)Instruction.f64_load);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.load8_s":
                        outBytecode.Add((byte)Instruction.i32_load8_s);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.load8_u":
                        outBytecode.Add((byte)Instruction.i32_load8_u);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.load16_s":
                        outBytecode.Add((byte)Instruction.i32_load16_s);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.load16_u":
                        outBytecode.Add((byte)Instruction.i32_load16_u);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load8_s":
                        outBytecode.Add((byte)Instruction.i64_load8_s);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load8_u":
                        outBytecode.Add((byte)Instruction.i64_load8_u);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load16_s":
                        outBytecode.Add((byte)Instruction.i64_load16_s);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load16_u":
                        outBytecode.Add((byte)Instruction.i64_load16_u);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load32_s":
                        outBytecode.Add((byte)Instruction.i64_load32_s);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.load32_u":
                        outBytecode.Add((byte)Instruction.i64_load32_s);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.store":
                        outBytecode.Add((byte)Instruction.i32_store);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.store":
                        outBytecode.Add((byte)Instruction.i64_store);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "f32.store":
                        outBytecode.Add((byte)Instruction.f32_store);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "f64.store":
                        outBytecode.Add((byte)Instruction.f64_store);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.store8":
                        outBytecode.Add((byte)Instruction.i32_store8);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i32.store16":
                        outBytecode.Add((byte)Instruction.i32_store16);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.store8":
                        outBytecode.Add((byte)Instruction.i64_store);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.store16":
                        outBytecode.Add((byte)Instruction.i64_store16);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "i64.store32":
                        outBytecode.Add((byte)Instruction.i64_store32);
                        InsertIntLEB32FromString(tokens[idx], outBytecode, true);
                        ++idx;
                        break;

                    case "memory.size":
                        outBytecode.Add((byte)Instruction.MemorySize);
                        break;

                    case "memory.grow":
                        outBytecode.Add((byte)Instruction.MemoryGrow);
                        break;

                    case "i32.const":
                        outBytecode.Add((byte)Instruction.i32_const);

                        InsertIntLEB32FromString(tokens[idx], outBytecode, tokens[idx][0] != '-');
                        ++idx;
                        break;

                    case "i64.const":
                        outBytecode.Add((byte)Instruction.i64_const);
                        InsertIntLEB64FromString(tokens[idx], outBytecode, tokens[idx][0] != '-');
                        ++idx;
                        break;

                    case "f32.const":
                        outBytecode.Add((byte)Instruction.f32_const);
                        InsertFloat32FromString(tokens[idx], outBytecode);
                        ++idx;
                        break;

                    case "f64.const":
                        outBytecode.Add((byte)Instruction.f64_const);
                        InsertFloat64FromString(tokens[idx], outBytecode);
                        ++idx;
                        break;

                    case "i32.eqz":
                        outBytecode.Add((byte)Instruction.i32_eqz);
                        break;

                    case "i32.eq":
                        outBytecode.Add((byte)Instruction.i32_eq);
                        break;

                    case "i32.ne":
                        outBytecode.Add((byte)Instruction.i32_ne);
                        break;

                    case "i32.lt_s":
                        outBytecode.Add((byte)Instruction.i32_lt_s);
                        break;

                    case "i32.lt_u":
                        outBytecode.Add((byte)Instruction.i32_lt_u);
                        break;

                    case "i32.gt_s":
                        outBytecode.Add((byte)Instruction.i32_gt_s);
                        break;

                    case "i32.gt_u":
                        outBytecode.Add((byte)Instruction.i32_gt_u);
                        break;

                    case "i32.le_s":
                        outBytecode.Add((byte)Instruction.i32_le_s);
                        break;

                    case "i32.le_u":
                        outBytecode.Add((byte)Instruction.i32_le_u);
                        break;

                    case "i32.ge_s":
                        outBytecode.Add((byte)Instruction.i32_ge_s);
                        break;

                    case "i32.ge_u":
                        outBytecode.Add((byte)Instruction.i32_ge_u);
                        break;

                    case "i64.eqz":
                        outBytecode.Add((byte)Instruction.i64_eqz);
                        break;

                    case "i64.eq":
                        outBytecode.Add((byte)Instruction.i64_eq);
                        break;

                    case "i64.ne":
                        outBytecode.Add((byte)Instruction.i64_ne);
                        break;

                    case "i64.lt_s":
                        outBytecode.Add((byte)Instruction.i64_lt_s);
                        break;

                    case "i64.lt_u":
                        outBytecode.Add((byte)Instruction.i64_lt_u);
                        break;

                    case "i64.gt_s":
                        outBytecode.Add((byte)Instruction.i64_gt_s);
                        break;

                    case "i64.gt_u":
                        outBytecode.Add((byte)Instruction.i64_gt_u);
                        break;

                    case "i64.le_s":
                        outBytecode.Add((byte)Instruction.i64_le_s);
                        break;

                    case "i64.le_u":
                        outBytecode.Add((byte)Instruction.i64_le_u);
                        break;

                    case "i64.ge_s":
                        outBytecode.Add((byte)Instruction.i64_ge_s);
                        break;

                    case "i64.ge_u":
                        outBytecode.Add((byte)Instruction.i64_ge_u);
                        break;

                    case "f32.eq":
                        outBytecode.Add((byte)Instruction.f32_eq);
                        break;

                    case "f32.ne":
                        outBytecode.Add((byte)Instruction.f32_ne);
                        break;

                    case "f32.lt":
                        outBytecode.Add((byte)Instruction.f32_lt);
                        break;

                    case "f32.gt":
                        outBytecode.Add((byte)Instruction.f32_gt);
                        break;

                    case "f32.le":
                        outBytecode.Add((byte)Instruction.f32_le);
                        break;

                    case "f32.ge":
                        outBytecode.Add((byte)Instruction.f32_ge);
                        break;

                    case "f64.eq":
                        outBytecode.Add((byte)Instruction.f64_eq);
                        break;

                    case "f64.ne":
                        outBytecode.Add((byte)Instruction.f64_ne);
                        break;

                    case "f64.lt":
                        outBytecode.Add((byte)Instruction.f32_lt);
                        break;

                    case "f64.gt":
                        outBytecode.Add((byte)Instruction.f64_gt);
                        break;

                    case "f64.le":
                        outBytecode.Add((byte)Instruction.f64_le);
                        break;

                    case "f64.ge":
                        outBytecode.Add((byte)Instruction.f64_ge);
                        break;

                    case "i32.clz":
                        outBytecode.Add((byte)Instruction.i32_clz);
                        break;

                    case "i32.ctz":
                        outBytecode.Add((byte)Instruction.i32_ctz);
                        break;

                    case "i32.popcnt":
                        outBytecode.Add((byte)Instruction.i32_popcnt);
                        break;

                    case "i32.add":
                        outBytecode.Add((byte)Instruction.i32_add);
                        break;

                    case "i32.sub":
                        outBytecode.Add((byte)Instruction.i32_sub);
                        break;

                    case "i32.mul":
                        outBytecode.Add((byte)Instruction.i32_mul);
                        break;

                    case "i32.div_s":
                        outBytecode.Add((byte)Instruction.i32_div_s);
                        break;

                    case "i32.div_u":
                        outBytecode.Add((byte)Instruction.i32_div_u);
                        break;

                    case "i32.rem_s":
                        outBytecode.Add((byte)Instruction.i32_rem_s);
                        break;

                    case "i32.rem_u":
                        outBytecode.Add((byte)Instruction.i32_rem_u);
                        break;

                    case "i32.and":
                        outBytecode.Add((byte)Instruction.i32_and);
                        break;

                    case "i32.or":
                        outBytecode.Add((byte)Instruction.i32_or);
                        break;

                    case "i32.xor":
                        outBytecode.Add((byte)Instruction.i32_xor);
                        break;

                    case "i32.shl":
                        outBytecode.Add((byte)Instruction.i32_shl);
                        break;

                    case "i32.shr_s":
                        outBytecode.Add((byte)Instruction.i32_shr_s);
                        break;

                    case "i32.shr_u":
                        outBytecode.Add((byte)Instruction.i32_shr_u);
                        break;

                    case "i32.rotl":
                        outBytecode.Add((byte)Instruction.i32_rotl);
                        break;

                    case "i32.rotr":
                        outBytecode.Add((byte)Instruction.i32_rotr);
                        break;

                    case "i64.clz":
                        outBytecode.Add((byte)Instruction.i64_clz);
                        break;

                    case "i64.ctz":
                        outBytecode.Add((byte)Instruction.i64_ctz);
                        break;

                    case"i64.popcnt":
                        outBytecode.Add((byte)Instruction.i64_popcnt);
                        break;

                    case"i64.add":
                        outBytecode.Add((byte)Instruction.i64_add);
                        break;

                    case"i64.sub":
                        outBytecode.Add((byte)Instruction.i64_sub);
                        break;

                    case"i64.mul":
                        outBytecode.Add((byte)Instruction.i64_mul);
                        break;

                    case"i64.div_s":
                        outBytecode.Add((byte)Instruction.i64_div_s);
                        break;

                    case"i64.div_u":
                        outBytecode.Add((byte)Instruction.i64_div_u);
                        break;

                    case"i64.rem_s":
                        outBytecode.Add((byte)Instruction.i64_rem_s);
                        break;

                    case"i64.rem_u":
                        outBytecode.Add((byte)Instruction.i64_rem_u);
                        break;

                    case"i64.and":
                        outBytecode.Add((byte)Instruction.i64_and);
                        break;

                    case"i64.or":
                        outBytecode.Add((byte)Instruction.i64_or);
                        break;

                    case"i64.xor":
                        outBytecode.Add((byte)Instruction.i64_xor);
                        break;

                    case"i64.shl":
                        outBytecode.Add((byte)Instruction.i64_shl);
                        break;

                    //case"i64.shr_s":
                    //    outBytecode.Add((byte)Instruction.i64_shr_s);
                    //    break;

                    case"i64.shr_u":
                        outBytecode.Add((byte)Instruction.i64_shr_u);
                        break;

                    case"i64.rotl":
                        outBytecode.Add((byte)Instruction.i64_rotl);
                        break;

                    case"i64.rotr":
                        outBytecode.Add((byte)Instruction.i64_rotr);
                        break;

                    case"f32.abs":
                        outBytecode.Add((byte)Instruction.f32_abs);
                        break;

                    case"f32.neg":
                        outBytecode.Add((byte)Instruction.f32_neg);
                        break;

                    case"f32.ceil":
                        outBytecode.Add((byte)Instruction.f32_ceil);
                        break;

                    case"f32.floor":
                        outBytecode.Add((byte)Instruction.f32_floor);
                        break;

                    case"f32.trunc":
                        outBytecode.Add((byte)Instruction.f32_trunc);
                        break;

                    case"f32.nearest":
                        outBytecode.Add((byte)Instruction.f32_nearest);
                        break;

                    case"f32.sqrt":
                        outBytecode.Add((byte)Instruction.f32_sqrt);
                        break;

                    case"f32.add":
                        outBytecode.Add((byte)Instruction.f32_add);
                        break;

                    case"f32.sub":
                        outBytecode.Add((byte)Instruction.f32_sub);
                        break;

                    case"f32.mul":
                        outBytecode.Add((byte)Instruction.f32_mul);
                        break;

                    case"f32.div":
                        outBytecode.Add((byte)Instruction.f32_div);
                        break;

                    case"f32.min":
                        outBytecode.Add((byte)Instruction.f32_min);
                        break;

                    case"f32.max":
                        outBytecode.Add((byte)Instruction.f32_max);
                        break;

                    case"f32.copysign":
                        outBytecode.Add((byte)Instruction.f32_copysign);
                        break;

                    case"f64.abs":
                        outBytecode.Add((byte)Instruction.f64_abs);
                        break;

                    case"f64.neg":
                        outBytecode.Add((byte)Instruction.f64_neg);
                        break;

                    case"f64.ceil":
                        outBytecode.Add((byte)Instruction.f64_ceil);
                        break;

                    case"f64.floor":
                        outBytecode.Add((byte)Instruction.f64_floor);
                        break;

                    case"f64.trunc":
                        outBytecode.Add((byte)Instruction.f64_trunc);
                        break;

                    case"f64.nearest":
                        outBytecode.Add((byte)Instruction.f64_nearest);
                        break;

                    case"f64.sqrt":
                        outBytecode.Add((byte)Instruction.f64_sqrt);
                        break;

                    case"f64.add":
                        outBytecode.Add((byte)Instruction.f64_add);
                        break;

                    case"f64.sub":
                        outBytecode.Add((byte)Instruction.f64_sub);
                        break;

                    case"f64.mul":
                        outBytecode.Add((byte)Instruction.f64_mul);
                        break;

                    case"f64.div":
                        outBytecode.Add((byte)Instruction.f64_div);
                        break;

                    case"f64.min":
                        outBytecode.Add((byte)Instruction.f64_min);
                        break;

                    case"f64.max":
                        outBytecode.Add((byte)Instruction.f64_max);
                        break;

                    case"f64.copysign":
                        outBytecode.Add((byte)Instruction.f64_copysign);
                        break;

                    case"i32.wrap_i64":
                        outBytecode.Add((byte)Instruction.i32_wrap_i64);
                        break;

                    case "i32.trunc_f32_s":
                        outBytecode.Add((byte)Instruction.i32_trunc_f32_s);
                        break;

                    case"i32.trunc_f32_u":
                        outBytecode.Add((byte)Instruction.i32_trunc_f32_u);
                        break;

                    case"i32.trunc_f64_s":
                        outBytecode.Add((byte)Instruction.i32_trunc_f64_s);
                        break;

                    case"i32.trunc_f64_u":
                        outBytecode.Add((byte)Instruction.i32_trunc_f64_u);
                        break;

                    case"i64.extend_i32_s":
                        outBytecode.Add((byte)Instruction.i64_extend_i32_s);
                        break;

                    case"i64.extend_i32_u":
                        outBytecode.Add((byte)Instruction.i64_extend_i32_u);
                        break;

                    case"i64.trunc_f32_s":
                        outBytecode.Add((byte)Instruction.i64_trunc_f32_s);
                        break;

                    case"i64.trunc_f32_u":
                        outBytecode.Add((byte)Instruction.i64_trunc_f32_u);
                        break;

                    case"i64.trunc_f64_s":
                        outBytecode.Add((byte)Instruction.i64_trunc_f64_s);
                        break;

                    case"i64.trunc_f64_u":
                        outBytecode.Add((byte)Instruction.i64_trunc_f64_u);
                        break;

                    case"f32.convert_i32_s":
                        outBytecode.Add((byte)Instruction.f32_convert_i32_s);
                        break;

                    case"f32.convert_i32_u":
                        outBytecode.Add((byte)Instruction.f32_convert_i32_u);
                        break;

                    case"f32.convert_i64_s":
                        outBytecode.Add((byte)Instruction.f32_convert_i64_s);
                        break;

                    case"f32.convert_i64_u":
                        outBytecode.Add((byte)Instruction.f32_convert_i64_u);
                        break;

                    //case"f32.demote_f64":
                    //    outBytecode.Add((byte)Instruction.f32_demote_f64);
                    //    break;

                    case"f64.convert_i32_s":
                        outBytecode.Add((byte)Instruction.f64_convert_i32_s);
                        break;

                    case"f64.convert_i32_u":
                        outBytecode.Add((byte)Instruction.f64_convert_i32_u);
                        break;

                    case"f64.convert_i64_s":
                        outBytecode.Add((byte)Instruction.f64_convert_i64_s);
                        break;

                    case"f64.convert_i64_u":
                        outBytecode.Add((byte)Instruction.f64_convert_i64_u);
                        break;

                    case"f64.promote_f32":
                        outBytecode.Add((byte)Instruction.f64_promote_f32);
                        break;

                    case"i32.reinterpret_f32":
                        outBytecode.Add((byte)Instruction.i32_reinterpret_f32);
                        break;

                    case"i64.reinterpret_f64":
                        outBytecode.Add((byte)Instruction.i64_reinterpret_f64);
                        break;

                    case"f32.reinterpret_i32":
                        outBytecode.Add((byte)Instruction.f32_reinterpret_i32);
                        break;

                    case"f64.reinterpret_i64":
                        outBytecode.Add((byte)Instruction.f64_reinterpret_i64);
                        break;

                    case"i32.extend8_s":
                        outBytecode.Add((byte)Instruction.i32_extend8_s);
                        break;

                    case"i32.extend16_s":
                        outBytecode.Add((byte)Instruction.i32_extend16_s);
                        break;

                    case"i64.extend8_s":
                        outBytecode.Add((byte)Instruction.i32_extend8_s);
                        break;

                    case"i64.extend16_s":
                        outBytecode.Add((byte)Instruction.i64_extend16_s);
                        break;

                    case"i64.extend32_s":
                        outBytecode.Add((byte)Instruction.i64_extend32_s);
                        break;

                    case "i32.trunc_sat_f32_s":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(0);
                        break;

                    case "i32.trunc_sat_f32_u":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(1);
                        break;

                    case "i32.trunc_sat_f64_s":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(2);
                        break;

                    case "i32.trunc_sat_f64_u":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(3);
                        break;

                    case "i64.trunc_sat_f32_s":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(4);
                        break;

                    case "i64.trunc_sat_f32_u":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(5);
                        break;

                    case "i64.trunc_sat_f64_s":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(6);
                        break;

                    case "i64.trunc_sat_f64_u":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(7);
                        break;

                    case "memory.fill":
                        outBytecode.Add((byte)Instruction.trunc_sat);
                        outBytecode.Add(0x0B);
                        outBytecode.Add(0x00);
                        break;

                    default:
                        throw new System.Exception(); // TODO: Error message
                }
            }
        }
    }
}