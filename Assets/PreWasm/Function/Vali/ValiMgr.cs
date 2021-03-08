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

namespace PxPre.WASM.Vali
{
    public class ValiMgr
    {
        public List<StackOpd> opds = new List<StackOpd>();
        public Stack<CtrlFrame> ctrls = new Stack<CtrlFrame>();

        public void PushOpd(StackOpd so)
        {
            this.opds.Add(so);
        }

        public void PushOpd(Bin.TypeID ty)
        {
            StackOpd so = ConvertToStackType(ty);
            this.PushOpd(so);
        }

        public StackOpd PopOpd(bool ending = false)
        {
            if(ending == false)
            {
                if (this.opds.Count == this.ctrls.Peek().height && this.ctrls.Peek().unreachable == true)
                    return StackOpd.Unknown;

                if (this.opds.Count == this.ctrls.Peek().height)
                    this.EmitValidationError("Operation stack mismatch.");
            }

            StackOpd ret = this.opds[this.opds.Count - 1];
            this.opds.RemoveAt(this.opds.Count - 1);
            return ret;
        }

        public StackOpd PopOpd(StackOpd expect, bool ending = false)
        {
            StackOpd actual = this.PopOpd(ending);
            if (actual == StackOpd.Unknown)
                return expect;

            if (expect == StackOpd.Unknown)
                return actual;

            if (actual != expect)
                this.EmitValidationError($"Incorrect type {actual} when expecting {expect}.");

            return actual;
        }

        public StackOpd PopOpd(Bin.TypeID expect)
        {
            StackOpd soexp = ConvertToStackType(expect);
            return this.PopOpd(soexp);
        }

        public void PushOpds(List<StackOpd> types)
        {
            this.opds.AddRange(types);
        }

        public void PopOpds(List<StackOpd> types, bool ending = false)
        {
            for (int i = types.Count - 1; i >= 0; --i)
                this.PopOpd(types[i], ending);
        }

        public CtrlFrame PushCtrl(
            Instruction opcode, 
            List<StackOpd> instk, 
            List<StackOpd> outstk,
            DataStoreIdx memStore,
            DataStoreIdx globStore,
            DataStoreIdx tabStore)
        {
            CtrlFrame frame     = new CtrlFrame();
            frame.opcode        = opcode;
            frame.startTypes    = instk;
            frame.endTypes      = outstk;
            frame.unreachable   = false;
            frame.height        = this.opds.Count;
            frame.memoryStore   = memStore;
            frame.globalStore   = globStore;
            frame.tableStore    = tabStore;
            this.ctrls.Push(frame);
            this.PushOpds(instk);

            return frame;
        }

        unsafe public CtrlFrame PopCtrl(List<byte> expanded)
        {
            if (this.ctrls.Count == 0)
            { } // TODO: Error

            CtrlFrame frame = this.ctrls.Peek();
            this.PopOpds(frame.endTypes);

            if(frame.returnedInside == true)
            { 
                while(this.opds.Count > frame.height)
                    this.PopOpd();
            }
            if (this.opds.Count != frame.height)
            { } // TODO: Error

            this.ctrls.Pop();

            if (frame.opcode == Instruction.loop)
            {
                //Function.TransferInstruction(expanded, Instruction._goto);
                //Function.TransferInt32u(expanded, frame.loopStart);
                frame.FlushEndWrites(expanded, frame.loopStart);
            }
            else
                frame.FlushEndWrites(expanded);

            if(this.ctrls.Count > 0)
            {
                CtrlFrame entered = this.ctrls.Peek();
                entered.FlushEnterWrites(expanded);
            }

            return frame;
        }

        public CtrlFrame GetCtrl(int fromTop)
        {
            foreach (CtrlFrame cf in this.ctrls)
            {
                if (fromTop == 0)
                    return cf;

                --fromTop;
            }
            return null;
        }

        public void Unreachable()
        {
            int height = this.ctrls.Peek().height;

            // This popping in batch isn't as efficient as it could be
            while (this.opds.Count > height)
                this.opds.RemoveAt(this.opds.Count - 1);

            this.ctrls.Peek().unreachable = true;
        }

