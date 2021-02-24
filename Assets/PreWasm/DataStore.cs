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
    unsafe public class DataStore
    { 
        public enum ExpandRet
        { 
            Err_NoChange,
            Err_TooLarge,
            Err_TooSmall,
            Successful
        }

        byte [] _data = null;
        byte * _pdata = null;

        public byte [] data {get{return this._data; } }
        public byte * pdata {get{return this._pdata; } }

        int maxByteSize = 0;

        public int CurByteSize 
        {
            get
            { 
                return this._data != null ? this._data.Length : 0; 
            } 
        }

        public int MaxByteSize
        { 
            get
            { 
                return this.maxByteSize;
            }
        }

        protected DataStore(int initialSize, int maxSize)
        {
            if(maxSize < initialSize)
                maxSize = initialSize;

            this.SetMaxSize(maxSize);
            this.ExpandSize(initialSize);
        }

        protected DataStore(int initialSize)
        {
            this.SetMaxSize(initialSize);
            this.ExpandSize(initialSize);
        }

        protected bool SetMaxSize(int newMax)
        { 
            if(newMax <= this.maxByteSize)
                return false;

            this.maxByteSize = newMax;
            return true;
        }

        protected ExpandRet ExpandSize(int newByteSize)
        { 
            ExpandRet ret = this._ExpandSize(newByteSize);

            if(ret == ExpandRet.Successful)
            { 
                fixed(byte * pb = this.data)
                { 
                    this._pdata = pb;
                }
            }

            return ret;
        }

        private ExpandRet _ExpandSize(int newByteSize)
        { 
            if(newByteSize > this.maxByteSize)
                return ExpandRet.Err_TooLarge;

            if(newByteSize < 0 || (this._data != null && newByteSize < _data.Length))
                return ExpandRet.Err_TooSmall;

            if(newByteSize == 0 && (this._data == null || this._data.Length == 0))
                return ExpandRet.Err_NoChange;

            if(this._data == null)
            { 
                this._data = new byte [newByteSize];
                for(int i = 0; i < newByteSize; ++i)
                    this._data[i] = 0;

                return ExpandRet.Successful;
            }

            if(this._data.Length == newByteSize)
                return ExpandRet.Err_NoChange;

            byte [] oldData = this.data;
            this._data = new byte[newByteSize];

            // Transfer existing data
            for(int i = 0; i < oldData.Length; ++i)
                this._data[i] = oldData[i];

            // Clear new expanded data
            for(int i = oldData.Length; i < newByteSize; ++i)
                this._data[i] = 0;

            return ExpandRet.Successful;
        }
    }
}
