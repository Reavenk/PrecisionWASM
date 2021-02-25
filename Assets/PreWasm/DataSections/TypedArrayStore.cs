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
    public class TypedArrayStore : DataStore
    {
        public readonly Bin.TypeID type;

        public TypedArrayStore(Bin.TypeID type, int elements, int maxElements)
            : base(GetTypeIDSize(type) * elements, GetTypeIDSize(type) * maxElements)
        {
            this.type = type;
        }

        public TypedArrayStore(Bin.TypeID type, int elements)
            : base(GetTypeIDSize(type) * elements)
        {
            this.type = type;
        }

        public ExpandRet ExpandElements(int elementCount)
        {
            int newByteSize = this.ElementSize() * elementCount;
            return this.ExpandSize(newByteSize);
        }

        public int ElementSize()
        {
            return GetTypeIDSize(this.type);
        }
    }
}
