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
        private struct Default
        { 
            public int offset;
            public byte [] data;
        }

        public readonly int index;

        public readonly uint initialPages;
        public LimitsPaged limits;

        List<Default> defaultData;

        public void AddDefault(int offset, byte [] data)
        {
            Default def = new Default();
            def.offset = offset;
            def.data = data;

            if(this.defaultData == null)
                this.defaultData = new List<Default>();

            this.defaultData.Add(def);
        }

        public DefMem(int index, uint initialPages, uint minPages, uint maxPages )
        { 
            this.index = index;

            this.initialPages = initialPages;
            this.limits = new LimitsPaged((int)minPages, (int)maxPages);
            this.defaultData = null;
        }

        public Memory CreateDefault()
        { 
            Memory ret = new Memory((int)this.initialPages, this.limits);

            if(this.defaultData != null)
            { 
                foreach(Default def in this.defaultData)
                { 
                    int max = (int)(def.offset + def.data.Length);
                    int reqPages = (int)System.Math.Ceiling(max / (double)DataStore.PageSize);

                    int curPageCt = (int)ret.CalculatePageCt();
                    if ( curPageCt < reqPages)
                    {
                        if(ret.ExpandPageCt(reqPages) != DataStore.ExpandRet.Successful)
                            throw new System.Exception("Could not initialize memory to the appropriate size.");
                    }

                    for(int i = 0; i < def.data.Length; ++i)
                        ret.store.data[def.offset + i] = def.data[i];
                }
            }

            return ret;
        }
    }
}