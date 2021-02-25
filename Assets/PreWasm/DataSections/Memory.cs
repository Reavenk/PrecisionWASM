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
    // https://webassembly.github.io/spec/core/syntax/modules.html#syntax-mem
    public unsafe class Memory : DataStore
    {
        public const int PageSize = 64 * 1024;

        public uint flags;
        public int minPageCt = 0;
        public int maxPageCt = int.MaxValue;

        public new int CurByteSize
        {
            get => this.data != null ? this.data.Length : 0;
        }

        public new int MaxByteSize
        {
            get => this.MaxByteSize;
        }

        public Memory(int initPageCt, int minPageCt, int maxPageCt)
            : base(initPageCt * PageSize, maxPageCt * PageSize)
        {
            this.minPageCt = minPageCt;
            this.maxPageCt = maxPageCt;
        }

        public Memory(int initialPageCt)
            : base(PageSize * initialPageCt)
        {
        }

        public uint CalculatePageCt()
        { 
            return (uint)(this.CurByteSize / PageSize);
        }

        public ExpandRet ExpandPageCt(int newPageSize)
        {
            int newPageByteCt = (int)(newPageSize * PageSize);
            return this.ExpandSize(newPageByteCt);
        }

    }
}
