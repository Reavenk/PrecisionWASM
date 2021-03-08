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
    public class CtrlFrame 
    {
        public Instruction opcode;
        public List<StackOpd> startTypes;
        public List<StackOpd> endTypes;
        public int height;
        public bool unreachable;

        public DataStoreIdx memoryStore;
        public DataStoreIdx tableStore;
        public DataStoreIdx globalStore;
        public uint loopStart = uint.MaxValue;

        public List<uint> writePopped;
        public List<uint> writeEntered;

        public bool returnedInside = false;

        public void QueueEndWrite(List<byte> expanded)
        {
            if (this.writePopped == null)
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

        public void QueueEnterWrite(List<byte> expanded)
        {
            if (this.writeEntered == null)
                this.writeEntered = new List<uint>();

            this.writeEntered.Add((uint)expanded.Count);

            // Add an int into expanded
            expanded.Add(0);
            expanded.Add(0);
            expanded.Add(0);
            expanded.Add(0);
        }

        public void QueueEnterWrite(uint loc)
        { 
            if(this.writeEntered == null)
                this.writeEntered = new List<uint>();

            this.writeEntered.Add(loc);
        }

        unsafe public void FlushEndWrites(List<byte> expanded)
        {
            this.FlushEndWrites(expanded, (uint)expanded.Count);
        }

        unsafe public void FlushEndWrites(List<byte> expanded, uint jumpValue)
        {
            if (this.writePopped == null)
                return;

            uint idx = jumpValue;

            // Write the current end position (idx) in every position queued
            // to have it written to.
            byte[] rb = System.BitConverter.GetBytes(idx);
            foreach (uint u in this.writePopped)
            {
                expanded[(int)u + 0] = rb[0];
                expanded[(int)u + 1] = rb[1];
                expanded[(int)u + 2] = rb[2];
                expanded[(int)u + 3] = rb[3];
            }
        }

        unsafe public void FlushEnterWrites(List<byte> expanded)
        {
            this.FlushEnterWrites(expanded, (uint)expanded.Count);
        }

        unsafe public void FlushEnterWrites(List<byte> expanded, uint jumpValue)
        {
            if (this.writeEntered == null)
                return;

            uint idx = jumpValue;

            // Write the current end position (idx) in every position queued
            // to have it written to.
            byte[] rb = System.BitConverter.GetBytes(idx);
            foreach (uint u in this.writeEntered)
            {
                expanded[(int)u + 0] = rb[0];
                expanded[(int)u + 1] = rb[1];
                expanded[(int)u + 2] = rb[2];
                expanded[(int)u + 3] = rb[3];
            }

            this.writeEntered.Clear();
        }

        public List<StackOpd> LabelTypes()
        {
            if (this.opcode == Instruction.loop)
                return this.startTypes;
            else
                return this.endTypes;
        }

        public bool MatchesLabelTypes(CtrlFrame other)
        {
            List<StackOpd> otherTys = other.LabelTypes();
            return this.MatchesLabelTypes(otherTys);
        }

        public bool MatchesLabelTypes(List<StackOpd> other)
        {
            List<StackOpd> ltA = this.LabelTypes();

            if(ltA.Count != other.Count)
                return false;

            for(int i = 0; i < ltA.Count; ++i)
            { 
                if(ltA[i] != other[i])
                    return false;
            }

            return true;
        }

        public void SetToRestoreOnPop()
        { 
            this.returnedInside = true;
        }
    }
}