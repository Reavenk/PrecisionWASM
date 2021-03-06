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

using System.Collections.Generic;

namespace PxPre.WASM
{
    public class ExecutionContext
    {
        /// <summary>
        /// The size of the stack for code execution. It's given a default of a megabyte.
        /// </summary>
        const int InitialStackPos = 1024 * 1024;

        /// <summary>
        /// Working stack memory.
        /// </summary>
        public byte[] stack = new byte[InitialStackPos];

        /// <summary>
        /// Instead of starting at the beginning and using the stack by incrementing it, it's
        /// more efficient to point at the very end and use the stack by decrementing. 
        /// 
        /// This (counter-intuitively) means the beginning of the stack is at the end of the 
        /// array - and the end of the stack is at the beginning of the arrow. This also means
        /// to push memory, we subtract the stackPos; and we add to pop memory.
        /// </summary>
        public int stackPos = InitialStackPos;

        public List<Memory> memories = new List<Memory>();
        public List<Global> globals = new List<Global>();
        public List<Table> tables = new List<Table>();

        public List<Limits> memoryLimits = new List<Limits>();
        public List<Limits> tableLimits = new List<Limits>();

        public readonly Module instancer;
        public ImportModule importData {get; protected set; }

        private bool initialized = false;
        public bool Initialized { get => this.initialized; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExecutionContext(Module module, bool start = true) 
        { 
            this.instancer = module;
            this.importData = new ImportModule(module.storeDecl); 

            for(int i = 0; i < module.storeDecl.localMemsCt; ++i)
                this.memories.Add(null);

            for(int i = 0; i < module.storeDecl.memories.Count; ++i)
            {
                IndexEntry ie = module.storeDecl.IndexingMemory[i];
                if (ie.type == IndexEntry.FnIdxType.Import)
                    continue;

                this.memories[ie.index] = module.storeDecl.memories[i].CreateDefault();
            }

            for(int i = 0; i < module.storeDecl.localGlobalCt; ++i)
                this.globals.Add(null);

            for(int i = 0; i < module.storeDecl.globals.Count; ++i)
            { 
                IndexEntry ie = module.storeDecl.IndexingGlobal[i];
                if(ie.type == IndexEntry.FnIdxType.Import)
                    continue;

                this.globals[ie.index] = module.storeDecl.globals[i].CreateDefault();
            }

            for(int i = 0; i < module.storeDecl.tables.Count; ++i)
                this.tables.Add(null);

            for(int i = 0; i < module.storeDecl.tables.Count; ++i)
            { 
                IndexEntry ie = module.storeDecl.IndexingTable[i];
                if(ie.type == IndexEntry.FnIdxType.Import)
                    continue;

                this.tables[ie.index] = module.storeDecl.tables[i].CreateDefault();
            }

            if(start == true)
                this.InvokeStart();
        }

        unsafe public void RunFunction(Module module, int index)
        {
            IndexEntry fie = module.storeDecl.IndexingFunction[index];

            if(fie.type == IndexEntry.FnIdxType.Local)
            { 
                this.RunLocalFunction(module, fie.index);
            }
            else if(fie.type == IndexEntry.FnIdxType.Import)
            { 
                this.RunFunction( this.importData.importFn[fie.index]);
            }
            else
                throw new System.Exception(); // TODO: Error msg
        }

        public void RunFunction(ImportFunction ifn)
        { 
            if(ifn == null)
                throw new System.Exception(); // TODO: Error msg


            ImportFunctionUtil ifu = 
                new ImportFunctionUtil(ifn.functionType, this, this.stackPos);

            ifn.InvokeImpl(ifu);
        }

        unsafe public void RunLocalFunction(Module module, int localIndex)
        {
            Function fn = module.functions[localIndex];
            this.RunFunction(fn);
        }

        unsafe public void RunFunction(Function fn)
        { 

            int ip = 0;

            int startStack = this.stackPos;

            byte * pbGlob = null;

            Memory curMemStore = null;
            byte * pbMem = null;
            byte * pbTable = null;

            fixed (byte * pb = fn.expression, pstk = this.stack)
            {
                while(true)
                { 
                    Instruction instr = (Instruction)(int)*(ushort*)&pb[ip];
                    ip += 2;

                    switch(instr)
                    {
                        default:
                            throw new System.Exception($"Unknown opcode encountered, with code value {instr}");

                        case Instruction.nop:
                            break;

                        case Instruction.block:
                            break;

                        case Instruction.loop:
                            break;

                        case Instruction.ifblock:
                            {
                                int nbool = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;
                                if ( nbool != 0)
                                    ip += 4; // Skip to right after the jump point
                                else
                                {
                                    // Set to the jump point
                                    ip = *(int*)&pb[ip];
                                }
                            }
                            break;

                        case Instruction._goto:
                            ip = *(int*)&pb[ip];
                            break;

                        // case Instruction.elseblock:
                        //     break;

                        // case Instruction.end:
                        //     return;

                        case Instruction.br:
                            break;

                        case Instruction.br_if:
                            break;

                        case Instruction.br_table:
                            break;

                        case Instruction.returnblock:
                            return;

                        case Instruction._stackbackwrite:
                            {
                                int paramSz = *(int*)&pb[ip];
                                int resultSz = *(int*)&pb[ip + 4];
                                ip += 8;

                                // Move the return value data on the stack to overwrite
                                // the function call parameters on the stack.
                                //
                                // This is not an elegant solution, and will require future
                                // investigation on how to pop the call parameters when
                                // the return value is already on top of it on the stack.
                                //
                                // If we do keep this stackbackwrite strategy, for the very least
                                // we can probably leverage the fact that the byte counts will always
                                // be divisible by 4 and copy 32 bit chuncks instead of bytes.
                                for(int i = resultSz - 1; i >= 0; --i)
                                { 
                                    pstk[this.stackPos + paramSz + i] = 
                                        pstk[this.stackPos + i]; 
                                }

                                this.stackPos += (int)paramSz; // We need to figure out the best typing
                            }
                            break;

                        case Instruction._call_local:
                            {
                                uint fnid = *(uint*)&pb[ip];
                                ip += 4;

                                RunLocalFunction(fn.parentModule, (int)fnid);
                            }
                            break;

                        case Instruction._call_import:
                            {
                                uint fnid = *(uint*)&pb[ip];
                                ip += 4;

                                RunFunction(this.importData.importFn[(int)fnid]);
                            }
                            break;

                        case Instruction.call_indirect:
                            break;


                        case Instruction.drop:
                            // Skip the next operation - Not sure if we need to do more
                            // since we could be skipping an op that has parameters.
                            byte dropOpSz = pstk[stackPos - 1];
                            stackPos += dropOpSz;
                            break;

                        case Instruction.select:
                            int selOperand = *(int*)(&pstk[stackPos]);
                            stackPos +=6;

                            byte selOpSz = pstk[stackPos - 1];
                            if (*(int*)(&pstk[stackPos]) == 0)
                            {
                                // Drop it
                                stackPos += selOpSz;
                            }
                            else
                            { 
                                int oldStk = stackPos;  // Prepare for transfer.
                                stackPos += selOpSz;    // Drop ahead of time.

                                // Transfer it.
                                for(int i = 0; i < selOpSz - 2; ++i)
                                    pstk[stackPos + i] = pstk[oldStk + i];
                            }
                            break;

                        case Instruction._local_get32:
                            {
                                this.stackPos -= 4;
                                int offs = *(int*)&pb[ip];
                                ip += 4;
                                *(int*)&pstk[this.stackPos] = *(int*)&pstk[startStack + offs];
                            }
                            break;

                        case Instruction._local_get64:
                            {
                                this.stackPos -= 8;
                                int offs = *(int*)&pb[ip];
                                ip += 4;
                                *(long*)&pstk[this.stackPos] = *(long*)&pstk[startStack + offs];
                            }
                            break;

                        case Instruction._local_set32:
                            {
                                int offs = *(int*)&pb[ip];
                                ip += 4;
                                *(int*)&pstk[startStack + offs] = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction._local_set64:
                            {
                                int offs = *(int*)&pb[ip];
                                ip += 4;
                                *(long*)&pstk[startStack + offs] = *(long*)&pstk[this.stackPos];
                                this.stackPos += 8;
                            }
                            break;

                        case Instruction._local_tee32:
                            {
                                int offs = *(int*)&pb[ip];
                                ip += 4;
                                // A tee "returns" the value, so it pretty much places the value back on the stack
                                *(int*)&pstk[startStack + offs] = *(int*)&pstk[this.stackPos];
                            }
                            break;

                        case Instruction._local_tee64: 
                            {
                                int offs = *(int*)&pb[ip];
                                ip += 4;
                                *(long*)&pstk[startStack + offs] = *(long*)&pstk[this.stackPos];
                            }
                            break;

                        case Instruction._global_chStoreImp:
                            {
                                // Take an i32 off the instructions and set that as the index of the 
                                // import's global store to set/get from.
                                int importIdx = *(int*)&pb[ip];
                                ip += 4;

                                pbGlob = this.importData.globals[importIdx].store.pdata;
                            }
                            break;

                        case Instruction._global_chStoreLoc:
                            { 
                                // Take an i32 off the instructions and set that as the index of the 
                                // ExecutionContent's global store to set/get from.
                                int localIdx = *(int*)&pb[ip];
                                ip += 4;

                                pbGlob = this.globals[localIdx].store.pdata;
                            }
                            break;

                        case Instruction._global_get32:
                            {
                                // Push 4 bytes
                                this.stackPos -= 4;
                                * (int*)&pstk[this.stackPos] = *(int*)pbGlob;
                            }
                            break;

                        case Instruction._global_get64:
                            {
                                // Push 8 bytes
                                this.stackPos -= 8;
                                *(int*)&pstk[this.stackPos] = *(int*)pbGlob;
                            }
                            break;

                        case Instruction._global_set32:
                            {
                                *(int*)pbGlob = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction._global_set64:
                            {
                                *(long*)pbGlob = *(long*)&pstk[this.stackPos];
                                this.stackPos += 8;
                            }
                            break;

                        case Instruction._i32_load_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = *(int*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i32_load_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = *(int*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load_noO:
                            { 
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(long*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(long*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._f32_load_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(float*)&pstk[this.stackPos] = *(float*)&pbMem[memid];
                            }
                            break;

                        case Instruction._f32_load_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(float*)&pstk[this.stackPos] = *(float*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._f64_load_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(double*)&pstk[this.stackPos] = *(double*)&pbMem[memid];
                            }
                            break;

                        case Instruction._f64_load_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(double*)&pstk[this.stackPos] = *(double*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i32_load8_s_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = (sbyte)pbMem[memid];
                            }
                            break;

                        case Instruction._i32_load8_s_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = (sbyte)pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i32_load8_u_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = pbMem[memid];
                            }
                            break;

                        case Instruction._i32_load8_u_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i32_load16_s_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = *(short*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i32_load16_s_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = *(short*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i32_load16_u_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(uint*)&pstk[this.stackPos] = *(ushort*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i32_load16_u_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(uint*)&pstk[this.stackPos] = *(ushort*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load8_s_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(sbyte*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load8_s_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(sbyte*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load8_u_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = (byte)pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load8_u_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = (byte)pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load16_s_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(short*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load16_s_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(short*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load16_u_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = *(ushort*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load16_u_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = *(ushort*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load32_s_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(int*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load32_s_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(int*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i64_load32_u_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = *(uint*)&pbMem[memid];
                            }
                            break;

                        case Instruction._i64_load32_u_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = *(uint*)&pbMem[memid + offset];
                            }
                            break;

                        case Instruction._i32_store_noO:
                        case Instruction._f32_store_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 4];
                                *(int*)&pbMem[memid] = *(int*)&pstk[this.stackPos];
                                this.stackPos += 8; // Pop an int, and a 4byte param
                            }
                            break;

                        case Instruction._i32_store_Off:
                        case Instruction._f32_store_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 4];
                                *(int*)&pbMem[memid + offset] = *(int*)&pstk[this.stackPos];
                                this.stackPos += 8; // Pop an int, and a 4byte param
                            }
                            break;

                        case Instruction._i64_store_noO:
                        case Instruction._f64_store_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 8];
                                *(long*)&pbMem[memid] = *(long*)&pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and a 8 byte param
                            }
                            break;

                        case Instruction._i64_store_Off:
                        case Instruction._f64_store_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 8];
                                *(long*)&pbMem[memid + offset] = *(long*)&pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and a 8 byte param
                            }
                            break;

                        case Instruction._i32_store8_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 4];
                                pbMem[memid] = pstk[this.stackPos];
                                this.stackPos += 8; // Pop 2 ints
                            }
                            break;

                        case Instruction._i32_store8_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 4];
                                pbMem[memid + offset] = pstk[this.stackPos];
                                this.stackPos += 8; // Pop 2 ints
                            }
                            break;

                        case Instruction._i32_store16_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 4];
                                *(short*)&pbMem[memid] = *(short*)&pstk[this.stackPos];
                                this.stackPos += 4; // Pop 2 ints
                            }
                            break;

                        case Instruction._i32_store16_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 4];
                                *(short*)&pbMem[memid + offset] = *(short*)&pstk[this.stackPos];
                                this.stackPos += 4; // Pop 2 ints
                            }
                            break;

                        case Instruction._i64_store8_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 8];
                                pbMem[memid] = pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and an int64
                            }
                            break;

                        case Instruction._i64_store8_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 8];
                                pbMem[memid + offset] = pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and an int64
                            }
                            break;

