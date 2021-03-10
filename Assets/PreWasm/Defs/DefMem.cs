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
    public struct DefMem
    {
        public readonly int index;

        public readonly uint initialPages;
        public LimitsPaged limits;

        List<DefSegment> defSegments;

        public void AddDefault(DefSegment ds)
        {
            this.defSegments.Add(ds);
        }

        public DefMem(int index, uint initialPages, uint minPages, uint maxPages )
        { 
            this.index = index;

            this.initialPages = initialPages;
            this.limits = new LimitsPaged(minPages, maxPages);
            this.defSegments = new List<DefSegment>();
        }

        public Memory CreateDefault(ExecutionContext globScr)
        { 
            Memory ret = 
                new Memory(
                    this.initialPages, 
                    this.limits, 
                    this.defSegments, 
                    globScr);

            return ret;
        }
    }
}