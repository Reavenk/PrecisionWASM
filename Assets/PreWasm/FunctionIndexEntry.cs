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
    // TODO: Comment
    public class IndexEntry
    {
        public enum FnIdxType
        { 
            Local,
            Import
        }

        public FnIdxType type;
        public int index;
        public string module;
        public string fieldname;

        protected IndexEntry(FnIdxType type, int index, string module, string fieldname)
        { 
            this.type = type;
            this.index = index;
            this.module = module;
            this.fieldname = fieldname;
        }

        public static IndexEntry CreateLocal(int index)
        {
            return 
                new IndexEntry(FnIdxType.Local, index, string.Empty, string.Empty);
        }

        public static IndexEntry CreateImport(int index, string module, string fieldname)
        {
            return 
                new IndexEntry(FnIdxType.Import, index, module, fieldname);
        }
    }
}