                        case Instruction._i64_store16_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 8];
                                *(short*)&pbMem[memid] = *(short*)&pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and an int64
                            }
                            break;

                        case Instruction._i64_store16_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 8];
                                *(short*)&pbMem[memid + offset] = *(short*)&pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and an int64
                            }
                            break;

                        case Instruction._i64_store32_noO:
                            {
                                int memid = *(int*)&pstk[this.stackPos + 8];
                                *(int*)&pbMem[memid] = *(int*)&pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and an int64
                            }
                            break;

                        case Instruction._i64_store32_Off:
                            {
                                int offset = *(int*)&pb[ip];
                                ip += 4;

                                int memid = *(int*)&pstk[this.stackPos + 8];
                                *(int*)&pbMem[memid + offset] = *(int*)&pstk[this.stackPos];
                                this.stackPos += 12; // Pop an int, and an int64
                            }
                            break;

                        case Instruction._SetMemoryStoreImp:
                            {
                                int idx = *(int*)&pb[ip]; 
                                ip += 4;

                                curMemStore = this.importData.memories[idx];
                                pbMem = curMemStore.store.pdata;
                            }
                            break;

                        case Instruction._SetMemoryStoreLoc:
                            {
                                int idx = *(int*)&pb[ip];
                                ip += 4;

                                curMemStore = this.memories[idx];
                                pbMem = curMemStore.store.pdata;
                            }
                            break;

                        case Instruction.MemorySize:
                            {
                                this.stackPos -= 4;

                                if (curMemStore == null)
                                    *(int*)&pstk[this.stackPos] = -1;
                                else
                                {
                                    // The return is a page size, and we're following the rule that the memory size is always
                                    // a multiple of the page size.
                                    *(int*)&pstk[this.stackPos] = (int)curMemStore.CalculatePageCt();
                                }
                            }
                            break;

                        case Instruction.MemoryGrow:
                            { 
                                int newPages = *(int*)&pstk[this.stackPos];

                                // The stackpop is popped, but another 32 bit values is put on the 
                                // stack. No stack position modification.

                                if(newPages < 0)
                                {
                                    *(int*)&pstk[this.stackPos] = -1;
                                }
                                else
                                {
                                    // The signed/unsigned mixing may need some review. It's pretty washy.

                                    uint oldPageSz = curMemStore.CalculatePageCt();
                                    if(newPages == 0)
                                        *(int*)&pstk[this.stackPos] = (int)oldPageSz;
                                    else
                                    { 
                                        DataStore.ExpandRet expRet = 
                                            this.memories[0].ExpandPageCt((int)(oldPageSz + newPages));

                                        if (expRet == DataStore.ExpandRet.Successful)
                                        {
                                            *(uint*)&pstk[this.stackPos] = oldPageSz;
                                            pbMem = this.memories[0].store.pdata;
                                        }
                                        else
                                            *(int*)&pstk[this.stackPos] = -1;
                                    }
                                }
                            }
                            break;

                        case Instruction.i32_const:
                            stackPos -= 4;
                            *(int*)(&pstk[stackPos]) = *(int*)&pb[ip];
                            ip += 4;
                            break;

                        case Instruction.i64_const:
                            stackPos -= 8;
                            *(long*)(&pstk[stackPos]) = *(long*)&pb[ip];
                            ip += 8;
                            break;

                        case Instruction.f32_const:
                            stackPos -= 4;
                            *(float*)(&pstk[stackPos]) = *(float*)&pb[ip];
                            ip += 4;
                            break;

                        case Instruction.f64_const:
                            stackPos -= 8;
                            *(double*)(&pstk[stackPos]) = *(double*)&pb[ip];
                            ip += 8;
                            break;

                        case Instruction.i32_eqz:
                            {
                                bool b = *(int*)(&pstk[stackPos]) == 0;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_eq:
                            {
                                bool b = *(int*)(&pstk[stackPos + 4]) == *(int*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_ne:
                            {
                                bool b = *(int*)(&pstk[stackPos + 4]) != *(int*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_lt_s:
                            {
                                bool b = *(int*)(&pstk[stackPos + 4]) < *(int*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_lt_u:
                            {
                                bool b = *(uint*)(&pstk[stackPos + 4]) < *(uint*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_gt_s:
                            {
                                bool b = *(int*)(&pstk[stackPos + 4]) > *(int*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_gt_u:
                            {
                                bool b = *(uint*)(&pstk[stackPos + 4]) > *(uint*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_le_s:
                            {
                                bool b = *(int*)(&pstk[stackPos + 4]) <= *(int*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_le_u:
                            {
                                bool b = *(uint*)(&pstk[stackPos + 4]) <= *(uint*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_ge_s:
                            {
                                bool b = *(int*)(&pstk[stackPos + 4]) >= *(int*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_ge_u:
                            {
                                bool b = *(uint*)(&pstk[stackPos + 4]) >= *(uint*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_eqz:
                            {
                                bool b = *(long*)(&pstk[this.stackPos]) == 0;
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[this.stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_eq:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) == *(long*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_ne:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) != *(long*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_lt_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) < *(long*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_lt_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) < *(ulong*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_gt_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) > *(long*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_gt_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) > *(ulong*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_le_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) <= *(long*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_le_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) <= *(ulong*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_ge_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) >= *(long*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_ge_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) >= *(ulong*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 longs, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_eq:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) == *(float*)(&pstk[stackPos]);
                                stackPos += 4; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_ne:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) != *(float*)(&pstk[stackPos]);
                                stackPos += 4; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_lt:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) < *(float*)(&pstk[stackPos]);
                                stackPos += 4; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_gt:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) > *(float*)(&pstk[stackPos]);
                                stackPos += 4; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_le:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) <= *(float*)(&pstk[stackPos]);
                                stackPos += 4; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_ge:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) >= *(float*)(&pstk[stackPos]);
                                stackPos += 4; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_eq:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) == *(double*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_ne:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) != *(double*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_lt:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) < *(double*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_gt:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) > *(double*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_le:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) <= *(double*)(&pstk[stackPos]);
                                stackPos += 12; // Pop 2 floats, Push 1 int;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_ge:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) >= *(double*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i32_clz:
                            {
                                // Count leading zero bits of an int.
                                int topi = *(int*)(&pstk[stackPos]);
                                if(topi == 0)
                                    *(int*)(&pstk[stackPos]) = 32;
                                else
                                { 
                                    for(int i = 0; i < 32; ++i)
                                    { 
                                        if((topi & ((1 << 31) >> i)) != 0)
                                        {
                                            *(int*)(&pstk[stackPos]) = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case Instruction.i32_ctz:
                            {
                                // Count trailing zero bits of an int
                                int topi = *(int*)(&pstk[stackPos]);
                                if (topi == 0)
                                    *(int*)(&pstk[stackPos]) = 32;
                                else
                                {
                                    for (int i = 0; i < 32; ++i)
                                    {
                                        if ((topi & (1 << i)) != 0)
                                        {
                                            *(int*)(&pstk[stackPos]) = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case Instruction.i32_popcnt:
                            {
                                // Count non-zero bits
                                int topi = *(int*)(&pstk[stackPos]);
                                int v = 0;
                                for (int i = 0; i < 32; ++i)
                                {
                                    if ((topi & (1 << i)) != 0)
                                        ++v;
                                }
                                *(int*)(&pstk[stackPos]) = v;
                            }
                            break;

                        case Instruction.i32_add:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos]) + *(int*)(&pstk[stackPos + 4]);

                            this.stackPos += 4;
                            break;

                        case Instruction.i32_sub:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) - *(int*)(&pstk[stackPos]);

                            this.stackPos += 4;
                            break;

                        case Instruction.i32_mul:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) * *(int*)(&pstk[stackPos]);

                            this.stackPos += 4;
                            break;

                        case Instruction.i32_div_s:
                            // HELP: It's unknown why this divide will still raise overflow exceptions.
                            // The same probably goes for all the other integer divides.
                            unchecked
                            {
                                *(int*)(&pstk[stackPos + 4]) =
                                    *(int*)(&pstk[stackPos + 4]) / *(int*)(&pstk[stackPos]);
                            }

                            this.stackPos += 4;
                            break;

                        case Instruction.i32_div_u:
                            unchecked
                            {
                                *(uint*)(&pstk[stackPos + 4]) =
                                   *(uint*)(&pstk[stackPos + 4]) / *(uint*)(&pstk[stackPos]);
                            }
                            this.stackPos += 4;
                            break;

                        case Instruction.i32_rem_s:
                            unchecked
                            {
                                *(int*)(&pstk[stackPos + 4]) =
                                    *(int*)(&pstk[stackPos + 4]) % *(int*)(&pstk[stackPos]);
                            }

                            this.stackPos += 4;
                            break;

                        case Instruction.i32_rem_u:
                            unchecked
                            {
                                *(uint*)(&pstk[stackPos + 4]) =
                                    *(uint*)(&pstk[stackPos + 4]) % *(uint*)(&pstk[stackPos]);
                            }

                            this.stackPos += 4;
                            break;

                        case Instruction.i32_and:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) & *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_or:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) | *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_xor:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) ^ *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_shl:
                            {
                                * (int*)(&pstk[stackPos + 4]) =
                                    *(int*)(&pstk[stackPos + 4]) << *(int*)(&pstk[stackPos]);
                            }
                            stackPos += 4;
                            break;

                        case Instruction.i32_shr_s:
                            // Right shift, but bit extend the most significant value
                            {
                                *(int*)(&pstk[stackPos + 4]) =
                                    *(int*)(&pstk[stackPos + 4]) >> *(int*)(&pstk[stackPos]);

                                stackPos += 4;
                            }
                            break;

                        case Instruction.i32_shr_u:
                            *(uint*)(&pstk[stackPos + 4]) =
                                *(uint*)(&pstk[stackPos + 4]) >> *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_rotl:
                            {
                                uint val = *(uint*)(&pstk[stackPos + 4]);
                                int shiftAmt = *(int*)(&pstk[stackPos]);

                                
                                *(uint*)(&pstk[this.stackPos + 4]) = ((val << shiftAmt) | (val >> (32 - shiftAmt)));
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.i32_rotr:
                            {
                                uint val = *(uint*)(&pstk[stackPos + 4]);
                                int shiftAmt = *(int*)(&pstk[stackPos]);

                                *(uint*)(&pstk[this.stackPos + 4]) = ((val >> shiftAmt) | (val << (32 - shiftAmt)));
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.i64_clz:
                            {
                                // Count leading zero bits
                                long topi = *(long*)(&pstk[this.stackPos]);
                                if (topi == 0)
                                    *(long*)(&pstk[this.stackPos]) = 64;
                                else
                                {
                                    for (int i = 0; i < 64; ++i)
                                    {
                                        if ((topi & ((1 << 63) >> i)) != 0)
                                        {
                                            *(long*)(&pstk[this.stackPos]) = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case Instruction.i64_ctz:
                            {
                                // Count trailing zero bits
                                long topi = *(long*)(&pstk[this.stackPos]);
                                if (topi == 0)
                                    *(long*)(&pstk[this.stackPos]) = 64;
                                else
                                {
                                    for (int i = 0; i < 64; ++i)
                                    {
                                        if ((topi & (1 << i)) != 0)
                                        {
                                            *(long*)(&pstk[this.stackPos]) = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case Instruction.i64_popcnt:
                            {
                                // Count non-zero bits
                                long topi = *(long*)(&pstk[this.stackPos]);
                                long v = 0;
                                for (int i = 0; i < 64; ++i)
                                {
                                    if ((topi & (1 << i)) != 0)
                                        ++v;
                                }
                                *(long*)(&pstk[this.stackPos]) = v;
                            }
                            break;

                        case Instruction.i64_add:
                            {
                                *(long*)(&pstk[this.stackPos + 8]) =
                                    *(long*)(&pstk[this.stackPos + 8]) + *(long*)(&pstk[this.stackPos]);

                                stackPos += 8;
                            }
                            break;

                        case Instruction.i64_sub:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) - *(long*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_mul:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) * *(long*)(&pstk[this.stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_div_s:
                            unchecked
                            {
                                *(long*)(&pstk[this.stackPos + 8]) =
                                   *(long*)(&pstk[this.stackPos + 8]) / *(long*)(&pstk[this.stackPos]);
                            }

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_div_u:
                            unchecked
                            {
                                *(ulong*)(&pstk[this.stackPos + 8]) =
                                   *(ulong*)(&pstk[this.stackPos + 8]) / *(ulong*)(&pstk[this.stackPos]);
                            }

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_rem_s:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) % *(long*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_rem_u:
                            *(ulong*)(&pstk[this.stackPos + 8]) =
                                *(ulong*)(&pstk[this.stackPos + 8]) % *(ulong*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_and:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) & *(long*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_or:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) | *(long*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_xor:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) ^ *(long*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_shl:
                            *(long*)(&pstk[this.stackPos + 8]) =
                                *(long*)(&pstk[this.stackPos + 8]) << *(int*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_shr_s:
                            {
                                long val = *(long*)(&pstk[this.stackPos + 8]);
                                long shift = *(long*)(&pstk[this.stackPos]);

                                this.stackPos += 8;
                                *(long*)(&pstk[this.stackPos]) = val >> (int)shift;
                            }
                            break;

                        case Instruction.i64_shr_u:
                            *(ulong*)(&pstk[this.stackPos + 8]) =
                                *(ulong*)(&pstk[this.stackPos + 8]) >> *(int*)(&pstk[this.stackPos]);

                            this.stackPos += 8;
                            break;

                        case Instruction.i64_rotl:
                            {
                                ulong val = *(ulong*)(&pstk[stackPos + 8]);
                                int shiftAmt = (int)*(long*)(&pstk[stackPos]) % 64;

                                this.stackPos += 8;
                                *(ulong*)(&pstk[this.stackPos]) = (val << shiftAmt) | (val >> (64 - shiftAmt));
                            }
                            break;

                        case Instruction.i64_rotr:
                            {
                                ulong val = *(ulong*)(&pstk[stackPos + 8]);
                                int shiftAmt = (int)*(long*)(&pstk[stackPos]) % 64;

                                this.stackPos += 8;
                                *(ulong*)(&pstk[this.stackPos]) = (val >> shiftAmt) | (val << (64 - shiftAmt));
                            }
                            break;

                        case Instruction.f32_abs:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) >= 0.0f ?
                                    *(float*)(&pstk[stackPos]) :
                                    -*(float*)(&pstk[stackPos]);
                            break;

                        case Instruction.f32_neg:
                            *(float*)(&pstk[stackPos]) = -*(float*)(&pstk[stackPos]);
                            break;

                        case Instruction.f32_ceil:
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Ceiling(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_floor:
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-ffloor
                            //
                            // If z is a NaN, then return an element of nansN{z}.
                            // Else if z is an infinity, then return z.
                            // Else if z is a zero, then return z.
                            // Else if z is greater than 0 but smaller than 1, then return positive zero.
                            // Else return the largest integral value that is not larger than z.
                            //
                            // The definition of System.Math matches this logic.
                            // https://docs.microsoft.com/en-us/dotnet/api/system.math.floor
                            // Returns the largest integral value less than or equal to the specified number.

                            *(float*)(&pstk[stackPos]) = (float)System.Math.Floor(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_trunc:
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-ftrunc
                            //
                            // If z is a NaN, then return an element of nansN{z}.
                            // Else if z is an infinity, then return z.
                            // Else if z is a zero, then return z.
                            // Else if z is greater than 0 but smaller than 1, then return positive zero.
                            // Else if z is smaller than 0 but greater than −1, then return negative zero.
                            // Else return the integral value with the same sign as z and the largest magnitude that is not larger than the magnitude of z.
                            //
                            // The definition of System.Math.Truncate matches this logic
                            // https://docs.microsoft.com/en-us/dotnet/api/system.math.truncate
                            // Calculates the integral part of a specified double-precision floating-point number.
                            // The integral part of is the number that remains after any fractional digits have been discarded.

                            *(float*)(&pstk[stackPos]) = (float)System.Math.Truncate(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_nearest:
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Round(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_sqrt:
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Sqrt(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_add:
                            *(float*)(&pstk[stackPos + 4]) =
                                *(float*)(&pstk[stackPos + 4]) + *(float*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_sub:
                            *(float*)(&pstk[stackPos + 4]) =
                                *(float*)(&pstk[stackPos + 4]) - *(float*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_mul:
                            *(float*)(&pstk[stackPos + 4]) =
                                *(float*)(&pstk[stackPos + 4]) * *(float*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_div:
                            *(float*)(&pstk[stackPos + 4]) =
                                *(float*)(&pstk[stackPos + 4]) / *(float*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_min:
                            {
                                // https://webassembly.github.io/spec/core/exec/numerics.html?highlight=fmin
                                //
                                // • If either 𝑧1 or 𝑧2 is a NaN, then return an element of nans𝑁 {𝑧1, 𝑧2}.
                                // • Else if one of 𝑧1 or 𝑧2 is a positive infinity, then return positive infinity.
                                // • Else if one of 𝑧1 or 𝑧2 is a negative infinity, then return the other value.
                                // • Else if both 𝑧1 and 𝑧2 are zeroes of opposite signs, then return positive zero.
                                // • Else return the larger value of 𝑧1 and 𝑧2.

                                // We're going to explicitly define the operator instead of using System.Min or 
                                // Mathf.Min (not that we woudld allow Mathf code in here anways) because the
                                // WASM spec is slightly different when dealing with NaNs and infs.

                                float a = *(float*)(&pstk[stackPos + 4]);
                                float b = *(float*)(&pstk[stackPos]);
                                float * resloc = (float*)(&pstk[stackPos + 4]);

                                if (float.IsNaN(a) == true || float.IsNaN(b) == true)
                                    *resloc = float.NaN;
                                else if (float.IsNegativeInfinity(a) == true || float.IsNegativeInfinity(b) == true)
                                    *resloc = float.NegativeInfinity;
                                else if (float.IsPositiveInfinity(a) == true)
                                    *resloc = b;
                                else if (float.IsPositiveInfinity(b) == true)
                                    *resloc = a;
                                else if (a < b)
                                    *resloc = a;
                                else
                                    *resloc = b;
                            }

                            stackPos += 4;
                            break;

                        case Instruction.f32_max:
                            {
                                // https://webassembly.github.io/spec/core/exec/numerics.html?highlight=fmin
                                //
                                // If either z1 or z2 is a NaN, then return an element of nansN{ z1,z2}.
                                // Else if one of z1 or z2 is a negative infinity, then return negative infinity.
                                // Else if one of z1 or z2 is a positive infinity, then return the other value.
                                // Else if both z1 and z2 are zeroes of opposite signs, then return negative zero.
                                // Else return the smaller value of z1 and z2.

                                // We're going to explicitly define the operator instead of using System.Min or 
                                // Mathf.Min (not that we woudld allow Mathf code in here anways) because the
                                // WASM spec is slightly different when dealing with NaNs and infs.

                                float a = *(float*)(&pstk[stackPos + 4]);
                                float b = *(float*)(&pstk[stackPos]);
                                float* resloc = (float*)(&pstk[stackPos + 4]);

                                if (float.IsNaN(a) == true || float.IsNaN(b) == true)
                                    *resloc = float.NaN;
                                else if (float.IsPositiveInfinity(a) == true || float.IsPositiveInfinity(b) == true)
                                    *resloc = float.PositiveInfinity;
                                else if (float.IsNegativeInfinity(a) == true)
                                    *resloc = b;
                                else if (float.IsNegativeInfinity(b) == true)
                                    *resloc = a;
                                else if (a > b)
                                    *resloc = a;
                                else
                                    *resloc = b;
                            }

                            stackPos += 4;
                            break;

                        case Instruction.f32_copysign:
                            {
                                // https://webassembly.github.io/spec/core/exec/numerics.html#op-fcopysign
                                //
                                // If z1 and z2 have the same sign, then return z1.
                                // Else return z1 with negated sign.
                                //
                                // This definition is missing explanation for cases involving
                                // NaN vs -Nan, and 0.0f vs -0.0f. We're going to follow what WAT2WASM
                                // returns, which seems to be performing bit-level logic with the sign -
                                // which has different conventions than what a high-level C# implementation
                                // would give, both for how the language behaves, and how the float utilities
                                // behave.

                                float csMag = *(float*)(&pstk[stackPos + 4]);
                                float csSin = *(float*)(&pstk[stackPos + 0]);

                                if (float.IsNaN(csMag) == true)
                                {
                                    // In case the platform also has a negative NaN, either
                                    // sign returns a positive NaN.
                                    *(float*)(&pstk[stackPos + 4]) = float.NaN;
                                }
                                else
                                {
                                    // For how WASM is supposed to work, it WAY easier to 
                                    // do this on a bit level with an integer type instead.
                                    uint nMag = *(uint*)(&pstk[stackPos + 4]);
                                    uint nSign = *(uint*)(&pstk[stackPos]);
                                    const uint SignMask = (uint)1 << 31;
                                    const uint MagMask = SignMask - 1;

                                    * (uint*)(&pstk[stackPos + 4]) = 
                                        (nSign & SignMask) | (nMag & MagMask);
                                }
                            }
                            stackPos += 4;
                            break;

                        case Instruction.f64_abs:
                            *(double*)(&pstk[stackPos]) =
                                System.Math.Abs(*(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_neg:

                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-fneg
                            //
                            // If z is a NaN, then return z with negated sign.
                            // Else if z is an infinity, then return that infinity negated.
                            // Else if z is a zero, then return that zero negated.
                            // Else return z negated.

                            *(double*)(&pstk[stackPos]) =
                                -*(double*)(&pstk[stackPos]);
                            break;

                        case Instruction.f64_ceil:
                            *(double*)(&pstk[stackPos]) = 
                                System.Math.Ceiling(
                                    *(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_floor:
                            *(double*)(&pstk[stackPos]) =
                                System.Math.Floor(
                                    *(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_trunc:
                            *(double*)(&pstk[stackPos]) =
                               System.Math.Truncate(
                                   *(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_nearest:
                            *(double*)(&pstk[stackPos]) =
                               System.Math.Round(
                                   *(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_sqrt:
                            // Just use the build in System.Math implementation. If it behaves
                            // non-deterministically there shall be eyebrows to raise - but it's
                            // doubtful sqrt() for IEEE754 implementations will vary.
                            *(double*)(&pstk[stackPos]) =
                                System.Math.Sqrt(
                                    *(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_add:
                            // Built in implementation
                            *(double*)(&pstk[stackPos + 8]) = 
                                *(double*)(&pstk[stackPos + 8]) + 
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_sub:
                            // Built in implementation
                            *(double*)(&pstk[stackPos + 8]) =
                                *(double*)(&pstk[stackPos + 8]) -
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_mul:
                            // Built in implementation
                            *(double*)(&pstk[stackPos + 8]) =
                                *(double*)(&pstk[stackPos + 8]) *
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_div:
                            // Built in implementation
                            *(double*)(&pstk[stackPos + 8]) =
                                *(double*)(&pstk[stackPos + 8]) /
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_min:
                            {
                                // • If either 𝑧1 or 𝑧2 is a NaN, then return an element of nans𝑁 {𝑧1, 𝑧2}.
                                // • Else if one of 𝑧1 or 𝑧2 is a positive infinity, then return positive infinity.
                                // • Else if one of 𝑧1 or 𝑧2 is a negative infinity, then return the other value.
                                // • Else if both 𝑧1 and 𝑧2 are zeroes of opposite signs, then return positive zero.
                                // • Else return the larger value of 𝑧1 and 𝑧2.

                                // We're going to explicitly define the operator instead of using System.Min 
                                // because the WASM spec is slightly different when dealing with NaNs and infs.

                                double a = *(double*)(&pstk[stackPos + 8]);
                                double b = *(double*)(&pstk[stackPos]);

                                if(double.IsNaN(a) == true || double.IsNaN(b) == true)
                                    *(double*)(&pstk[stackPos + 8]) = double.NaN;
                                else if(double.IsNegativeInfinity(a) == true || double.IsNegativeInfinity(b) == true)
                                    *(double*)(&pstk[stackPos + 8]) = double.NegativeInfinity;
                                else if(double.IsPositiveInfinity(a) == true)
                                    *(double*)(&pstk[stackPos + 8]) = b;
                                else if (double.IsPositiveInfinity(b) == true)
                                    *(double*)(&pstk[stackPos + 8]) = a;
                                else if (a < b)
                                    *(double*)(&pstk[stackPos + 8]) = a;
                                else
                                    *(double*)(&pstk[stackPos + 8]) = b;
                            }

                            stackPos += 8;
                            break;

                        case Instruction.f64_max:
                            {
                                // • If either 𝑧1 or 𝑧2 is a NaN, then return an element of nans𝑁 {𝑧1, 𝑧2}.
                                // • Else if one of 𝑧1 or 𝑧2 is a positive infinity, then return positive infinity.
                                // • Else if one of 𝑧1 or 𝑧2 is a negative infinity, then return the other value.
                                // • Else if both 𝑧1 and 𝑧2 are zeroes of opposite signs, then return positive zero.
                                // • Else return the larger value of 𝑧1 and 𝑧2.

                                double a = *(double*)(&pstk[stackPos + 8]);
                                double b = *(double*)(&pstk[stackPos]);

                                if (double.IsNaN(a) == true || double.IsNaN(b) == true)
                                    *(double*)(&pstk[stackPos + 8]) = double.NaN;
                                else if (double.IsPositiveInfinity(a) == true || double.IsPositiveInfinity(b) == true)
                                    *(double*)(&pstk[stackPos + 8]) = double.PositiveInfinity;
                                else if (double.IsNegativeInfinity(a) == true)
                                    *(double*)(&pstk[stackPos + 8]) = b;
                                else if (double.IsNegativeInfinity(b) == true)
                                    *(double*)(&pstk[stackPos + 8]) = a;
                                else if (a > b)
                                    *(double*)(&pstk[stackPos + 8]) = a;
                                else
                                    *(double*)(&pstk[stackPos + 8]) = b;
                            }

                            stackPos += 8;
                            break;

                        case Instruction.f64_copysign:
                            {
                                double csMag = *(double*)(&pstk[stackPos + 8]);
                                double csSin = *(double*)(&pstk[stackPos + 0]);

                                // This has the same issues mentioned in the comments for f32_copysign
                                if (double.IsNaN(csMag) == true)
                                {
                                    // In case the platform also has a negative NaN, either
                                    // sign returns a positive NaN.
                                    *(double*)(&pstk[stackPos + 8]) = double.NaN;
                                }
                                else
                                {
                                    // For how WASM is supposed to work, it WAY easier to 
                                    // do this on a bit level with an integer type instead.
                                    ulong nMag = *(ulong*)(&pstk[stackPos + 8]);
                                    ulong nSign = *(ulong*)(&pstk[stackPos]);

                                    const ulong SignMask = (ulong)1 << 63;
                                    const ulong MagMask = SignMask - 1;

                                    *(ulong*)(&pstk[stackPos + 8]) =
                                        (nSign & SignMask) | (nMag & MagMask);
                                }
                            }

                            stackPos += 8;
                            break;

                        case Instruction.i32_wrap_i64:
                            {
                                // TODO: Test if we can just chop off the high bytes and get the 
                                // same results without the explicit modulus.

                                ulong val = *(ulong*)(&pstk[stackPos]); 
                                stackPos += 4; // -8 to pop off the parameter, and then +4 to add the 32 bit result.
                                *(uint*)(&pstk[stackPos]) = (uint)val % 0xFFFFFFFF;
                            }
                            break;

                        case Instruction.i32_trunc_f32_s:
                            // Both 32, pop 32bits, and then pop 32 bits leaving no change in the stack.
                            *(int*)(&pstk[stackPos]) = (int)*(float*)(&pstk[stackPos]);
                            break;

                        case Instruction.i32_trunc_f32_u:
                            {
                                // Both 32, pop 32bits, and then pop 32 bits leaving no change in the stack.
                                float f = *(float*)&pstk[stackPos];

                                if(f < -1.0f)
                                    throw new System.Exception("float unrepresentable in integer range");

                                if(f < 0.0f)
                                    *(uint*)(&pstk[stackPos]) = 0;
                                else
                                    *(uint*)(&pstk[stackPos]) = (uint)f;
                            }
                            break;

                        case Instruction.i32_trunc_f64_s:
                            *(int*)(&pstk[this.stackPos + 4]) = (int)*(double*)(&pstk[stackPos]);

                            // Pop off 8 bytes, push back 4 bytes
                            this.stackPos += 4;
                            break;

                        case Instruction.i32_trunc_f64_u:
                            { 
                                double d = *(double*)&pstk[stackPos];

                                if(d <= -1.0)
                                    throw new System.Exception("float unrepresentable in integer range");

                                this.stackPos += 4;

                                if(d <= 0.0)
                                    *(uint*)&pstk[stackPos] = (uint)0;
                                else
                                    *(uint*)&pstk[stackPos] = (uint)d;

                                // Pop off 8 bytes, push back 4 bytes
                            }
                            break;

                        case Instruction.i64_extend_i32_s:
                            *(long*)&pstk[this.stackPos - 4] = *(int*)&pstk[stackPos];
                            this.stackPos -= 4; // pop 4 bytes, push 8 bytes
                            break;

                        case Instruction.i64_extend_i32_u:
                            *(ulong*)&pstk[this.stackPos - 4] = *(uint*)&pstk[stackPos];
                            this.stackPos -= 4; // pop 4 bytes, push 8 bytes
                            break;

                        case Instruction.i64_trunc_f32_s:
                            *(long*)&pstk[this.stackPos - 4] = (long)*(float*)&pstk[stackPos];
                            this.stackPos -= 4;
                            break;

                        case Instruction.i64_trunc_f32_u:
                            {
                                float f = (long)*(float*)&pstk[stackPos];

                                if(f <= -1.0f)
                                    throw new System.Exception("float unrepresentable in integer range");

                                this.stackPos -= 4; // pop 4 bytes, push 8 bytes

                                if(f <= 0.0f)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else
                                    *(ulong*)&pstk[this.stackPos] = (ulong)f;
                            }
                            break;

                        case Instruction.i64_trunc_f64_s:
                            *(long*)&pstk[this.stackPos] = (long)*(double*)&pstk[stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction.i64_trunc_f64_u:
                            {
                                double d = *(double*)&pstk[stackPos];
                                
                                if(d <= -1.0)
                                    throw new System.Exception("float urepresentable in integer range");

                                if(d <= 0.0)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else
                                    *(ulong*)&pstk[this.stackPos] = (ulong)d;
                                // Pop 8 bytes, pushed 8 bytes. No stack change
                            }
                            break;

                        case Instruction.f32_convert_i32_s:
                            *(float*)&pstk[this.stackPos] = *(int*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction.f32_convert_i32_u:
                            *(float*)&pstk[this.stackPos] = *(uint*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction.f32_convert_i64_s:
                            *(float*)&pstk[this.stackPos + 4] = *(long*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 4 bytes.
                            this.stackPos += 4;
                            break;

                        case Instruction.f32_convert_i64_u:
                            *(float*)&pstk[this.stackPos + 4] = *(ulong*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 4 bytes.
                            this.stackPos += 4;
                            break;

                        case Instruction.f32_demote_f64:
                            *(float*)&pstk[this.stackPos + 4] = (float)*(double*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 4 bytes.
                            this.stackPos += 4;
                            break;

                        case Instruction.f64_convert_i32_s:
                            *(double*)&pstk[this.stackPos - 4] = *(int*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 8 bytes.
                            this.stackPos -= 4;
                            break;

                        case Instruction.f64_convert_i32_u:
                            *(double*)&pstk[this.stackPos - 4] = *(uint*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 8 bytes.
                            this.stackPos -= 4;
                            break;

                        case Instruction.f64_convert_i64_s:
                            *(double*)&pstk[this.stackPos] = *(long*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction.f64_convert_i64_u:
                            *(double*)&pstk[this.stackPos] = *(ulong*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction.f64_promote_f32:
                            *(double*)&pstk[this.stackPos - 4] = *(float*)&pstk[this.stackPos];
                            stackPos -= 4;
                            // Pop 8 bytes, pushed 4 bytes.
                            break;

                        //case Instruction.i32_reinterpret_f32:
                        //    break;
                        //
                        //case Instruction.i64_reinterpret_f64:
                        //    break;
                        //
                        //case Instruction.f32_reinterpret_i32:
                        //    break;
                        //
                        //case Instruction.f64_reinterpret_i64:
                        //    break;

                        case Instruction.i32_extend8_s:
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            *(int*)&pstk[this.stackPos] = *(sbyte*)&pstk[this.stackPos];
                            break;

                        case Instruction.i32_extend16_s:
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            *(int*)&pstk[this.stackPos] = *(short*)&pstk[this.stackPos];
                            break;

                        case Instruction.i64_extend8_s:
                            *(long*)&pstk[this.stackPos] = *(sbyte*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction.i64_extend16_s:
                            *(long*)&pstk[this.stackPos] = *(short*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction.i64_extend32_s:
                            *(long*)&pstk[this.stackPos] = *(int*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction._i32_trunc_sat_f32_s:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-s
                            // 
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return −2N−1.
                            // Else if z is positive infinity, then return 2N−1−1.
                            // Else if trunc(z) is less than −2N−1, then return −2N−1.
                            // Else if trunc(z) is greater than 2N−1−1, then return 2N−1−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                float fval = *(float*)&pstk[this.stackPos];

                                if(float.IsNaN(fval) == true)
                                    *(int*)&pstk[this.stackPos] = 0;
                                else if(float.IsNegativeInfinity(fval) == true)
                                    *(int*)&pstk[this.stackPos] = int.MinValue;
                                else if(float.IsPositiveInfinity(fval) == true)
                                    *(int*)&pstk[this.stackPos] = int.MaxValue;
                                else if(fval < int.MinValue)
                                    *(int*)&pstk[this.stackPos] = int.MinValue;
                                else if(fval > int.MaxValue)
                                    *(int*)&pstk[this.stackPos] = int.MaxValue;
                                else
                                    *(int*)&pstk[this.stackPos] = (int)fval;
                            }
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction._i32_trunc_sat_f32_u:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-u
                            //
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return 0.
                            // Else if z is positive infinity, then return 2N−1.
                            // Else if trunc(z) is less than 0, then return 0.
                            // Else if trunc(z) is greater than 2N−1, then return 2N−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                float fval = *(float*)&pstk[this.stackPos];

                                if(float.IsNaN(fval) == true)
                                    *(uint*)&pstk[this.stackPos] = 0;
                                else if(float.IsNegativeInfinity(fval) == true)
                                    *(uint*)&pstk[this.stackPos] = 0;
                                else if(float.IsPositiveInfinity(fval) == true)
                                    *(uint*)&pstk[this.stackPos] = uint.MaxValue;
                                else if(fval < 0.0f)
                                    *(uint*)&pstk[this.stackPos] = 0;
                                else if(fval > uint.MaxValue)
                                    *(uint*)&pstk[this.stackPos] = uint.MaxValue;
                                else
                                    *(uint*)&pstk[this.stackPos] = (uint)fval;
                            }
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction._i32_trunc_sat_f64_s:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-s
                            // 
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return −2N−1.
                            // Else if z is positive infinity, then return 2N−1−1.
                            // Else if trunc(z) is less than −2N−1, then return −2N−1.
                            // Else if trunc(z) is greater than 2N−1−1, then return 2N−1−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                double dval = *(double*)&pstk[this.stackPos];
                                this.stackPos += 4; // Pop 8 bytes, pushed 4 bytes.

                                if(double.IsNaN(dval) == true)
                                    *(int*)&pstk[this.stackPos] = 0;
                                else if(double.IsNegativeInfinity(dval) == true)
                                    *(int*)&pstk[this.stackPos] = int.MinValue;
                                else if(double.IsPositiveInfinity(dval) == true)
                                    *(int*)&pstk[this.stackPos] = int.MaxValue;
                                else if(dval < int.MinValue)
                                    *(int*)&pstk[this.stackPos] = int.MinValue;
                                else if(dval > int.MaxValue)
                                    *(int*)&pstk[this.stackPos] = int.MaxValue;
                                else
                                    *(int*)&pstk[this.stackPos] = (int)dval;
                            }
                            break;

                        case Instruction._i32_trunc_sat_f64_u:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-u
                            //
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return 0.
                            // Else if z is positive infinity, then return 2N−1.
                            // Else if trunc(z) is less than 0, then return 0.
                            // Else if trunc(z) is greater than 2N−1, then return 2N−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                double dval = *(double*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                if(double.IsNaN(dval) == true)
                                    *(uint*)&pstk[this.stackPos] = 0;
                                else if(double.IsNegativeInfinity(dval) == true)
                                    *(uint*)&pstk[this.stackPos] = 0;
                                else if (double.IsPositiveInfinity(dval) == true)
                                    *(uint*)&pstk[this.stackPos] = uint.MaxValue;
                                else if(dval < 0.0)
                                    *(uint*)&pstk[this.stackPos] = 0;
                                else if(dval > uint.MaxValue)
                                    *(uint*)&pstk[this.stackPos] = uint.MaxValue;
                                else
                                    *(uint*)&pstk[this.stackPos] = (uint)dval;
                            }
                            // Pop 8 bytes, pushed 4 bytes.
                            break;

                        case Instruction._i64_trunc_sat_f32_s:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-s
                            // 
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return −2N−1.
                            // Else if z is positive infinity, then return 2N−1−1.
                            // Else if trunc(z) is less than −2N−1, then return −2N−1.
                            // Else if trunc(z) is greater than 2N−1−1, then return 2N−1−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                float fval = *(float*)&pstk[this.stackPos];
                                this.stackPos -= 4; // Pop 4 bytes, pushed 8 bytes.

                                if(float.IsNaN(fval) == true)
                                    *(long*)&pstk[this.stackPos] = 0;
                                else if(float.IsNegativeInfinity(fval) == true)
                                    *(long*)&pstk[this.stackPos] = long.MinValue;
                                else if(float.IsPositiveInfinity(fval) == true)
                                    *(long*)&pstk[this.stackPos] = long.MaxValue;
                                else if(fval < long.MinValue)
                                    *(long*)&pstk[this.stackPos] = long.MinValue;
                                else if(fval > long.MaxValue)
                                    *(long*)&pstk[this.stackPos] = long.MaxValue;
                                else
                                    *(long*)&pstk[this.stackPos] = (long)fval;
                            }
                            break;

                        case Instruction._i64_trunc_sat_f32_u:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-u
                            //
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return 0.
                            // Else if z is positive infinity, then return 2N−1.
                            // Else if trunc(z) is less than 0, then return 0.
                            // Else if trunc(z) is greater than 2N−1, then return 2N−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                float fval = *(float*)&pstk[this.stackPos];
                                this.stackPos -= 4; // Pop 4 bytes, pushed 8 bytes.

                                if (float.IsNaN(fval) == true)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else if(float.IsNegativeInfinity(fval) == true)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else if(float.IsPositiveInfinity(fval) == true)
                                    *(ulong*)&pstk[this.stackPos] = ulong.MaxValue;
                                else if(fval < 0.0f)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else if (fval > ulong.MaxValue)
                                    *(ulong*)&pstk[this.stackPos] = ulong.MaxValue;
                                else 
                                    *(ulong*)&pstk[this.stackPos] = (ulong)fval;

                            }
                            break;

                        case Instruction._i64_trunc_sat_f64_s:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-s
                            // 
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return −2N−1.
                            // Else if z is positive infinity, then return 2N−1−1.
                            // Else if trunc(z) is less than −2N−1, then return −2N−1.
                            // Else if trunc(z) is greater than 2N−1−1, then return 2N−1−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                double dval = *(double*)&pstk[this.stackPos];
                                // Pop 8 bytes, pushed 8 bytes. No stack change

                                if(double.IsNaN(dval) == true)
                                    *(long*)&pstk[this.stackPos] = 0;
                                else if(double.IsNegativeInfinity(dval) == true)
                                    *(long*)&pstk[this.stackPos] = long.MinValue;
                                else if(double.IsPositiveInfinity(dval) == true)
                                    *(long*)&pstk[this.stackPos] = long.MaxValue;
                                else if(dval < long.MinValue)
                                    *(long*)&pstk[this.stackPos] = long.MinValue;
                                else if (dval > long.MaxValue)
                                    *(long*)&pstk[this.stackPos] = long.MaxValue;
                                else
                                    *(long*)&pstk[this.stackPos] = (long)dval;
                            }
                            break;

                        case Instruction._i64_trunc_sat_f64_u:
                            // Saturating Truncate. A float to int operator that has rules
                            // for handling/converting floating point errors and codes.
                            // 
                            // https://webassembly.github.io/spec/core/exec/numerics.html#op-trunc-sat-u
                            //
                            // If z is a NaN, then return 0.
                            // Else if z is negative infinity, then return 0.
                            // Else if z is positive infinity, then return 2N−1.
                            // Else if trunc(z) is less than 0, then return 0.
                            // Else if trunc(z) is greater than 2N−1, then return 2N−1.
                            // Else, return trunc(z).
                            //
                            // I don't think there's a C# convention (or even an x86 op) for this.
                            // We're going to implement this by hand.
                            {
                                double dval = *(double*)&pstk[this.stackPos];
                                // Pop 8 bytes, pushed 8 bytes. No stack change

                                if(double.IsNaN(dval) == true)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else if(double.IsNegativeInfinity(dval) == true)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else if(double.IsPositiveInfinity(dval) == true)
                                    *(ulong*)&pstk[this.stackPos] = ulong.MaxValue;
                                else if(dval < 0.0)
                                    *(ulong*)&pstk[this.stackPos] = 0;
                                else if(dval > ulong.MaxValue)
                                    *(ulong*)&pstk[this.stackPos] = ulong.MaxValue;
                                else
                                    *(ulong*)&pstk[this.stackPos] = (ulong)dval;
                            }
                            break;

                        case Instruction._memory_fill:
                            {
                                // Bulk memory extension
                                // https://github.com/WebAssembly/bulk-memory-operations/blob/master/proposals/bulk-memory-operations/Overview.md
                                //
                                // While bulk memory is not functionality target for PreWASM, it was
                                // featured as a demo in WAT2WASM so it was simple enough to support.
                                //
                                // Other bulk memory features may be added later, but there are no 
                                // details if and when.

                                ip += 4; // Skip unused parameter

                                uint start  = *(uint*)&pstk[this.stackPos + 8];
                                uint val    = *(uint*)&pstk[this.stackPos + 4];
                                uint count  = *(uint*)&pstk[this.stackPos + 0];
                                this.stackPos += 12; 

                                for(uint i = 0; i < count; ++i)
                                    pbMem[start + i] = (byte)val;
                            }
                            break;

                    }
                }
            }
        }


        public bool InvokeStart(bool allowRecall = false)
        { 
            if(this.initialized == true && allowRecall == false)
                return false;

            if(this.instancer.startFnIndex == 0xFFFFFFFF)
            { 
                this.initialized = true;
                return true;
            }

            IndexEntry fie = this.instancer.storeDecl.IndexingFunction[(int)this.instancer.startFnIndex];

            if(fie.type == IndexEntry.FnIdxType.Local)
            {
                Function startFn = this.instancer.functions[fie.index];

                // TODO: Check this earlier during compile
                if (startFn.fnType.resultTypes.Count > 0 || startFn.fnType.paramTypes.Count > 0)
                    throw new System.Exception("Start function is invalid function type."); // TODO: Error message

                this.Invoke(startFn);
            }
            else if(fie.type == IndexEntry.FnIdxType.Import)
            { 
                ImportFunction startIFn = this.importData.importFn[fie.index];
                if(startIFn == null)
                    throw new System.Exception("Imported start function not set.");

                // TODO: Check this earlier during compile
                if(startIFn.functionType.resultTypes.Count > 0 || startIFn.functionType.paramTypes.Count > 0)
                    throw new System.Exception("Start function is invalid function type.");

                ImportFunctionUtil ifn = new ImportFunctionUtil(startIFn.functionType, this, this.stackPos);
                startIFn.InvokeImpl(ifn);
            }
            else
                throw new System.Exception(); // TODO: Error message

            this.initialized = false;

            return true;
        }

        public int GetExportedFunctionIndex(string name)
        { 
            return this.instancer.GetExportedFunctionID(name);
        }

        public Function GetExportedFunction(string name)
        { 
            return this.instancer.GetExportedFunction(name);
        }

        public int GetExportedGlobalIndex(string name)
        {
            foreach (Export e in this.instancer.exports)
            {
                if (e.kind != Module.ImportType.GlobalType)
                    continue;

                if (e.name == name)
                    return (int)e.index;
            }
            return -1;
        }

        public Global GetExportedGlobal(string name)
        { 
            int glid = this.GetExportedGlobalIndex(name);

            if(glid == -1)
                return null;

            return this.globals[glid];
        }

        public int GetExportedTableIndex(string name)
        {
            foreach (Export e in this.instancer.exports)
            {
                if (e.kind != Module.ImportType.TableType)
                    continue;

                if (e.name == name)
                    return (int)e.index;
            }
            return -1;
        }

        public Table GetExportedTable(string name)
        { 
            int tbid = this.GetExportedTableIndex(name);

            if(tbid == -1)
                return null;

            return this.tables[tbid];
        }

        public int GetExportedMemoryIndex(string name)
        {
            foreach (Export e in this.instancer.exports)
            {
                if (e.kind != Module.ImportType.MemType)
                    continue;

                if (e.name == name)
                    return (int)e.index;
            }
            return -1;
        }

        public Memory GetExportedMemory(string name)
        { 
            int meid = this.GetExportedMemoryIndex(name);

            if(meid == -1)
                return null;

            return this.memories[meid];
        }
    }
}