﻿// MIT License
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

        public void PushOpd(StackOpd os)
        {
            this.opds.Add(os);
        }

        public StackOpd PopOpd()
        {
            if (this.opds.Count == this.ctrls.Peek().height && this.ctrls.Peek().unreachable == true)
                return StackOpd.Unknown;

            if (this.opds.Count == this.ctrls.Peek().height)
                this.EmitValidationError("Operation stack mismatch.");

            StackOpd ret = this.opds[this.opds.Count - 1];
            this.opds.RemoveAt(this.opds.Count - 1);
            return ret;
        }

        public StackOpd PopOpd(StackOpd expect)
        {
            StackOpd actual = this.PopOpd();
            if (actual == StackOpd.Unknown)
                return expect;

            if (expect == StackOpd.Unknown)
                return actual;

            if (actual != expect)
                this.EmitValidationError($"Incorrect type {actual} when expecting {expect}.");

            return actual;
        }

        public void PushOpds(List<StackOpd> types)
        {
            this.opds.AddRange(types);
        }

        public void PopOpds(List<StackOpd> types)
        {
            for (int i = types.Count - 1; i >= 0; --i)
                this.PopOpd(types[i]);
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

            if (this.opds.Count != frame.height)
            { } // TODO: Error

            this.ctrls.Pop();
            frame.FlushEndWrites(expanded);
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

        public List<StackOpd> LabelTypes(CtrlFrame frame)
        {
            if (frame.opcode == Instruction.loop)
                return frame.startTypes;
            else
                return frame.endTypes;
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
            List<IndexEntry> idxEntries, 
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
    }
}