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
    public unsafe class Memory
    {
        public const int PageSize = 64 * 1024;

        public byte flags;
        public uint minSize = 0;
        public uint maxSize = int.MaxValue;

        // The memory. While pmemory is the pointer that's suggested 
        // for use, pmemory is used to make sure the memory target isn't
        // garbage collected - as well as giving access to properties
        // such as the array size.
        //
        // If this turns out to be too dangerous or unreliable on certain
        // builds/compilers, alternative implementations with a preprocessor
        // may be needed.
        // (wleu 02/19/2021)
        public byte [] memory = null;
        public byte * pmemory; // point to memory. Should always be reset if memory is reset

        public uint CalculatePageSize()
        { 
            if(this.memory == null)
                return 0;

            return (uint)(this.memory.Length / PageSize);
        }

        public bool Resize(int newPageSize)
        { 
            uint origPageSize = this.CalculatePageSize();
            if(origPageSize > newPageSize)
                return false;

            if(newPageSize > this.maxSize)
                return false;

            uint oldEnd = origPageSize * PageSize;

            if(memory == null)
                this.memory = new byte[newPageSize * PageSize];
            else
            {
                byte [] oldMem = this.memory;
                this.memory = new byte[newPageSize * PageSize];

                for(int i = 0; i < oldEnd; ++i)
                    this.memory[i] = oldMem[i];
            }

            uint newPageByteCt = (uint)(newPageSize * PageSize);
            for(uint i = origPageSize; i < newPageByteCt; ++i)
                this.memory[i] = 0;

            fixed(byte * pb = this.memory)
            {
                this.pmemory = pb;
            }

            return true;
        }

    }
}
