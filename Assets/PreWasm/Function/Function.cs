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

        public uint totalLocalsSize;

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

            for (int i = 0; i < this.localTypes.Count; ++i)
            {
                FunctionType.DataOrgInfo doi = this.localTypes[i];
                FunctionType.FillInOrg(ref doi, ref this.totalStackSize);
                this.localTypes[i] = doi;
            }

            this.totalLocalsSize = totalStackSize - this.fnType.totalParamSize;
        }

        unsafe public static void ConsumeTypes(byte * pb, ref uint idx, List<StackOpd> stk)
        {
            while (true)
            {
                // Right now we just pluck out bytes, but doing an LEB128 might
                // be more correct.
                switch ((Bin.TypeID)pb[idx])
                {
                    case Bin.TypeID.Int32:
                        stk.Add(StackOpd.i32);
                        ++idx;
                        break;

                    case Bin.TypeID.Int64:
                        stk.Add(StackOpd.i64);
                        ++idx;
                        break;

                    case Bin.TypeID.Float32:
                        stk.Add(StackOpd.f32);
                        ++idx;
                        break;

                    case Bin.TypeID.Float64:
                        stk.Add(StackOpd.f64);
                        ++idx;
                        break;

                    case Bin.TypeID.Empty: //void return type
                        ++idx;
                        return;

                    default:
                        return;
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

        /// <summary>
        /// Given an encoding, convert it to be usable. The bytecode is converted to
        /// a modified version of the program with a few changes:
        /// - Polymorphic instructions have their types deduced, and will have
        /// their instruction converted to specifialized version based on the
        /// type's bitwidth.
        /// - Things that use a store will check if the cached pointer needs to
        /// be updated first. If they do, extra niche instructions will be injected
        /// before the operator to set things up properly.
        /// - Jump locations are deduced.
        /// - Operators that have offets are broken off into two version, ones that
        /// have a zero-offset, and ones that have a non-zero offset. This is so
        /// zero-offset instructions shed a little bit of overhead.
        /// </summary>
        /// <param name="session">The module the function belongs to.</param>
        /// <param name="index">The function index.</param>
        unsafe public void ExpandExpressionToBeUsable(Module session)
        { 
            List<byte> expanded = new List<byte>();

            // Tracking variables used to make sure pointer caches are setup
            // correctly.
            DataStoreIdx memoryStore    = new DataStoreIdx();
            DataStoreIdx globalStore    = new DataStoreIdx();
            DataStoreIdx tableStore     = new DataStoreIdx();

            ValiMgr vmgr = new ValiMgr();
            // The algorithm in the appendix of the spec didn't say how vu should be initialized,
            // but an initial ctrl is required on the stack.
            // (wleu 02/18/2021)
            List<StackOpd> functionReturnOps = new List<StackOpd>();
            foreach(FunctionType.DataOrgInfo doi in this.fnType.resultTypes)
                functionReturnOps.Add(ValiMgr.ConvertToStackType(doi.type));
            
            vmgr.PushCtrl(
                Instruction.nop, // Filler instruction type. All that matter is that it's not a loop
                new List<StackOpd>(),
                new List<StackOpd>(),
                memoryStore,
                globalStore,
                tableStore);

            FunctionType ft = this.fnType;

            // If the function has local variables, set up the stack space.
            if(this.totalLocalsSize > 0)
            { 
                TransferInstruction(expanded, Instruction._substkLocal);
                TransferInt32u(expanded, this.totalLocalsSize);
            }

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
                                //TransferInstruction(expanded, instr);
                            }
                            break;

                        case Instruction.loop:
                            {
                                List<StackOpd> instk = new List<StackOpd>(); 
                                List<StackOpd> outstk = new List<StackOpd>();
                                ConsumeTypes(pb, ref idx, outstk);

                                vmgr.PopOpds(instk);

                                Vali.CtrlFrame loopFrame = 
                                    vmgr.PushCtrl(
                                        Instruction.loop, 
                                        instk, 
                                        outstk,
                                        memoryStore,
                                        globalStore,
                                        tableStore);

                                loopFrame.loopStart = (uint)expanded.Count;
                                
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
                                int breakDepth = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                if (vmgr.ctrls.Count < breakDepth)
                                    vmgr.EmitValidationError("Stack mismatch for br");

                                CtrlFrame cf = vmgr.GetCtrl(breakDepth);
                                vmgr.PopOpds( cf.LabelTypes());
                                vmgr.Unreachable();

                                TransferInstruction(expanded, Instruction._goto);
                                cf.QueueEndWrite(expanded);
                            }
                            break;

                        case Instruction.br_if:
                            {
                                int breakDepth = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                if(vmgr.ctrls.Count < breakDepth)
                                    vmgr.EmitValidationError("Stack mismatch for br_if");

                                vmgr.PopOpd(StackOpd.i32);

                                CtrlFrame cf = vmgr.GetCtrl(breakDepth);
                                vmgr.PopOpds(cf.LabelTypes());
                                vmgr.PushOpds(cf.LabelTypes());

                                TransferInstruction(expanded, instr);
                                cf.QueueEndWrite(expanded);
                            }
                            break;

                        case Instruction.br_table:
                            {
                                TransferInstruction(expanded, Instruction.br_table);
                                int numBreaks = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);

                                // The table needs to know the offset, not only for validation, but for
                                // hangling the default case.
                                TransferInt32u(expanded, (uint)numBreaks);

                                List<int> breakIDs = new List<int>();
                                int maxBreak = -1;
                                for (int i = 0; i < numBreaks + 1; ++i)
                                {
                                    int breakDepth = (int)BinParse.LoadUnsignedLEB32(pb, ref idx);
                                    breakIDs.Add(breakDepth);
                                    maxBreak = System.Math.Max(maxBreak, breakDepth);
                                }

                                if (maxBreak < 0)
                                    throw new System.Exception("Table break resulted in incorrect maximum break value.");

                                if(vmgr.ctrls.Count < maxBreak)
                                    throw new System.Exception("Table break attempts to exceed number of control frames available.");

                                for(int i = 0; i < numBreaks + 1; ++i)
                                {
                                    CtrlFrame breakFrame = vmgr.GetCtrl(breakIDs[i]);
                                    if (breakFrame.MatchesLabelTypes(vmgr.GetCtrl(maxBreak)) == false)
                                        throw new System.Exception("Table break stack types mismatch.");

                                    breakFrame.QueueEndWrite(expanded);
                                }

                                vmgr.PopOpd(StackOpd.i32);
                                vmgr.PopOpds(vmgr.GetCtrl(maxBreak).LabelTypes());

                                vmgr.Unreachable();
                            }
                            break;

                        case Instruction.returnblock:
                            {
                                // I'm pretty sure this isn't 100% right, but I had issues
                                // deciphering the spec.
                                // (wleu 03/07/2021)
                                
                                int arity = vmgr.opds.Count;
                                if(arity < this.fnType.resultTypes.Count)
                                    throw new System.Exception("Not enough argument on stack for the return value of a function's return statement.");

                                AddFunctionExit(vmgr, expanded, this, functionReturnOps, vmgr.GetStackOpdSize());
                                vmgr.GetCtrl(0).SetToRestoreOnPop();
                                vmgr.Unreachable();
                            }
                            break;

                        case Instruction.call:
                            {
                                uint fnidx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                IndexEntry fie = parentModule.storeDecl.IndexingFunction[(int)fnidx];

                                foreach(FunctionType.DataOrgInfo doi in parentModule.storeDecl.functions[(int)fnidx].fnType.paramTypes)
                                    vmgr.PopOpd(doi.type);

                                foreach (FunctionType.DataOrgInfo doi in parentModule.storeDecl.functions[(int)fnidx].fnType.resultTypes)
                                    vmgr.PushOpd(doi.type);

                                if (fie.type == IndexEntry.FnIdxType.Local)
                                { 
                                    Function fn = this.parentModule.functions[fie.index];

                                    TransferInstruction(expanded, Instruction._call_local);
                                    TransferInt32u(expanded, (uint)fie.index);

                                }
                                else
                                {
                                    DefFunction dfn = this.parentModule.storeDecl.functions[(int)fnidx];

                                    TransferInstruction(expanded, Instruction._call_import);
                                    TransferInt32u(expanded, (uint)fie.index);
                                }

                                // For now we're going to be conservative about other functions 
                                // potentially changing the state of memory - especially if they
                                // resize the memory buffer which could invalidate the current
                                // memory we have cached.
                                memoryStore.SetInvalid();
                                globalStore.SetInvalid();
                                tableStore.SetInvalid();
                            }
                            break;

                        case Instruction.call_indirect:
                            {
                                uint sigIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                uint tbldx = BinParse.LoadUnsignedLEB32(pb, ref idx);

                                // The table index is assumed to be 0 for now and while consumed, 
                                // is currently never used.
                                ValiMgr.EnsureDefaulTable(
                                    this.parentModule.storeDecl.IndexingTable, 
                                    expanded, 
                                    ref globalStore);

                                vmgr.PopOpd(StackOpd.i32);

                                FunctionType fnTy = this.parentModule.types[(int)sigIdx];
                                foreach (FunctionType.DataOrgInfo doi in fnTy.paramTypes)
                                    vmgr.PopOpd(doi.type);

                                foreach (FunctionType.DataOrgInfo doi in fnTy.resultTypes)
                                    vmgr.PushOpd(doi.type);

                                TransferInstruction(expanded, instr);

                                // For now we're going to be conservative about other functions 
                                // potentially changing the state of memory - especially if they
                                // resize the memory buffer which could invalidate the current
                                // memory we have cached.
                                memoryStore.SetInvalid();
                                globalStore.SetInvalid();
                                tableStore.SetInvalid();
                            }
                            break;

                        case Instruction.drop:
                            {
                                StackOpd dropTy = vmgr.PopOpd();
                                int size = ValiMgr.GetSize(dropTy);
                                if(size == 4)
                                    TransferInstruction(expanded, Instruction._pop4b);
                                else if(size == 8)
                                    TransferInstruction(expanded, Instruction._pop8b);
                                else
                                    throw new System.Exception("Encountered instruction to drop stack data of unknown size.");
                                
                            }
                            break;
                        
                        case Instruction.select:
                            vmgr.PopOpd(StackOpd.i32);
                            StackOpd selos1 = vmgr.PopOpd();
                            StackOpd selos2 = vmgr.PopOpd(selos1);
                            vmgr.PushOpd(selos2);

                            switch(selos1)
                            { 
                                case StackOpd.i32:
                                case StackOpd.f32:
                                    TransferInstruction(expanded, Instruction._select32);
                                    break;

                                case StackOpd.i64:
                                case StackOpd.f64:
                                    TransferInstruction(expanded, Instruction._select64);
                                    break;

                                default:
                                    throw new System.Exception("Select of unknown type.");
                            }
                            break;

                        case Instruction.local_get:
                            {
                                uint paramIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                FunctionType.DataOrgInfo ty = this.GetStackDataInfo(paramIdx);

                                vmgr.PushOpd(ty.type);
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

                                vmgr.PopOpd(ty.type);
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

                                vmgr.PopOpd(ty.type); 
                                vmgr.PushOpd(ty.type);
                                if (ty.size == 4)
                                {
                                    TransferInstruction(expanded, Instruction._local_tee32);
                                    TransferInt32u(expanded, this.totalStackSize - ty.offset - 4);
                                }
                                else if (ty.size == 8)
                                {
                                    TransferInstruction(expanded, Instruction._local_tee64);
                                    TransferInt32u(expanded, this.totalStackSize - ty.offset - 8);
                                }
                                else
                                    vmgr.EmitValidationError("Setting parameter of illegal size.");
                            }
                            break;
                        case Instruction.global_get:
                            {
                                uint globalIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                Bin.TypeID type = parentModule.storeDecl.globals[(int)globalIdx].type;

                                // Validate the stack typing
                                vmgr.PushOpd(type);

                                ValiMgr.DoDataStoreValidation(
                                    this.parentModule.storeDecl.IndexingGlobal, 
                                    (int)globalIdx, 
                                    expanded, 
                                    ref globalStore);

                                uint typeSize = DataStore.GetTypeIDSize(type);
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
                                uint globalIdx = BinParse.LoadUnsignedLEB32(pb, ref idx);
                                Bin.TypeID type = parentModule.storeDecl.globals[(int)globalIdx].type;

                                // Validate the stack typing
                                vmgr.PopOpd(type);

                                ValiMgr.DoDataStoreValidation(
                                    this.parentModule.storeDecl.IndexingGlobal,
                                    (int)globalIdx,
                                    expanded,
                                    ref globalStore);

                                uint typeSize = DataStore.GetTypeIDSize(type);
                                if (typeSize == 4)
                                    TransferInstruction(expanded, Instruction._global_set32);
                                else if (typeSize == 8)
                                    TransferInstruction(expanded, Instruction._global_set64);
                                else
                                    vmgr.EmitValidationError("Setting global value of illegal size.");
                            }
                        
                            break;

                        case Instruction.i32_load:
                            {
                                ValidateLoad(
                                    vmgr, 
                                    parentModule, 
                                    ref memoryStore, 
                                    2, 
                                    "i32.load", 
                                    StackOpd.i32, 
                                    expanded, 
                                    Instruction._i32_load_noO, 
                                    Instruction._i32_load_Off, 
                                    pb, 
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    3, 
                                    "i64.load",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load_noO,
                                    Instruction._i64_load_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.f32_load:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    2,
                                    "f32.load",
                                    StackOpd.f32,
                                    expanded,
                                    Instruction._f32_load_noO,
                                    Instruction._f32_load_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.f64_load:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    3,
                                    "f64.load",
                                    StackOpd.f64,
                                    expanded,
                                    Instruction._f64_load_noO,
                                    Instruction._f64_load_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i32_load8_s:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    0,
                                    "i32.load8_s",
                                    StackOpd.i32,
                                    expanded,
                                    Instruction._i32_load8_s_noO,
                                    Instruction._i32_load8_s_Off,
                                    pb,
                                    ref idx);
                            }
                            break;
                        case Instruction.i32_load8_u:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    0,
                                    "i32.load8_u",
                                    StackOpd.i32,
                                    expanded,
                                    Instruction._i32_load8_u_noO,
                                    Instruction._i32_load8_u_Off,
                                    pb,
                                    ref idx);
                            }
                            break;
                        case Instruction.i32_load16_s:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    1,
                                    "i32.load16_s",
                                    StackOpd.i32,
                                    expanded,
                                    Instruction._i32_load16_s_noO,
                                    Instruction._i32_load16_s_Off,
                                    pb,
                                    ref idx);
                            }
                            break;
                        case Instruction.i32_load16_u:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    1,
                                    "i32.load16_u",
                                    StackOpd.i32,
                                    expanded,
                                    Instruction._i32_load16_u_noO,
                                    Instruction._i32_load16_u_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load8_s:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    0,
                                    "i32.load8_u",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load8_s_noO,
                                    Instruction._i64_load8_s_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load8_u:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    0,
                                    "i32.load8_u",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load8_u_noO,
                                    Instruction._i64_load8_u_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load16_s:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    1,
                                    "i32.load16_s",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load16_s_noO,
                                    Instruction._i64_load16_s_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load16_u:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    1,
                                    "i64.load16_u",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load16_s_noO,
                                    Instruction._i64_load16_s_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load32_s:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    2,
                                    "i64.load32_s",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load32_s_noO,
                                    Instruction._i64_load32_s_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_load32_u:
                            {
                                ValidateLoad(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    2,
                                    "i64.load32_u",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_load32_u_noO,
                                    Instruction._i64_load32_u_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i32_store:
                            {
                                ValidateStore(
                                    vmgr, 
                                    parentModule, 
                                    ref memoryStore, 
                                    2, 
                                    "i32.store", 
                                    StackOpd.i32, 
                                    expanded,
                                    Instruction._i32_store_noO,
                                    Instruction._i32_store_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_store:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    3,
                                    "i64.store",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_store_noO,
                                    Instruction._i64_store_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.f32_store:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    2,
                                    "f32.store",
                                    StackOpd.f32,
                                    expanded,
                                    Instruction._f32_store_noO,
                                    Instruction._f32_store_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.f64_store:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    3,
                                    "f64.store",
                                    StackOpd.f64,
                                    expanded,
                                    Instruction._f64_store_noO,
                                    Instruction._f64_store_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i32_store8:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    1,
                                    "i32.store8",
                                    StackOpd.i32,
                                    expanded,
                                    Instruction._i32_store8_noO,
                                    Instruction._i32_store8_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i32_store16:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    2,
                                    "i32.store16",
                                    StackOpd.i32,
                                    expanded,
                                    Instruction._i32_store16_noO,
                                    Instruction._i32_store16_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_store8:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    1,
                                    "i64.store8",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_store8_noO,
                                    Instruction._i64_store8_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_store16:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    3,
                                    "i64.store16",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_store16_noO,
                                    Instruction._i64_store16_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.i64_store32:
                            {
                                ValidateStore(
                                    vmgr,
                                    parentModule,
                                    ref memoryStore,
                                    2,
                                    "i64.store32",
                                    StackOpd.i64,
                                    expanded,
                                    Instruction._i64_store32_noO,
                                    Instruction._i64_store32_Off,
                                    pb,
                                    ref idx);
                            }
                            break;

                        case Instruction.MemorySize:
                            ValiMgr.EnsureDefaultMemory(parentModule.storeDecl.IndexingMemory, expanded, ref memoryStore);

                            ++idx; // Skip reserved/unused following zero byte.
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.MemoryGrow:
                            ValiMgr.EnsureDefaultMemory(parentModule.storeDecl.IndexingMemory, expanded, ref memoryStore);

                            ++idx; // Skip reserved/unused following zero byte.
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_const:
                            {
                                vmgr.PushOpd(StackOpd.i32);
                                TransferInstruction(expanded, instr);

                                int cval = BinParse.LoadSignedLEB32(pb, ref idx);
                                TransferInt32s(expanded, cval);

                            }
                            break;

                        case Instruction.i64_const:
                            {
                                vmgr.PushOpd(StackOpd.i64);
                                TransferInstruction(expanded, instr);

                                long cval = BinParse.LoadSignedLEB64(pb, ref idx);
                                TransferInt64s(expanded, cval);
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
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_trunc_f64_s:
                        case Instruction.i32_trunc_f64_u:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.i32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_extend_i32_s:
                        case Instruction.i64_extend_i32_u:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_trunc_f32_s:
                        case Instruction.i64_trunc_f32_u:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_trunc_f64_s:
                        case Instruction.i64_trunc_f64_u:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_convert_i32_s:
                        case Instruction.f32_convert_i32_u:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_convert_i64_s:
                        case Instruction.f32_convert_i64_u:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_demote_f64:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.f32);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_convert_i32_s:
                        case Instruction.f64_convert_i32_u:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_convert_i64_s:
                        case Instruction.f64_convert_i64_u:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_promote_f32:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.f64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i32_reinterpret_f32:
                            vmgr.PopOpd(StackOpd.f32);
                            vmgr.PushOpd(StackOpd.i32);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.i64_reinterpret_f64:
                            vmgr.PopOpd(StackOpd.f64);
                            vmgr.PushOpd(StackOpd.i64);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f32_reinterpret_i32:
                            vmgr.PopOpd(StackOpd.i32);
                            vmgr.PushOpd(StackOpd.f32);

                            // Validate the instruction, but we don't need to emit any
                            // instructions - it literally changes nothing with the 
                            // program or stack state.
                            //TransferInstruction(expanded, instr);
                            break;

                        case Instruction.f64_reinterpret_i64:
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.f64);

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
                            vmgr.PopOpd(StackOpd.i64);
                            vmgr.PushOpd(StackOpd.i64);
                            TransferInstruction(expanded, instr);
                            break;

                        case Instruction.extPrefixed:
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
                                            ValiMgr.EnsureDefaultMemory(parentModule.storeDecl.IndexingMemory, expanded, ref memoryStore);

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

                // Pop all the controls. The biggest reason we do this is to make sure
                // all the queued jump tables are written.
                while(vmgr.ctrls.Count > 0)
                    vmgr.PopCtrl(expanded);

                AddFunctionExit(vmgr, expanded, this, functionReturnOps, vmgr.GetStackOpdSize());
                

                this.expression = expanded.ToArray();
            }
        }

        public static void AddFunctionExit(ValiMgr vmgr, List<byte> expanded, Function fn, List<StackOpd> returnTypes, int stackLeft)
        {
            vmgr.PopOpds(returnTypes, true);

            // If we have any return values, we need to move the, from the current 
            // location right on top of where the stack will be when we pop this
            // call frame.
            if (fn.fnType.totalResultSize > 0)
            {
                // But if we don't have any stack items (local variables or parameters)
                // then the return values are already where they need to be.
                if (fn.totalStackSize > 0)
                {
                    // But if we do have return values, we copy the return data to overwrite 
                    // the byte where they need to be.
                    TransferInstruction(expanded, Instruction._stackbackwrite);
                    TransferInt32u(expanded, fn.totalStackSize);   // How much to move the stack by to overwrite the parameters
                    TransferInt32u(expanded, fn.fnType.totalResultSize);  // How much bytes in the results payload that need to be transfered
                }
            }
            else if (fn.fnType.totalResultSize == 0)
            {
                // If we don't have return values, we don't have a _stackbackwrite to fix up the
                // stack. So instead we'll explicitly add to the stack to erase the locals and
                // function parameters.
                if (fn.fnType.totalParamSize > 0 || stackLeft > 0)
                {
                    TransferInstruction(expanded, Instruction._addstk);
                    TransferInt32u(expanded, (uint)(fn.fnType.totalParamSize + stackLeft));
                }
            }

            TransferInstruction(expanded, Instruction.returnblock);
        }

        FunctionType.DataOrgInfo GetStackDataInfo(uint uidx)
        {
            if(uidx >= this.localTypes.Count + this.fnType.paramTypes.Count)
                throw new System.Exception("Attempting to get local value out of bounds.");

            if (uidx < this.fnType.paramTypes.Count)
                return this.fnType.paramTypes[(int)uidx];
            else
                return this.localTypes[(int)uidx - fnType.paramTypes.Count];
        }

        unsafe public static void ValidateLoad(
            ValiMgr vmgr, 
            Module parentModule, 
            ref DataStoreIdx memoryStore, 
            uint maxAlign, 
            string opName, 
            StackOpd pushType, 
            List<byte> expanded, 
            Instruction instrZeroOff, 
            Instruction instrNonZeroOff, 
            byte * pb, 
            ref uint idx)
        {
            ValiMgr.EnsureDefaultMemory(parentModule.storeDecl.IndexingMemory, expanded, ref memoryStore);

            vmgr.PopOpd(StackOpd.i32);
            vmgr.PushOpd(pushType);

            uint align = BinParse.LoadUnsignedLEB32(pb, ref idx);
            uint offset = BinParse.LoadUnsignedLEB32(pb, ref idx);

            if(align < 0 || align > maxAlign)
                throw new System.Exception($"Invalid alignment of {align} for {opName}. Type limits the alignment between 0 and {maxAlign}.");

            // We throw away the alignment for now, not supported.
            if(offset == 0)
                TransferInstruction(expanded, instrZeroOff);
            else
            {
                TransferInstruction(expanded, instrNonZeroOff);
                TransferInt32u(expanded, offset);
            }
        }

        unsafe public static void ValidateStore(
           ValiMgr vmgr,
           Module parentModule,
           ref DataStoreIdx memoryStore,
           uint maxAlign,
           string opName,
           StackOpd popType,
           List<byte> expanded,
           Instruction instrZeroOff,
           Instruction instrNonZeroOff,
           byte* pb,
           ref uint idx)
        {
            ValiMgr.EnsureDefaultMemory(parentModule.storeDecl.IndexingMemory, expanded, ref memoryStore);

            vmgr.PopOpd(popType);
            vmgr.PopOpd(StackOpd.i32);

            uint align = BinParse.LoadUnsignedLEB32(pb, ref idx);
            uint offset = BinParse.LoadUnsignedLEB32(pb, ref idx);

            if (align < 0 || align > maxAlign)
                throw new System.Exception($"Invalid alignment of {align} for {opName}. Type limits the alignment between 0 and {maxAlign}.");

            // We throw away the alignment for now, not supported.
            if (offset == 0)
                TransferInstruction(expanded, instrZeroOff);
            else
            {
                TransferInstruction(expanded, instrNonZeroOff);
                TransferInt32u(expanded, offset);
            }
        }
    }
}