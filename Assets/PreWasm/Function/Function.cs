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
using PxPre.WASM.Vali;

namespace PxPre.WASM
{
    public class Function
    {
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


        unsafe public static void ConsumeTypes(byte * pb, ref uint idx, List<StackOpd> stk)
        {
            bool c = true;
            while (c)
            {
                switch (pb[idx])
                {
                    case (int)Bin.TypeID.Int32:
                        stk.Add(StackOpd.i32);
                        ++idx;
                        break;

                    case (int)Bin.TypeID.Int64:
                        stk.Add(StackOpd.i64);
                        ++idx;
                        break;

                    case (int)Bin.TypeID.Float32:
                        stk.Add(StackOpd.f32);
                        ++idx;
                        break;

                    case (int)Bin.TypeID.Float64:
                        stk.Add(StackOpd.f64);
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

            DataStoreIdx memoryStore = new DataStoreIdx();
            DataStoreIdx globalStore = new DataStoreIdx();
            DataStoreIdx tableStore = new DataStoreIdx();

            ValiMgr vmgr = new ValiMgr();
            // The algorithm in the appendix of the spec didn't say how vu should be initialized,
            // but an initial ctrl is required on the stack.
            // (wleu 02/18/2021)
            List<StackOpd> startFrameCtrl = new List<StackOpd>();
            foreach(FunctionType.DataOrgInfo doi in this.fnType.resultTypes)
            {
                startFrameCtrl.Add(ValiMgr.ConvertToStackType((Bin.TypeID)doi.type));
            }
            vmgr.PushCtrl(
                Instruction.nop, 
                new List<StackOpd>(), 
                startFrameCtrl,
                memoryStore,
                globalStore,
                tableStore);

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
                            vmgr.Unreachable();
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.nop:
                            break;

                        case Instruction.block:
                            {
                                List<StackOpd> instk = new List<StackOpd>();
                                List<StackOpd> outstk = new List<StackOpd>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vmgr.PopOpds(instk);
                                vmgr.PushCtrl(
                                    Instruction.block, 
                                    instk, 
                                    outstk,
                                    memoryStore,
                                    globalStore,
                                    tableStore);
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.loop:
                            {
                                List<StackOpd> instk = new List<StackOpd>();
                                List<StackOpd> outstk = new List<StackOpd>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vmgr.PopOpds(instk);
                                vmgr.PushCtrl(
                                    Instruction.loop, 
                                    instk, 
                                    outstk,
                                    memoryStore,
                                    globalStore,
                                    tableStore);
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.ifblock:
                            {
                                List<StackOpd> instk = new List<StackOpd>();
                                List<StackOpd> outstk = new List<StackOpd>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vmgr.PopOpd(StackOpd.i32);
                                vmgr.PopOpds(instk);
                                TransferInstruction(expanded, instr);

                                CtrlFrame ctrlFrame = 
                                    vmgr.PushCtrl(
                                        Instruction.ifblock, 
                                        instk, 
                                        outstk,
                                        memoryStore,
                                        globalStore,
                                        tableStore);

                                ctrlFrame.QueueEndWrite(expanded);
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

                                CtrlFrame frame = vmgr.PopCtrl(expanded);
                                if(frame.opcode != Instruction.ifblock)
                                    vmgr.EmitValidationError("Illegal else block that did not follow if statement.");

                                
                                CtrlFrame ctrlFrame = 
                                    vmgr.PushCtrl( 
                                        Instruction.elseblock, 
                                        frame.startTypes, 
                                        frame.endTypes,
                                        memoryStore,
                                        globalStore,
                                        tableStore);

                                ctrlFrame.QueueEndWrite(jumpLoc);
                            }
                            break;

                        case Instruction.end:
                            CtrlFrame endf = vmgr.PopCtrl(expanded);
                            vmgr.PushOpds(endf.endTypes);
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.br:
                            {
                                int n = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                if (vmgr.ctrls.Count < n)
                                    vmgr.EmitValidationError("Stack mismatch for br");

                                vmgr.PopOpds( vmgr.LabelTypes(vmgr.GetCtrl(n)));
                                vmgr.Unreachable();
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.br_if:
                            {
                                int n = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                if(vmgr.ctrls.Count < n)
                                    vmgr.EmitValidationError("Stack mismatch for br_if");

                                vmgr.PopOpd(StackOpd.i32);
                                vmgr.PopOpds(vmgr.LabelTypes(vmgr.GetCtrl(n)));
                                vmgr.PushOpds(vmgr.LabelTypes(vmgr.GetCtrl(n)));
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.br_table:
                            { 
                                int n = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                int m = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);

                                if(vmgr.ctrls.Count < m)
                                    vmgr.EmitValidationError("");

                                for(int i = 0; i < n; ++i)
                                { 
                                    if(vmgr.ctrls.Count < i || vmgr.LabelTypes(vmgr.GetCtrl(i)) != vmgr.LabelTypes(vmgr.GetCtrl(m)))
                                        vmgr.EmitValidationError("");
                                }
                                vmgr.PopOpd( StackOpd.i32);
                                vmgr.PopOpds( vmgr.LabelTypes(vmgr.GetCtrl(m)));
                                vmgr.Unreachable();

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
                                IndexEntry fie = parentModule.indexingFunction[(int)fnidx];
                                if(fie.type == IndexEntry.FnIdxType.Local)
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
                            vmgr.PopOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.drop:
                            vmgr.PopOpd();
                            TransferInstruction(expanded, instr);
                            break;
                        
                        case Instruction.select:
                            vmgr.PopOpd(StackOpd.i32);
                            StackOpd selos1 = vmgr.PopOpd();
                            StackOpd selos2 = vmgr.PopOpd(selos1);
                            vmgr.PushOpd(selos2);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.local_get:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vmgr.PushOpd(ValiMgr.ConvertToStackType(ty.type));
                                if(ty.size == 4)
                                {
                                    TransferInstruction(expanded, Instruction._local_get32);
                                    TransferInt32u(expanded, this.totalStackSize - ty.offset - 4);
                                }
                                else if(ty.size == 8)
                                {
                                    TransferInstruction(expanded, Instruction._local_get64);
                                    TransferInt32u(expanded, this.totalStackSize - ty.offset - 8);
                                }
                                else
                                    vmgr.EmitValidationError("Retrieving parameter of illegal size.");

                            

                            }
                            break;
                        case Instruction.local_set:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vmgr.PopOpd(ValiMgr.ConvertToStackType(ty.type));
                                if (ty.size == 4)
                                {
                                    TransferInstruction(expanded, Instruction._local_set32);
                                    TransferInt32u(expanded, this.totalStackSize - ty.offset - 4);
                                }
                                else if (ty.size == 8)
                                {
                                    TransferInstruction(expanded, Instruction._local_set64);
                                    TransferInt32u(expanded, this.totalStackSize - ty.offset - 8);
                                }
                                else
                                    vmgr.EmitValidationError("Setting parameter of illegal size.");

                            }
                            break;
                        case Instruction.local_tee:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vmgr.PopOpd(ValiMgr.ConvertToStackType(ty.type));
                                vmgr.PushOpd(ValiMgr.ConvertToStackType(ty.type));
                                if (ty.size == 4)
                                    TransferInstruction(expanded, Instruction._local_tee32);
                                else if (ty.size == 8)
                                    TransferInstruction(expanded, Instruction._local_tee64);
                                else
                                    vmgr.EmitValidationError("Setting parameter of illegal size.");

                                TransferInt32u(expanded, ty.offset);
                            }
                            break;
                        case Instruction.global_get:
                            {
                                uint globalIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                Bin.TypeID type = parentModule.storeDecl.globals[(int)globalIdx].type;

                                // Validate the stack typing
                                vmgr.PushOpd(ValiMgr.ConvertToStackType(type));

                                ValiMgr.DoDataStoreValidation(
                                    this.parentModule.indexingGlobal, 
                                    (int)globalIdx, 
                                    expanded, 
                                    ref globalStore);

                                int typeSize = Memory.GetTypeIDSize(type);
                                if (typeSize == 4)
                                    TransferInstruction(expanded, Instruction._global_get32);
                                else if (typeSize == 8)
                                    TransferInstruction(expanded, Instruction._global_get64);
                                else
                                    vmgr.EmitValidationError("Getting global value of illegal size.");
                            }
                            break;
                        case Instruction.global_set:
                            {
                                // This function is incorrect in that it's a duplicate of local_set.
                                // this eventually needs to pull from the global varable source.

                                uint globalIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                Bin.TypeID type = parentModule.storeDecl.globals[(int)globalIdx].type;

                                // Validate the stack typing
                                vmgr.PopOpd(ValiMgr.ConvertToStackType(type));

                                ValiMgr.DoDataStoreValidation(
                                    this.parentModule.indexingGlobal,
                                    (int)globalIdx,
                                    expanded,
                                    ref globalStore);

                                int typeSize = Memory.GetTypeIDSize(type);
                                if (typeSize == 4)
                                    TransferInstruction(expanded, Instruction._global_set32);
                                else if (typeSize == 8)
                                    TransferInstruction(expanded, Instruction._global_set64);
                                else
                                    vmgr.EmitValidationError("Setting global value of illegal size.");
                            }
                        
                            break;

                        case Instruction.i32_load:
                            ValiMgr.EnsureDefaultMemory( parentModule.indexingMemory, expanded, ref memoryStore);

                            vmgr.PopOpd( StackOpd.i32 );
                            vmgr.PushOpd(StackOpd.i32 );
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_load:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_load:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_load:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_load8_s:
                        case Instruction.i32_load8_u:
                        case Instruction.i32_load16_s:
                        case Instruction.i32_load16_u:
                            {
                                ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                                uint val = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                vmgr.PushOpd(StackOpd.i32);
                                TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.i64_load8_s:
                        case Instruction.i64_load8_u:
                        case Instruction.i64_load16_s:
                        case Instruction.i64_load16_u:
                        case Instruction.i64_load32_s:
                        case Instruction.i64_load32_u:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_store:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PopOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_store:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PopOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_store:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PopOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_store:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PopOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_store8:
                        case Instruction.i32_store16:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PopOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_store8:
                        case Instruction.i64_store16:
                        case Instruction.i64_store32:
                            ValiMgr.EnsureDefaultMemory(parentModule.indexingMemory, expanded, ref memoryStore);

                            BinParse.LoadUnsignedLEB32(pb, ref idx);
                            vmgr.PopOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.MemorySize:
                        case Instruction.MemoryGrow:
                            break;

                        case Instruction.i32_const:
                            {
                                vmgr.PushOpd(StackOpd.i32);
                                TransferInstruction(expanded, instr);

                                uint cval = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                TransferInt32u(expanded, cval);

                            }
                            break;

                        case Instruction.i64_const:
                            {
                                vmgr.PushOpd(StackOpd.i64);
                                TransferInstruction(expanded, instr);

                                ulong cval = BinParse.LoadUnsignedLEB64(pb, ref idx);
                                TransferInt64u(expanded, cval);
                            }
                            break;

                        case Instruction.f32_const:
                            {
                                vmgr.PushOpd(StackOpd.f32);
                                TransferInstruction(expanded, instr);

                                TransferInt32u(expanded, *(uint*)&pb[idx]);
                                idx += 4;

                            }
                            break;

                        case Instruction.f64_const:
                            {
                                vmgr.PushOpd(StackOpd.f64);
                                TransferInstruction(expanded, instr);

                                TransferInt64u(expanded, *(ulong*)&pb[idx]);
                                idx += 8;

                            }
                            break;

                        case Instruction.i32_eqz:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);
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
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_eqz:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i32);
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
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_eq:
                        case Instruction.f32_ne:
                        case Instruction.f32_lt:
                        case Instruction.f32_gt:
                        case Instruction.f32_le:
                        case Instruction.f32_ge:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_eq:
                        case Instruction.f64_ne:
                        case Instruction.f64_lt:
                        case Instruction.f64_gt:
                        case Instruction.f64_le:
                        case Instruction.f64_ge:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_clz:
                        case Instruction.i32_ctz:
                        case Instruction.i32_popcnt:
                            StackOpd unopty = vmgr.PopOpd( StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);
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
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_clz:
                        case Instruction.i64_ctz:
                        case Instruction.i64_popcnt:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i64);
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
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_abs:
                        case Instruction.f32_neg:
                        case Instruction.f32_ceil:
                        case Instruction.f32_floor:
                        case Instruction.f32_trunc:
                        case Instruction.f32_nearest:
                        case Instruction.f32_sqrt:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_add:
                        case Instruction.f32_sub:
                        case Instruction.f32_mul:
                        case Instruction.f32_div:
                        case Instruction.f32_min:
                        case Instruction.f32_max:
                        case Instruction.f32_copysign:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_abs:
                        case Instruction.f64_neg:
                        case Instruction.f64_ceil:
                        case Instruction.f64_floor:
                        case Instruction.f64_trunc:
                        case Instruction.f64_nearest:
                        case Instruction.f64_sqrt:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_add:
                        case Instruction.f64_sub:
                        case Instruction.f64_mul:
                        case Instruction.f64_div:
                        case Instruction.f64_min:
                        case Instruction.f64_max:
                        case Instruction.f64_copysign:
                            vmgr.PopOpd( StackOpd.f64 );
                            vmgr.PopOpd( StackOpd.f64 );
                            vmgr.PushOpd(StackOpd.f64 );
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_wrap_i64:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_trunc_f32_s:
                        case Instruction.i32_trunc_f32_u:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_trunc_f64_s:
                        case Instruction.i32_trunc_f64_u:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_extend_i32_s:
                        case Instruction.i64_extend_i32_u:
                        case Instruction.i64_trunc_f32_s:
                        case Instruction.i64_trunc_f32_u:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_trunc_f64_s:
                        case Instruction.i64_trunc_f64_u:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_convert_i32_s:
                        case Instruction.f32_convert_i32_u:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_convert_i64_s:
                        case Instruction.f32_convert_i64_u:
                        case Instruction.f32_convert_f64:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_convert_i32_s:
                        case Instruction.f64_convert_i32_u:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_convert_i64_s:
                        case Instruction.f64_convert_i64_u:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_promote_f32:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_reinterpret_f32:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f32);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_reinterpret_f64:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i64);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_reinterpret_i32:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_reinterpret_i64:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i64);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_extend8_s:
                        case Instruction.i32_extend16_s:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_extend8_s:
                        case Instruction.i64_extend16_s:
                        case Instruction.i64_extend32_s:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.trunc_sat:
                            { 
                                uint subop = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                switch(subop)
                                { 
                                    case 0:
                                        vmgr.PopOpd(StackOpd.f32);
                                        vmgr.PushOpd(StackOpd.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f32_s);
                                        break;
                                    case 1:
                                        vmgr.PopOpd(StackOpd.f32);
                                        vmgr.PushOpd(StackOpd.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f32_u);
                                        break;
                                    case 2:
                                        vmgr.PopOpd(StackOpd.f64);
                                        vmgr.PushOpd(StackOpd.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f64_s);
                                        break;
                                    case 3:
                                        vmgr.PopOpd(StackOpd.f64);
                                        vmgr.PushOpd(StackOpd.i32);
                                        TransferInstruction(expanded, Instruction._i32_trunc_sat_f64_u);
                                        break;
                                    case 4:
                                        vmgr.PopOpd(StackOpd.f32);
                                        vmgr.PushOpd(StackOpd.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f32_s);
                                        break;
                                    case 5:
                                        vmgr.PopOpd(StackOpd.f32);
                                        vmgr.PushOpd(StackOpd.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f32_u);
                                        break;
                                    case 6:
                                        vmgr.PopOpd(StackOpd.f64);
                                        vmgr.PushOpd(StackOpd.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f64_s);
                                        break;
                                    case 7:
                                        vmgr.PopOpd(StackOpd.f64);
                                        vmgr.PushOpd(StackOpd.i64);
                                        TransferInstruction(expanded, Instruction._i64_trunc_sat_f64_u);
                                        break;
                                    case 0xB:
                                        vmgr.PopOpd(StackOpd.i32);
                                        vmgr.PopOpd(StackOpd.i32);
                                        vmgr.PopOpd(StackOpd.i32);
                                        {
                                            TransferInstruction(expanded, Instruction._memory_fill);
                                            uint filler = BinParse.LoadUnsignedLEB32(pb, ref idx); // Unused placeholder in the WASM spec
                                            TransferInt32u(expanded, filler);
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }

                while(vmgr.ctrls.Count > 0)
                    vmgr.PopCtrl(expanded);

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