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
    public class Function
    {
        /// <summary>
        /// The various values used to track and validate execution, matching
        /// the unique values in the WASM spec - for the reference algorithm in
        /// the appendix.
        /// 
        /// See type opd_stack in https://webassembly.github.io/spec/core/appendix/algorithm.html
        /// for more details.
        /// </summary>
        public enum Opd_Stack
        { 
            /// <summary>
            /// The type is a 32 bit integer.
            /// </summary>
            i32,

            /// <summary>
            /// The type is a 64 bit integer.
            /// </summary>
            i64,

            /// <summary>
            /// The type is a 32 bit (single precision) float.
            /// </summary>
            f32,

            /// <summary>
            /// The type is a 64 bit (double precision) float.
            /// </summary>
            f64,

            /// <summary>
            /// The type is unknown.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// The function signature. This will point to the type in the parentModule's 
        /// types list.
        /// </summary>
        /// <remarks>The dereferenced will be cached in fnType.</remarks>
        public uint typeidx;

        /// <summary>
        /// The cached FunctionType, which is the result to retrieving the typeidx from 
        /// types list in the parentModule.
        /// </summary>
        public FunctionType fnType;

        /// <summary>
        /// The Module that this function belongs to.
        /// </summary>
        public readonly Module parentModule;


        /// <summary>
        /// A listing of the types for the function's local working space
        /// on the stack. These will appear on the stack after the parameters.
        /// </summary>
        public List<FunctionType.DataOrgInfo> localTypes = 
            new List<FunctionType.DataOrgInfo>();

        /// <summary>
        /// A cached counter of how much stack memory should be allocated for 
        /// the program. This includes both memory for initializing the function
        /// parameters, as well as the local variables.
        /// </summary>
        public uint totalStackSize;

        /// <summary>
        /// The bytecode for the function.
        /// 
        /// When first set, it will be the binary from the WASM file. The execution engine 
        /// needs to process it and change the binary because it can be run as a PrecisionWASM
        /// program (it transpiles it into a slightly different binary). This is done by calling
        /// ExpandExpressionToBeUsable() once after it's been loaded. Afterwards it will contain
        /// the executable processed bytecode.
        /// </summary>
        public byte [] expression;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentModule">The parent Module.</param>
        public Function(Module parentModule)
        {
            this.parentModule = parentModule;
        }

        /// <summary>
        /// This function will be called once when loading and processing the function
        /// bytecode to correctly set values and cache totalStackSize after all the 
        /// basic stack data has been loaded.
        /// </summary>
        public void InitializeOrganization()
        {
            // This function requires the FunctionType fnType to already be organized with
            // its InitializeOrganization().

            this.totalStackSize = this.fnType.totalParamSize;

            for(int i = 0; i < this.localTypes.Count; ++i)
            {
                FunctionType.DataOrgInfo doi = this.localTypes[i];
                FunctionType.FillInOrg(ref doi, ref this.totalStackSize);
                this.localTypes[i] = doi;
            }
        }

        public static Opd_Stack ConvertToStackType(Bin.TypeID tyid)
        { 
            switch(tyid)
            {
                case Bin.TypeID.Float32:
                    return Opd_Stack.f32;

                case Bin.TypeID.Float64:
                    return Opd_Stack.f64;

                case Bin.TypeID.Int32:
                    return Opd_Stack.i32;

                case Bin.TypeID.Int64:
                    return Opd_Stack.i64;
            }

            return Opd_Stack.Unknown;
        }

        public class CtrlFrame
        { 
            public Instruction opcode;
            public List<Opd_Stack> startTypes;
            public List<Opd_Stack> endTypes;
            public int height;
            public bool unreachable;

            public List<uint> writePopped;

            public void QueueEndWrite(List<byte> expanded)
            { 
                if(this.writePopped == null)
                    this.writePopped = new List<uint>();

                this.writePopped.Add((uint)expanded.Count);

                // Add an int into expanded
                expanded.Add(0);
                expanded.Add(0);
                expanded.Add(0);
                expanded.Add(0);
            }

            public void QueueEndWrite(uint loc)
            {
                if (this.writePopped == null)
                    this.writePopped = new List<uint>();

                this.writePopped.Add(loc);
            }

            unsafe public void FlushEndWrites(List<byte> expanded)
            { 
                if(this.writePopped == null)
                    return;

                uint idx = (uint)expanded.Count;

                // Write the current end position (idx) in every position queued
                // to have it written to.
                byte [] rb = System.BitConverter.GetBytes(idx);
                foreach(uint u in writePopped)
                {
                    expanded[(int)u + 0] = rb[0];
                    expanded[(int)u + 1] = rb[1];
                    expanded[(int)u + 2] = rb[2];
                    expanded[(int)u + 3] = rb[3];
                }
            }
        }

        public class ValidatorUtil
        {
            public List<Opd_Stack> opds = new List<Opd_Stack>();
            public Stack<CtrlFrame> ctrls = new Stack<CtrlFrame>();

            public void PushOpd(Opd_Stack os)
            { 
                this.opds.Add(os);
            }

            public Opd_Stack PopOpd()
            { 
                if(this.opds.Count == this.ctrls.Peek().height && this.ctrls.Peek().unreachable == true)
                    return Opd_Stack.Unknown;

                if(this.opds.Count == this.ctrls.Peek().height)
                    this.EmitValidationError("Operation stack mismatch.");

                Opd_Stack ret = this.opds[this.opds.Count - 1];
                this.opds.RemoveAt(this.opds.Count - 1);
                return  ret;
            }

            public Opd_Stack PopOpd(Opd_Stack expect)
            { 
                Opd_Stack actual = this.PopOpd();
                if(actual == Opd_Stack.Unknown)
                    return expect;

                if(expect == Opd_Stack.Unknown)
                    return actual;

                if(actual != expect)
                    this.EmitValidationError($"Incorrect type {actual} when expecting {expect}.");

                return actual;
            }

            public void PushOpds(List<Opd_Stack> types)
            { 
                this.opds.AddRange(types);
            }

            public void PopOpds(List<Opd_Stack> types)
            { 
                for(int i = types.Count - 1; i >= 0; --i)
                    this.PopOpd(types[i]);
            }

            public CtrlFrame PushCtrl(Instruction opcode, List<Opd_Stack> instk, List<Opd_Stack> outstk)
            { 
                CtrlFrame frame = new CtrlFrame();
                frame.opcode        = opcode;
                frame.startTypes    = instk;
                frame.endTypes      = outstk;
                frame.unreachable   = false;
                frame.height        = this.opds.Count;
                this.ctrls.Push(frame);
                this.PushOpds(instk);

                return frame;
            }

            unsafe public CtrlFrame PopCtrl(List<byte> expanded)
            { 
                if(this.ctrls.Count == 0)
                { } // TODO: Error

                CtrlFrame frame = this.ctrls.Peek();
                this.PopOpds(frame.endTypes);

                if(this.opds.Count != frame.height)
                { } // TODO: Error

                this.ctrls.Pop();
                frame.FlushEndWrites(expanded);
                return frame;
            }

            public CtrlFrame GetCtrl(int fromTop)
            { 
                foreach(CtrlFrame cf in this.ctrls)
                {
                    if(fromTop == 0)
                        return cf;

                    --fromTop;
                }
                return null;
            }

            public List<Opd_Stack> LabelTypes(CtrlFrame frame)
            { 
                if(frame.opcode == Instruction.loop)
                    return frame.startTypes;
                else
                    return frame.endTypes;
            }

            public void Unreachable()
            { 
                int height = this.ctrls.Peek().height;

                // This popping in batch isn't as efficient as it could be
                while(this.opds.Count > height) 
                    this.opds.RemoveAt(this.opds.Count -1);

                this.ctrls.Peek().unreachable = true;
            }

            public void EmitValidationError(string str)
            { 
                throw new System.Exception(str);
            }
        }

        unsafe public static void ConsumeTypes(byte * pb, ref uint idx, List<Opd_Stack> stk)
        {
            bool c = true;
            while (c)
            {
                switch (pb[idx])
                {
                    case (int)Bin.TypeID.Int32:
                        stk.Add(Opd_Stack.i32);
                        ++idx;
                        break;

                    case (int)Bin.TypeID.Int64:
                        stk.Add(Opd_Stack.i64);
                        ++idx;
                        break;

                    case (int)Bin.TypeID.Float32:
                        stk.Add(Opd_Stack.f32);
                        ++idx;
                        break;

                    case (int)Bin.TypeID.Float64:
                        stk.Add(Opd_Stack.f64);
                        ++idx;
                        break;

                    default:
                        c = false;
                        break;
                }
            }
        }

        unsafe public static void TransferInstruction(List<byte> rb, Instruction instr)
        { 
            rb.AddRange( System.BitConverter.GetBytes((short)instr));
        }

        unsafe public static void TransferFloat32(List<byte> rb, float f)
        {
            rb.AddRange(System.BitConverter.GetBytes(f));
        }

        unsafe public static void TransferFloat64(List<byte> rb, double d)
        {
            rb.AddRange(System.BitConverter.GetBytes(d));
        }

        unsafe public static void TransferInt32s(List<byte> rb, int i)
        {
            rb.AddRange(System.BitConverter.GetBytes(i));
        }

        unsafe public static void TransferInt32u(List<byte> rb, uint i)
        {
            rb.AddRange(System.BitConverter.GetBytes(i));
        }

        unsafe public static void TransferInt64s(List<byte> rb, long i)
        {
            rb.AddRange(System.BitConverter.GetBytes(i));
        }

        unsafe public static void TransferInt64u(List<byte> rb, ulong i)
        {
            rb.AddRange(System.BitConverter.GetBytes(i));
        }

        // Given an encoding, convert it to be usable. The biggest impeding factor
        // is the LEB decoding required if we didn't do this. And since that changes
        // alignment, indices may also need to be changed.
        unsafe public void ExpandExpressionToBeUsable(Module session, int index)
        { 
            List<byte> expanded = new List<byte>();

            ValidatorUtil vu = new ValidatorUtil();
            // The algorithm in the appendix of the spec didn't say how vu should be initialized,
            // but an initial ctrl is required on the stack.
            // (wleu 02/18/2021)
            List<Opd_Stack> startFrameCtrl = new List<Opd_Stack>();
            foreach(FunctionType.DataOrgInfo doi in this.fnType.resultTypes)
            {
                startFrameCtrl.Add(ConvertToStackType((Bin.TypeID)doi.type));
            }
            vu.PushCtrl(Instruction.nop, new List<Opd_Stack>(), startFrameCtrl);

            FunctionType ft = session.types[index];

            fixed (byte * pb = this.expression)
            {
                uint idx = 0;
                while(idx < this.expression.Length)
                { 
                    Instruction instr = (Instruction)this.expression[idx];
                    ++idx;

                    switch(instr)
                    {
                        case Instruction.unreachable:
                            vu.Unreachable();
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.nop:
                            break;

                        case Instruction.block:
                            {
                                List<Opd_Stack> instk = new List<Opd_Stack>();
                                List<Opd_Stack> outstk = new List<Opd_Stack>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vu.PopOpds(instk);
                                vu.PushCtrl(Instruction.block, instk, outstk);
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.loop:
                            {
                                List<Opd_Stack> instk = new List<Opd_Stack>();
                                List<Opd_Stack> outstk = new List<Opd_Stack>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vu.PopOpds(instk);
                                vu.PushCtrl(Instruction.loop, instk, outstk);
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.ifblock:
                            {
                                List<Opd_Stack> instk = new List<Opd_Stack>();
                                List<Opd_Stack> outstk = new List<Opd_Stack>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vu.PopOpd(Opd_Stack.i32);
                                vu.PopOpds(instk);
                                TransferInstruction(expanded, instr);

                                vu.PushCtrl(Instruction.ifblock, instk, outstk).
                                    QueueEndWrite(expanded);
                            }
                            break;

                        case Instruction.elseblock:
                            {
                                // Add a jump so the if statement jumps over the else portion.
                                TransferInstruction(expanded, Instruction._goto);
                                uint jumpLoc = (uint)expanded.Count;
                                expanded.Add(0);
                                expanded.Add(0);
                                expanded.Add(0);
                                expanded.Add(0);

                                CtrlFrame frame = vu.PopCtrl(expanded);
                                if(frame.opcode != Instruction.ifblock)
                                    vu.EmitValidationError("Illegal else block that did not follow if statement.");

                                
                                vu.PushCtrl( Instruction.elseblock, frame.startTypes, frame.endTypes).
                                    QueueEndWrite(jumpLoc);
                            }
                            break;

                        case Instruction.end:
                            CtrlFrame endf = vu.PopCtrl(expanded);
                            vu.PushOpds(endf.endTypes);
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.br:
                            {
                                int n = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                if (vu.ctrls.Count < n)
                                    vu.EmitValidationError("Stack mismatch for br");

                                vu.PopOpds( vu.LabelTypes(vu.GetCtrl(n)));
                                vu.Unreachable();
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.br_if:
                            {
                                int n = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                if(vu.ctrls.Count < n)
                                    vu.EmitValidationError("Stack mismatch for br_if");

                                vu.PopOpd(Opd_Stack.i32);
                                vu.PopOpds(vu.LabelTypes(vu.GetCtrl(n)));
                                vu.PushOpds(vu.LabelTypes(vu.GetCtrl(n)));
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.br_table:
                            { 
                                int n = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                int m = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);

                                if(vu.ctrls.Count < m)
                                    vu.EmitValidationError("");

                                for(int i = 0; i < n; ++i)
                                { 
                                    if(vu.ctrls.Count < i || vu.LabelTypes(vu.GetCtrl(i)) != vu.LabelTypes(vu.GetCtrl(m)))
                                        vu.EmitValidationError("");
                                }
                                vu.PopOpd( Opd_Stack.i32);
                                vu.PopOpds( vu.LabelTypes(vu.GetCtrl(m)));
                                vu.Unreachable();

                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.returnblock:
                            { } // TODO: Figure out later
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.call:
                            {
                                uint fnidx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionIndexEntry fie = parentModule.functionIndexing[(int)fnidx];
                                if(fie.type == FunctionIndexEntry.FnIdxType.Local)
                                { 
                                    TransferInstruction(expanded, Instruction._call_local);
                                    TransferInt32u(expanded, (uint)fie.index);
                                }
                                else
                                {
                                    TransferInstruction(expanded, Instruction._call_import);
                                    TransferInt32u(expanded, (uint)fie.index);
                                }
                            }
                            break;

                        case Instruction.call_indirect:
                            vu.PopOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.drop:
                            vu.PopOpd();
                            TransferInstruction(expanded, instr);
                            break;
                        
                        case Instruction.select:
                            vu.PopOpd(Opd_Stack.i32);
                            Opd_Stack selos1 = vu.PopOpd();
                            Opd_Stack selos2 = vu.PopOpd(selos1);
                            vu.PushOpd(selos2);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.local_get:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vu.PushOpd(ConvertToStackType(ty.type));
                                if(ty.size == 4)
                                {
                                    TransferInstruction(expanded, Instruction._local_get32);
                                    TransferInt32u(expanded, ty.offset);
                                }
                                else if(ty.size == 8)
                                {
                                    TransferInstruction(expanded, Instruction._local_get64);
                                    TransferInt32u(expanded, ty.offset);
                                }
                                else
                                    vu.EmitValidationError("Retrieving parameter of illegal size.");

                            

                            }
                            break;
                        case Instruction.local_set:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vu.PopOpd(ConvertToStackType(ty.type));
                                if (ty.size == 4)
                                    TransferInstruction(expanded, Instruction._local_set32);
                                else if (ty.size == 8)
                                    TransferInstruction(expanded, Instruction._local_set64);
                                else
                                    vu.EmitValidationError("Setting parameter of illegal size.");

                                TransferInt32u(expanded, ty.offset);

                            }
                            break;
                        case Instruction.local_tee:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vu.PopOpd(ConvertToStackType(ty.type));
                                vu.PushOpd(ConvertToStackType(ty.type));
                                if (ty.size == 4)
                                    TransferInstruction(expanded, Instruction._local_tee32);
                                else if (ty.size == 8)
                                    TransferInstruction(expanded, Instruction._local_tee64);
                                else
                                    vu.EmitValidationError("Setting parameter of illegal size.");

                                TransferInt32u(expanded, ty.offset);
                            }
                            break;
                        case Instruction.global_get:
                            {
                                // This function is incorrect in that it's a duplicate of local_get.
                                // this eventually needs to pull from the global varable source.

                                uint globalIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                Bin.TypeID type = parentModule.globals[(int)globalIdx].global.type;

                                vu.PushOpd(ConvertToStackType(type));

                                int typeSize = Memory.GetTypeIDSize(type);
                                if (typeSize == 4)
                                    TransferInstruction(expanded, Instruction._global_get32);
                                else if (typeSize == 8)
                                    TransferInstruction(expanded, Instruction._global_get64);
                                else
                                    vu.EmitValidationError("Getting global value of illegal size.");

                                TransferInt32u(expanded, globalIdx);
                            }
                            break;
                        case Instruction.global_set:
                            {
                                // This function is incorrect in that it's a duplicate of local_set.
                                // this eventually needs to pull from the global varable source.

                                uint globalIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                Bin.TypeID type = parentModule.globals[(int)globalIdx].global.type;

                                vu.PopOpd(ConvertToStackType(session.globals[(int)globalIdx].global.type));

                                int typeSize = Memory.GetTypeIDSize(type);
                                if (typeSize == 4)
                                    TransferInstruction(expanded, Instruction._global_set32);
                                else if (typeSize == 8)
                                    TransferInstruction(expanded, Instruction._global_set64);
                                else
                                    vu.EmitValidationError("Setting global value of illegal size.");

                                TransferInt32u(expanded, globalIdx);

                            }
                        
                            break;

                        case Instruction.i32_load:
                            vu.PopOpd( Opd_Stack.i32 );
                            vu.PushOpd(Opd_Stack.i32 );
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_load:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_load:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_load:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_load8_s:
                        case Instruction.i32_load8_u:
                        case Instruction.i32_load16_s:
                        case Instruction.i32_load16_u:
                            {
                                uint val = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                vu.PushOpd(Opd_Stack.i32);
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.i64_load8_s:
                        case Instruction.i64_load8_u:
                        case Instruction.i64_load16_s:
                        case Instruction.i64_load16_u:
                        case Instruction.i64_load32_s:
                        case Instruction.i64_load32_u:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PushOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_store:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PopOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_store:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PopOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_store:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PopOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_store:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PopOpd(Opd_Stack.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_store8:
                        case Instruction.i32_store16:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PopOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_store8:
                        case Instruction.i64_store16:
                        case Instruction.i64_store32:
                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vu.PopOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.MemorySize:
                        case Instruction.MemoryGrow:
                            break;

                        case Instruction.i32_const:
                            {
                                vu.PushOpd(Opd_Stack.i32);
                                TransferInstruction(expanded, instr);

                                uint cval = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                TransferInt32u(expanded, cval);

                            }
                            break;

                        case Instruction.i64_const:
                            {
                                vu.PushOpd(Opd_Stack.i64);
                                TransferInstruction(expanded, instr);

                                ulong cval = BinParse.LoadUnsignedLEB64(pb, ref idx);
                                TransferInt64u(expanded, cval);
                            }
                            break;

                        case Instruction.f32_const:
                            {
                                vu.PushOpd(Opd_Stack.f32);
                                TransferInstruction(expanded, instr);

                                TransferInt32u(expanded, *(uint*)&pb[idx]);
                                idx += 4;

                            }
                            break;

                        case Instruction.f64_const:
                            {
                                vu.PushOpd(Opd_Stack.f64);
                                TransferInstruction(expanded, instr);

                                TransferInt64u(expanded, *(ulong*)&pb[idx]);
                                idx += 8;

                            }
                            break;

                        case Instruction.i32_eqz:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_eq:
                        case Instruction.i32_ne:
                        case Instruction.i32_lt_s:
                        case Instruction.i32_lt_u:
                        case Instruction.i32_gt_s:
                        case Instruction.i32_gt_u:
                        case Instruction.i32_le_s:
                        case Instruction.i32_le_u:
                        case Instruction.i32_ge_s:
                        case Instruction.i32_ge_u:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_eqz:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_eq:
                        case Instruction.i64_ne:
                        case Instruction.i64_lt_s:
                        case Instruction.i64_lt_u:
                        case Instruction.i64_gt_s:
                        case Instruction.i64_gt_u:
                        case Instruction.i64_le_s:
                        case Instruction.i64_le_u:
                        case Instruction.i64_ge_s:
                        case Instruction.i64_ge_u:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_eq:
                        case Instruction.f32_ne:
                        case Instruction.f32_lt:
                        case Instruction.f32_gt:
                        case Instruction.f32_le:
                        case Instruction.f32_ge:
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_eq:
                        case Instruction.f64_ne:
                        case Instruction.f64_lt:
                        case Instruction.f64_gt:
                        case Instruction.f64_le:
                        case Instruction.f64_ge:
                            vu.PopOpd(Opd_Stack.f64);
                            vu.PopOpd(Opd_Stack.f64);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_clz:
                        case Instruction.i32_ctz:
                        case Instruction.i32_popcnt:
                            Opd_Stack unopty = vu.PopOpd( Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_add:
                        case Instruction.i32_sub:
                        case Instruction.i32_mul:
                        case Instruction.i32_div_s:
                        case Instruction.i32_div_u:
                        case Instruction.i32_rem_s:
                        case Instruction.i32_rem_u:
                        case Instruction.i32_and:
                        case Instruction.i32_or:
                        case Instruction.i32_xor:
                        case Instruction.i32_shl:
                        case Instruction.i32_shr_s:
                        case Instruction.i32_shr_u:
                        case Instruction.i32_rotl:
                        case Instruction.i32_rotr:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_clz:
                        case Instruction.i64_ctz:
                        case Instruction.i64_popcnt:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_add:
                        case Instruction.i64_sub:
                        case Instruction.i64_mul:
                        case Instruction.i64_div_s:
                        case Instruction.i64_div_u:
                        case Instruction.i64_rem_s:
                        case Instruction.i64_rem_u:
                        case Instruction.i64_and:
                        case Instruction.i64_or:
                        case Instruction.i64_xor:
                        case Instruction.i64_shl:
                        case Instruction.i64_shr_s:
                        case Instruction.i64_shr_u:
                        case Instruction.i64_rotl:
                        case Instruction.i64_rotr:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_abs:
                        case Instruction.f32_neg:
                        case Instruction.f32_ceil:
                        case Instruction.f32_floor:
                        case Instruction.f32_trunc:
                        case Instruction.f32_nearest:
                        case Instruction.f32_sqrt:
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_add:
                        case Instruction.f32_sub:
                        case Instruction.f32_mul:
                        case Instruction.f32_div:
                        case Instruction.f32_min:
                        case Instruction.f32_max:
                        case Instruction.f32_copysign:
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_abs:
                        case Instruction.f64_neg:
                        case Instruction.f64_ceil:
                        case Instruction.f64_floor:
                        case Instruction.f64_trunc:
                        case Instruction.f64_nearest:
                        case Instruction.f64_sqrt:
                            vu.PopOpd(Opd_Stack.f64);
                            vu.PushOpd(Opd_Stack.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_add:
                        case Instruction.f64_sub:
                        case Instruction.f64_mul:
                        case Instruction.f64_div:
                        case Instruction.f64_min:
                        case Instruction.f64_max:
                        case Instruction.f64_copysign:
                            vu.PopOpd( Opd_Stack.f64 );
                            vu.PopOpd( Opd_Stack.f64 );
                            vu.PushOpd(Opd_Stack.f64 );
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_wrap_i64:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_trunc_f32_s:
                        case Instruction.i32_trunc_f32_u:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_trunc_f64_s:
                        case Instruction.i32_trunc_f64_u:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_extend_i32_s:
                        case Instruction.i64_extend_i32_u:
                        case Instruction.i64_trunc_f32_s:
                        case Instruction.i64_trunc_f32_u:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_trunc_f64_s:
                        case Instruction.i64_trunc_f64_u:
                            vu.PopOpd(Opd_Stack.f64);
                            vu.PushOpd(Opd_Stack.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_convert_i32_s:
                        case Instruction.f32_convert_i32_u:
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_convert_i64_s:
                        case Instruction.f32_convert_i64_u:
                        case Instruction.f32_convert_f64:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_convert_i32_s:
                        case Instruction.f64_convert_i32_u:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_convert_i64_s:
                        case Instruction.f64_convert_i64_u:
                            vu.PopOpd(Opd_Stack.i64);
                            vu.PushOpd(Opd_Stack.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_promote_f32:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_reinterpret_f32:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.f32);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_reinterpret_f64:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i64);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_reinterpret_i32:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i32);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_reinterpret_i64:
                            vu.PopOpd(Opd_Stack.i32);
                            vu.PushOpd(Opd_Stack.i64);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_extend8_s:
                        case Instruction.i32_extend16_s:
                            vu.PopOpd(Opd_Stack.f32);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_extend8_s:
                        case Instruction.i64_extend16_s:
                        case Instruction.i64_extend32_s:
                            vu.PopOpd(Opd_Stack.f64);
                            vu.PushOpd(Opd_Stack.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.trunc_sat:
                            { 
                                int subop = BinParse.LoadSignedLEB32(pb, ref idx);
                                switch(subop)
                                { 
                                    case 0:
                                        vu.PopOpd(Opd_Stack.f32);
                                        vu.PushOpd(Opd_Stack.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f32_s);
                                        break;
                                    case 1:
                                        vu.PopOpd(Opd_Stack.f32);
                                        vu.PushOpd(Opd_Stack.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f32_u);
                                        break;
                                    case 2:
                                        vu.PopOpd(Opd_Stack.f64);
                                        vu.PushOpd(Opd_Stack.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f64_s);
                                        break;
                                    case 3:
                                        vu.PopOpd(Opd_Stack.f64);
                                        vu.PushOpd(Opd_Stack.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f64_u);
                                        break;
                                    case 4:
                                        vu.PopOpd(Opd_Stack.f32);
                                        vu.PushOpd(Opd_Stack.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f32_s);
                                        break;
                                    case 5:
                                        vu.PopOpd(Opd_Stack.f32);
                                        vu.PushOpd(Opd_Stack.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f32_u);
                                        break;
                                    case 6:
                                        vu.PopOpd(Opd_Stack.f64);
                                        vu.PushOpd(Opd_Stack.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f64_s);
                                        break;
                                    case 7:
                                        vu.PopOpd(Opd_Stack.f64);
                                        vu.PushOpd(Opd_Stack.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f64_u);
                                        break;
                                    case 0xB:
                                        vu.PopOpd(Opd_Stack.i32);
                                        vu.PopOpd(Opd_Stack.i32);
                                        vu.PopOpd(Opd_Stack.i32);
                                        TransferInstruction(expanded, Instruction._memory_fill);
                                        BinParse.LoadSignedLEB32(pb, ref idx); // Eat unused placeholder number
                                        break;
                                }
                            }
                            break;
                    }
                }

                while(vu.ctrls.Count > 0)
                    vu.PopCtrl(expanded);

                if(this.fnType.totalResultSize > 0)
                {
                    TransferInstruction(expanded, Instruction._stackbackwrite);
                    TransferInt32u(expanded, this.totalStackSize);   // How much to move the stack by to overwrite the parameters
                    TransferInt32u(expanded, this.fnType.totalResultSize);  // How much bytes in the results payload that need to be transfered
                }

                TransferInstruction(expanded, Instruction.returnblock);

                this.expression = expanded.ToArray();
            }
        }

        FunctionType.DataOrgInfo GetStackDataInfo(uint uidx)
        {
            if (uidx < this.fnType.paramTypes.Count)
                return this.fnType.paramTypes[(int)uidx];
            else
                return this.localTypes[(int)uidx - fnType.paramTypes.Count];
        }
    }
}