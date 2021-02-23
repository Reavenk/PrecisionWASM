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
    unsafe public class Table
    {
        public Bin.TypeID type;
        public byte [] data = null;
        public byte * pdata = null;
        public uint max;

        public void Resize(int elementCount)
        { 
            if(elementCount == 0)
            { 
                this.data = null;
                this.pdata = null;
            }

            int es = this.ElementSize();
            int byteSz = es * elementCount;

            if(this.data == null)
            {
                this.data = new byte[byteSz];
                for(int i = 0; i < byteSz; ++i)
                    this.data[i] = 0;
            }
            else
            { 
                int oldSz = this.data.Length;
                byte [] oldRB = data;
                this.data = new byte[byteSz];

                int transferSz = oldSz < byteSz ? oldSz : byteSz;
                for(int i = 0; i < transferSz; ++i)
                    this.data[i] = oldRB[i];

                for(int i = transferSz; i < byteSz; ++i)
                    this.data[i] = 0;
            }

            fixed(byte * pb = this.data)
                this.pdata = pb;
        }

        public int ElementSize()
        { 
            switch(this.type)
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
    }
}