        public void EmitValidationError(string str)
        {
            throw new System.Exception(str);
        }

        public static StackOpd ConvertToStackType(Bin.TypeID tyid)
        {
            switch (tyid)
            {
                case Bin.TypeID.Float32:
                    return StackOpd.f32;

                case Bin.TypeID.Float64:
                    return StackOpd.f64;

                case Bin.TypeID.Int32:
                    return StackOpd.i32;

                case Bin.TypeID.Int64:
                    return StackOpd.i64;
            }

            return StackOpd.Unknown;
        }

        public static bool DoDataStoreValidation(
            IReadOnlyList<IndexEntry> idxEntries, 
            int operandSrc, 
            List<byte> expanded, 
            ref DataStoreIdx dstore)
        {
            // Check if we need to change the global store source
            IndexEntry ie = idxEntries[(int)operandSrc];
            if (dstore.Match(ie.type, ie.index) == false)
            {
                if (ie.type == IndexEntry.FnIdxType.Import)
                {
                    Function.TransferInstruction(expanded, Instruction._global_chStoreImp);
                    Function.TransferInt32u(expanded, (uint)ie.index);

                    dstore.Set(DataStoreIdx.Location.Import, ie.index);
                    return true;
                }
                else
                {
                    Function.TransferInstruction(expanded, Instruction._global_chStoreLoc);
                    Function.TransferInt32u(expanded, (uint)ie.index);

                    dstore.Set(DataStoreIdx.Location.Local, ie.index);
                    return true;
                }
            }
            else
                return false;
        }

        public static bool EnsureDefaultMemory(IReadOnlyList<IndexEntry> memoryIndices, List<byte> expanded, ref DataStoreIdx dsIdx)
        {
            if(dsIdx.loc != DataStoreIdx.Location.Unknown)
                return false;

            if(memoryIndices.Count == 0)
                throw new System.Exception("No memories provided for a function that requires memory storage.");

            IndexEntry ie = memoryIndices[0];
            if (ie.type == IndexEntry.FnIdxType.Local)
            {
                dsIdx.loc = DataStoreIdx.Location.Local;
                dsIdx.index = ie.index;

                Function.TransferInstruction(expanded, Instruction._SetMemoryStoreLoc);
                Function.TransferInt32s(expanded, ie.index );

                return true;
            }
            else
            {
                dsIdx.loc = DataStoreIdx.Location.Import;
                dsIdx.index = ie.index;

                Function.TransferInstruction(expanded, Instruction._SetMemoryStoreImp);
                Function.TransferInt32s(expanded, ie.index);

                return true;
            }

            throw new System.Exception(); // TODO: Error message
        }

        public static bool EnsureDefaulTable(IReadOnlyList<IndexEntry> tableIndices, List<byte> expanded, ref DataStoreIdx dsIdx)
        {
            if (dsIdx.loc != DataStoreIdx.Location.Unknown)
                return false;

            if (tableIndices.Count == 0)
                throw new System.Exception("No tables provided for a function that requires memory storage.");

            IndexEntry ie = tableIndices[0];
            if (ie.type == IndexEntry.FnIdxType.Local)
            {
                dsIdx.loc = DataStoreIdx.Location.Local;
                dsIdx.index = ie.index;

                Function.TransferInstruction(expanded, Instruction._SetTableStoreLoc);
                Function.TransferInt32s(expanded, ie.index);

                return true;
            }
            else
            {
                dsIdx.loc = DataStoreIdx.Location.Import;
                dsIdx.index = ie.index;

                Function.TransferInstruction(expanded, Instruction._SetTableStoreImp);
                Function.TransferInt32s(expanded, ie.index);

                return true;
            }

            throw new System.Exception(); // TODO: Error message
        }

        public static int GetSize(StackOpd so)
        {
            switch (so)
            {
                case StackOpd.i32:
                case StackOpd.f32:
                    return 4;

                case StackOpd.i64:
                case StackOpd.f64:
                    return 8;
            }
            throw new System.Exception("Cannot get variable stack size of unknown type.");
        }

        public int GetStackOpdSize()
        { 
            int ret = 0;

            foreach(StackOpd so in this.opds)
                ret += GetSize(so);

            return ret;
        }
    }
}