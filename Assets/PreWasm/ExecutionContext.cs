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

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExecutionContext()
        { }

        unsafe public void RunFunction(Module module, int index)
        { 
            FunctionIndexEntry fie = module.functionIndexing[index];
            if(fie.type == FunctionIndexEntry.FnIdxType.Local)
            { 
                this.RunLocalFunction(module, fie.index);
            }
            else if(fie.type == FunctionIndexEntry.FnIdxType.Import)
            { 
                this.RunFunction( module.importedFunctions[fie.index]);
            }
            else
                throw new System.Exception(); // TODO: Error msg
        }

        public void RunFunction(ImportFunction ifn)
        { 
            
            ImportFunctionUtil ifu = 
                new ImportFunctionUtil(ifn.functionType, this, this.stackPos);

            ifn.InvokeImpl(ifu);
        }

        public void RunFunction(ImportModule.FunctionImportEntry fie)
        { 
            if(fie.importFn == null)
                throw new System.Exception(); // TODO: Error msg

            this.RunFunction(fie.importFn);
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

            fixed(byte * pb = fn.expression, pstk = this.stack)
            {
                while(true)
                { 
                    Instruction instr = (Instruction)(int)*(ushort*)&pb[ip];
                    ip += 2;

                    switch(instr)
                    {
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

                                RunFunction(fn.parentModule.importedFunctions[(int)fnid]);
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

                        case Instruction._global_get32:
                            break;

                        case Instruction._global_get64:
                            break;

                        case Instruction._global_set32:
                            break;

                        case Instruction._global_set64:
                            break;

                        case Instruction.i32_load:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = *(int*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load:
                            { 
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(long*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.f32_load:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(float*)&pstk[this.stackPos] = *(float*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.f64_load:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(double*)&pstk[this.stackPos] = *(double*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i32_load8_s:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = (sbyte)fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i32_load8_u:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i32_load16_s:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(int*)&pstk[this.stackPos] = *(short*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i32_load16_u:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                // Pop the memory location (+4) but then allocate 32 bits (-4) - no stack position change
                                *(uint*)&pstk[this.stackPos] = *(ushort*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load8_s:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(sbyte*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load8_u:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = (byte)fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load16_s:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(short*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load16_u:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = *(ushort*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load32_s:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(long*)&pstk[this.stackPos] = *(int*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i64_load32_u:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos -= 4;  // Pop the memory location (+4) but then allocate 64 bits (-8)
                                *(ulong*)&pstk[this.stackPos] = *(uint*)&fn.parentModule.memories[0].pdata[memid];
                            }
                            break;

                        case Instruction.i32_store:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(int*)fn.parentModule.memories[0].pdata[memid] = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.i64_store:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(long*)fn.parentModule.memories[0].pdata[memid] = *(long*)&pstk[this.stackPos];
                                this.stackPos += 8;
                            }
                            break;

                        case Instruction.f32_store:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(float*)fn.parentModule.memories[0].pdata[memid] = *(float*)&pstk[this.stackPos];
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.f64_store:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(double*)fn.parentModule.memories[0].pdata[memid] = *(double*)&pstk[this.stackPos];
                                this.stackPos += 8;
                            }
                            break;

                        case Instruction.i32_store8:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                fn.parentModule.memories[0].pdata[memid] = pstk[this.stackPos]; // Not known if correct
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.i32_store16:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(uint*)fn.parentModule.memories[0].pdata[memid] = *(ushort*)&pstk[this.stackPos];
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.i64_store8:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                fn.parentModule.memories[0].pdata[memid] = pstk[this.stackPos];
                                this.stackPos += 8;
                            }
                            break;

                        case Instruction.i64_store16:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(ushort*)&fn.parentModule.memories[0].pdata[memid] = *(ushort*)&pstk[this.stackPos];
                                this.stackPos += 8;
                            }
                            break;

                        case Instruction.i64_store32:
                            {
                                int memid = *(int*)&pstk[this.stackPos];
                                this.stackPos += 4;

                                *(uint*)&fn.parentModule.memories[0].pdata[memid] = *(uint*)&pstk[this.stackPos];
                                this.stackPos += 4;
                            }
                            break;

                        case Instruction.MemorySize:
                            {

                                // memory.size comes with an additional unused parameter (+4)
                                // But we also push a return value as a 32 bit int (-4)
                                //this.stackPos += 0; 

                                if (fn.parentModule.memories.Count == 0 || fn.parentModule.memories[0].CalculatePageCt() == 0)
                                    *(int*)pstk[this.stackPos] = -1;
                                else
                                {
                                    // The return is a page size, and we're following the rule that the memory size is always
                                    // a multiple of the page size.
                                    *(uint*)pstk[this.stackPos] = fn.parentModule.memories[0].CalculatePageCt();
                                }
                            }
                            break;

                        case Instruction.MemoryGrow:
                            { 
                                int newPages = *(int*)&pstk[this.stackPos];

                                // I don't even know if we should allow adding a memory if one wasn't
                                // defined. While we're trying to gracefully what's arguably an error
                                // condition, we may need to just throw an exception/trap.
                                if (fn.parentModule.memories.Count == 0)
                                    fn.parentModule.memories.Add(new Memory(0, 0, 1));

                                // The stackpop is popped, but another 32 bit values is put on the stack. No stack modification.
                                uint oldPageSz = fn.parentModule.memories[0].CalculatePageCt();

                                DataStore.ExpandRet expRet = fn.parentModule.memories[0].ExpandPageCt((int)(oldPageSz + newPages));

                                // NOTE: This probably isn't to-spec.
                                if(expRet == DataStore.ExpandRet.Successful)
                                    *(uint*)&pstk[this.stackPos] = oldPageSz;
                                else
                                    *(int*)&pstk[this.stackPos] = -1;
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
                                bool b = *(long*)(&pstk[stackPos]) == 0;
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_eq:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) >= *(long*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_ne:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) != *(long*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_lt_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) >= *(long*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_lt_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) < *(ulong*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_gt_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) > *(long*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_gt_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) > *(ulong*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_le_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) <= *(long*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_le_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) <= *(ulong*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_ge_s:
                            {
                                bool b = *(long*)(&pstk[stackPos + 8]) >= *(long*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.i64_ge_u:
                            {
                                bool b = *(ulong*)(&pstk[stackPos + 8]) >= *(ulong*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_eq:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) == *(float*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_ne:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) != *(float*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_lt:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) < *(float*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_gt:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) > *(float*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_le:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) <= *(float*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f32_ge:
                            {
                                bool b = *(float*)(&pstk[stackPos + 4]) >= *(float*)(&pstk[stackPos]);
                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_eq:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) == *(double*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_ne:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) != *(double*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_lt:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) < *(double*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_gt:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) > *(double*)(&pstk[stackPos]);
                                stackPos += 12;
                                *(int*)(&pstk[stackPos]) = b ? 1 : 0;
                            }
                            break;

                        case Instruction.f64_le:
                            {
                                bool b = *(double*)(&pstk[stackPos + 8]) <= *(double*)(&pstk[stackPos]);
                                stackPos += 12;
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

                            stackPos += 4;
                            break;

                        case Instruction.i32_sub:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) - *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_mul:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) * *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_div_s:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) / *(int*)(&pstk[stackPos]);
                            break;

                        case Instruction.i32_div_u:
                            *(uint*)(&pstk[stackPos + 4]) =
                                *(uint*)(&pstk[stackPos + 4]) / *(uint*)(&pstk[stackPos]);
                            break;

                        case Instruction.i32_rem_s:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) % *(int*)(&pstk[stackPos]);
                            break;

                        case Instruction.i32_rem_u:
                            *(uint*)(&pstk[stackPos + 4]) =
                                *(uint*)(&pstk[stackPos + 4]) % *(uint*)(&pstk[stackPos]);
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
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) << *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_shr_s:
                            // Right shift, but bit extend the most significant value
                            {
                                int val = *(int*)(&pstk[stackPos + 4]);
                                bool hibit = ((1 << 31) & val) != 0;

                                int shiftAmt = *(int*)(&pstk[stackPos]);
                                if (hibit == false)
                                {
                                    *(int*)(&pstk[stackPos + 4]) = val >> shiftAmt;
                                }
                                else
                                {
                                    *(int*)(&pstk[stackPos + 4]) = 
                                        (val >> shiftAmt) | ((~0) << (32 - shiftAmt));
                                }

                                stackPos += 4;
                            }
                            break;

                        case Instruction.i32_shr_u:
                            *(int*)(&pstk[stackPos + 4]) =
                                *(int*)(&pstk[stackPos + 4]) >> *(int*)(&pstk[stackPos]);

                            stackPos += 4;
                            break;

                        case Instruction.i32_rotl:
                            {
                                int val = *(int*)(&pstk[stackPos + 4]);
                                int shiftAmt = *(int*)(&pstk[stackPos]);

                                int wrap = val >> (32 - shiftAmt);
                                int pushed = val << shiftAmt;

                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = wrap | pushed;
                            }
                            break;

                        case Instruction.i32_rotr:
                            {
                                int val = *(int*)(&pstk[stackPos + 4]);
                                int shiftAmt = *(int*)(&pstk[stackPos]);

                                int wrap = val << (32 - shiftAmt);
                                int pushed = val >> shiftAmt;

                                stackPos += 4;
                                *(int*)(&pstk[stackPos]) = wrap | pushed;
                            }
                            break;

                        case Instruction.i64_clz:
                            {
                                // Count leading zero bits
                                long topi = *(long*)(&pstk[stackPos]);
                                if (topi == 0)
                                    *(long*)(&pstk[stackPos]) = 64;
                                else
                                {
                                    for (int i = 0; i < 64; ++i)
                                    {
                                        if ((topi & ((1 << 63) >> i)) != 0)
                                        {
                                            *(long*)(&pstk[stackPos]) = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case Instruction.i64_ctz:
                            {
                                // Count trailing zero bits
                                long topi = *(long*)(&pstk[stackPos]);
                                if (topi == 0)
                                    *(long*)(&pstk[stackPos]) = 64;
                                else
                                {
                                    for (int i = 0; i < 64; ++i)
                                    {
                                        if ((topi & (1 << i)) != 0)
                                        {
                                            *(long*)(&pstk[stackPos]) = i;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case Instruction.i64_popcnt:
                            {
                                // Count non-zero bits
                                long topi = *(long*)(&pstk[stackPos]);
                                long v = 0;
                                for (int i = 0; i < 64; ++i)
                                {
                                    if ((topi & (1 << i)) != 0)
                                        ++v;
                                }
                                *(long*)(&pstk[stackPos]) = v;
                            }
                            break;

                        case Instruction.i64_add:
                            {
                                *(long*)(&pstk[stackPos + 8]) =
                                    *(long*)(&pstk[stackPos + 8]) + *(long*)(&pstk[stackPos]);

                                stackPos += 8;
                            }
                            break;

                        case Instruction.i64_sub:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) - *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_mul:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) * *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_div_s:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) / *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_div_u:
                            *(ulong*)(&pstk[stackPos + 8]) =
                                *(ulong*)(&pstk[stackPos + 8]) / *(ulong*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_rem_s:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) % *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_rem_u:
                            *(ulong*)(&pstk[stackPos + 8]) =
                                *(ulong*)(&pstk[stackPos + 8]) % *(ulong*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_and:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) & *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_or:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) | *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_xor:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) ^ *(long*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_shl:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) << *(int*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_shr_s:
                            {
                                long val = *(long*)(&pstk[stackPos + 8]);
                                long shift = *(int*)(&pstk[stackPos]);

                                bool sign = (val << (1 & 63)) != 0;

                                if(sign == false)
                                    * (long*)(&pstk[stackPos + 8]) = val << (int)shift;
                                else
                                    *(long*)(&pstk[stackPos + 8]) = (val << (int)shift) & (~(long)0 << (64 - (int)shift));

                                stackPos += 8;
                            }
                            break;

                        case Instruction.i64_shr_u:
                            *(long*)(&pstk[stackPos + 8]) =
                                *(long*)(&pstk[stackPos + 8]) >> *(int*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.i64_rotl:
                            {
                                long val = *(long*)(&pstk[stackPos + 8]);
                                long shift = *(int*)(&pstk[stackPos]);

                                long push = val << (int)shift;
                                long wrap = val >> (int)(64 - shift);

                                stackPos += 8;
                                *(long*)(&pstk[stackPos]) = push | wrap;
                            }
                            break;

                        case Instruction.i64_rotr:
                            {
                                {
                                    long val = *(long*)(&pstk[stackPos + 8]);
                                    long shift = *(int*)(&pstk[stackPos]);

                                    long push = val >> (int)shift;
                                    long wrap = val << (int)(64 - shift);

                                    stackPos += 8;
                                    *(long*)(&pstk[stackPos]) = push | wrap;
                                }
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
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Floor(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_trunc:
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Truncate(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_nearest:
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Round(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_sqrt:
                            *(float*)(&pstk[stackPos]) = (float)System.Math.Sqrt(*(float*)(&pstk[stackPos]));
                            break;

                        case Instruction.f32_add:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) - *(float*)(&pstk[stackPos + 4]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_sub:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) - *(float*)(&pstk[stackPos + 4]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_mul:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) * *(float*)(&pstk[stackPos + 4]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_div:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) / *(float*)(&pstk[stackPos + 4]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_min:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) < *(float*)(&pstk[stackPos + 4]) ?
                                    *(float*)(&pstk[stackPos]) :
                                    *(float*)(&pstk[stackPos + 4]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_max:
                            *(float*)(&pstk[stackPos]) =
                                *(float*)(&pstk[stackPos]) > *(float*)(&pstk[stackPos + 4]) ?
                                    *(float*)(&pstk[stackPos]) :
                                    *(float*)(&pstk[stackPos + 4]);

                            stackPos += 4;
                            break;

                        case Instruction.f32_copysign:
                            float csMag = 
                                *(float*)(&pstk[stackPos]) >= 0.0 ? 
                                    *(float*)(&pstk[stackPos]) : 
                                    -*(float*)(&pstk[stackPos]);

                            *(float*)(&pstk[stackPos + 4]) =
                                *(float*)(&pstk[stackPos + 4]) > 0.0 ?
                                    csMag : 
                                    -csMag;

                            stackPos += 4;
                            break;

                        case Instruction.f64_abs:
                            *(double*)(&pstk[stackPos]) =
                                System.Math.Abs(*(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_neg:
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
                            *(double*)(&pstk[stackPos]) =
                                System.Math.Sqrt(
                                    *(double*)(&pstk[stackPos]));
                            break;

                        case Instruction.f64_add:
                            *(double*)(&pstk[stackPos + 8]) = 
                                *(double*)(&pstk[stackPos + 8]) + 
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_sub:
                            *(double*)(&pstk[stackPos + 8]) =
                                *(double*)(&pstk[stackPos + 8]) -
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_mul:
                            *(double*)(&pstk[stackPos + 8]) =
                                *(double*)(&pstk[stackPos + 8]) *
                                *(double*)(&pstk[stackPos]);

                            stackPos += 8;
                            break;

                        case Instruction.f64_div:
                            *(double*)(&pstk[stackPos + 8]) =
                                *(double*)(&pstk[stackPos + 8]) /
                                *(double*)(&pstk[stackPos]);

                            stackPos -= 8;
                            break;

                        case Instruction.f64_min:
                            *(double*)(&pstk[stackPos + 8]) =
                                System.Math.Min(
                                    *(double*)(&pstk[stackPos + 8]),
                                    *(double*)(&pstk[stackPos]));

                            stackPos -= 8;
                            break;

                        case Instruction.f64_max:
                            *(double*)(&pstk[stackPos + 8]) =
                                System.Math.Max(
                                    *(double*)(&pstk[stackPos + 8]),
                                    *(double*)(&pstk[stackPos]));

                            stackPos -= 8;
                            break;

                        case Instruction.f64_copysign:
                            *(double*)(&pstk[stackPos + 8]) =
                                    System.Math.Abs(*(double*)(&pstk[stackPos + 8])) * 
                                    System.Math.Sign(*(double*)(&pstk[stackPos]));

                            stackPos -= 8;
                            break;

                        case Instruction.i32_wrap_i64:
                            {
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

                                if(f < 0.0f)
                                    throw new System.Exception("RuntimeError: float unrepresentable in integer range");

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

                                if(d < 0.0f)
                                    throw new System.Exception("RuntimeError: float unrepresentable in integer range");

                                this.stackPos += 4;
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

                                if(f < 0.0f)
                                    throw new System.Exception("RuntimeError: float unrepresentable in integer range");

                                this.stackPos -= 4; // pop 4 bytes, push 8 bytes
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
                                
                                if(d < 0.0)
                                    throw new System.Exception("RuntimeError: float urepresentable in integer range");

                                // Pop 8 bytes, pushed 8 bytes. No stack change
                                *(ulong*)&pstk[this.stackPos] = (ulong)d;
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

                        case Instruction.f32_convert_f64:
                            *(float*)&pstk[this.stackPos + 4] = (float)*(double*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 4 bytes.
                            this.stackPos += 4;
                            break;

                        case Instruction.f64_convert_i32_s:
                            *(double*)&pstk[this.stackPos - 4] = *(int*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 8 bytes.
                            this.stackPos += 4;
                            break;

                        case Instruction.f64_convert_i32_u:
                            *(double*)&pstk[this.stackPos - 4] = *(int*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 8 bytes.
                            this.stackPos -= 4;
                            break;

                        case Instruction.f64_convert_i64_s:
                            *(double*)&pstk[this.stackPos] = (float)*(double*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction.f64_convert_i64_u:
                            *(double*)&pstk[this.stackPos] = *(ulong*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction.f64_promote_f32:
                            *(double*)&pstk[this.stackPos - 4] = *(float*)&pstk[this.stackPos];
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
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction.i64_extend16_s:
                            *(long*)&pstk[this.stackPos] = *(short*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction.i64_extend32_s:
                            *(long*)&pstk[this.stackPos] = *(int*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction._i32_trunc_sat_f32_s:
                            *(int*)&pstk[this.stackPos] = (int)*(float*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction._i32_trunc_sat_f32_u:
                            *(uint*)&pstk[this.stackPos] = (uint)*(float*)&pstk[this.stackPos];
                            // Pop 4 bytes, pushed 4 bytes. No stack change
                            break;

                        case Instruction._i32_trunc_sat_f64_s:
                            *(int*)&pstk[this.stackPos + 4] = (int)*(double*)&pstk[this.stackPos];
                            this.stackPos += 4;
                            // Pop 8 bytes, pushed 4 bytes.
                            break;

                        case Instruction._i32_trunc_sat_f64_u:
                            *(uint*)&pstk[this.stackPos + 4] = (uint)*(double*)&pstk[this.stackPos];
                            this.stackPos += 4;
                            // Pop 8 bytes, pushed 4 bytes.
                            break;

                        case Instruction._i64_trunc_sat_f32_s:
                            *(long*)&pstk[this.stackPos - 4] = (int)*(float*)&pstk[this.stackPos];
                            this.stackPos -= 4;
                            // Pop 4 bytes, pushed 8 bytes.
                            break;

                        case Instruction._i64_trunc_sat_f32_u:
                            *(ulong*)&pstk[this.stackPos - 4] = (ulong)*(float*)&pstk[this.stackPos];
                            this.stackPos -= 4;
                            // Pop 4 bytes, pushed 8 bytes.
                            break;

                        case Instruction._i64_trunc_sat_f64_s:
                            *(long*)&pstk[this.stackPos] = (long)*(double*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction._i64_trunc_sat_f64_u:
                            *(ulong*)&pstk[this.stackPos] = (ulong)*(double*)&pstk[this.stackPos];
                            // Pop 8 bytes, pushed 8 bytes. No stack change
                            break;

                        case Instruction._memory_fill:
                            {
                                ip += 4; // Skip unused parameter

                                uint start  = *(uint*)&pstk[this.stackPos + 0];
                                uint val    = *(uint*)&pstk[this.stackPos + 4];
                                uint count  = *(uint*)&pstk[this.stackPos + 8];
                                this.stackPos += 12;

                                for(uint i = 0; i < count; ++i)
                                    ((uint*)fn.parentModule.memories[0].pdata)[start + i] = val;
                            }
                            break;

                    }
                }
            }
        }

        /// <summary>
        /// Call to run the mo
        /// </summary>
        /// <returns></returns>
        public bool InvokeStart(Module module, bool recursive = true)
        { 
            HashSet<Module> ignoreList = new HashSet<Module>();
            InvokeStart(module, ignoreList, recursive);
            return true;
        }

        public bool InvokeStart(Module module, HashSet<Module> ignore, bool recursive = true)
        { 
            if(ignore.Contains(module) == true)
                return false;

            ignore.Add(module);
            if(module.startFnIndex == 0xFFFFFFFF)
                return false;

            if(recursive == true)
            { 
                // TODO: Call InvokeStart() on dependent modules.
            }

            FunctionIndexEntry fie = module.functionIndexing[(int)module.startFnIndex];

            if(fie.type == FunctionIndexEntry.FnIdxType.Local)
            {
                Function startFn = module.functions[fie.index];

                // TODO: Check this earlier during compile
                if (startFn.fnType.resultTypes.Count > 0 || startFn.fnType.paramTypes.Count > 0)
                    throw new System.Exception("Start function is invalid function type."); // TODO: Error message

                this.Invoke(startFn);
            }
            else if(fie.type == FunctionIndexEntry.FnIdxType.Import)
            { 
                ImportFunction startIFn = module.importedFunctions[fie.index].importFn;
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

            return true;
        }
    }
}