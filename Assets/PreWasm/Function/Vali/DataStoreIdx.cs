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

namespace PxPre.WASM.Vali
{
    public struct DataStoreIdx
    {
        public enum Location
        {
            Unknown,
            Import,
            Local
        }

        public Location loc;
        public int index;

        public bool Match(Location lc, int idx)
        { 
            return this.loc == lc && this.index == idx;
        }

        public bool Match(IndexEntry.FnIdxType type, int idx)
        { 
            switch(type)
            { 
                case IndexEntry.FnIdxType.Local:
                    if(this.loc != Location.Local)
                        return false;
                    break;

                case IndexEntry.FnIdxType.Import:
                    if(this.loc != Location.Import)
                        return false;
                    break;
            }

            return this.index == idx;
        }

        public void Set(Location lc, int idx)
        { 
            this.loc = lc;
            this.index = idx;
        }

        public bool SetMatch(Location lc, int idx)
        { 
            if(this.Match(lc, idx) == true)
                return true;

            this.Set(lc, idx);
            return false;
        }

        public bool Update(IndexEntry o)
        { 
            if(this.loc == Location.Unknown)
                return false;

            if(this.Match(o.type, o.index) == false)
            {
                this.SetInvalid();
                return false;
            }

            return true;
        }

        public void SetInvalid()
        { 
            this.loc = Location.Unknown;
            this.index = -1;
        }
    }
}