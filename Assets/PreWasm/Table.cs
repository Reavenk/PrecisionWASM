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

// TODO: Remove if not used

namespace PxPre.WASM
{
    // TODO: Possibly merge with memories storage
    unsafe public class Table : DataStore
    {
        public Bin.TypeID type;
        public uint max;
        public uint flags;

        public Table(Bin.TypeID type, int elementCount, int elementMax)
            : base(
                  ElementSize(type) * elementCount, 
                  ElementSize(type) * elementMax)
        { 
            this.type = type;
        }

        public Table(Bin.TypeID type, int elementCount)
            : base(ElementSize(type))
        { 
            this.type = type;
        }

        public ExpandRet ExpandElements(int elementCount)
        { 
            int newByteSize = this.ElementSize() * elementCount;
            return this.ExpandSize(newByteSize);
        }

        protected static int ElementSize(Bin.TypeID ty)
        {
            switch (ty)
            {
                case Bin.TypeID.Float32:
                    return 4;

                case Bin.TypeID.Float64:
                    return 8;

                case Bin.TypeID.Int32:
                    return 4;

                case Bin.TypeID.Int64:
                    return 8;

                case Bin.TypeID.FuncRef:
                    return 4;
            }
            throw new System.Exception("Asking for Table element size of unknown type.");
        }

        public int ElementSize()
        { 
            return ElementSize(this.type);
        }
    }
}