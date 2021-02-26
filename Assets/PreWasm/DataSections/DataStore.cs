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

// using System.Collections;
// using System.Collections.Generic;

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

        public const int PageSize = 64 * 1024;

        byte [] _data = null;
        byte * _pdata = null;

        public byte [] data {get{return this._data; } }
        public byte * pdata {get{return this._pdata; } }

        public int CurByteSize 
        {
            get
            { 
                return this._data != null ? this._data.Length : 0; 
            } 
        }

        public  DataStore(int initialByteSize, Limits limits)
        {
            this.ExpandBytes(initialByteSize, limits);
        }

        public  DataStore(int initialPageSize, LimitsPaged limits)
        {   
            this.ExpandPages(initialPageSize, limits);
        }

        public DataStore(int initialEntryCt, LimitEntries limits)
        {
            this.ExpandEntries(initialEntryCt, limits);
        }

        public DataStore(int initalByteSize)
        { 
            this._ExpandInternal(initalByteSize);
        }

        public ExpandRet ExpandBytes(int newByteSize, Limits limits)
        {
            return 
                this._ExpandSize(
                    newByteSize, 
                    limits.minBytes, 
                    limits.maxBytes);
        }

        public ExpandRet ExpandEntries(int newEntriesCt, LimitEntries limits)
        {
            return
                this._ExpandSize(
                    newEntriesCt * limits.dataTypeSize,
                    limits.minEntries * limits.dataTypeSize,
                    limits.maxEntries * limits.dataTypeSize);
        }

        public ExpandRet ExpandPages(int newPageSize, LimitsPaged limits)
        { 
            return 
                this._ExpandSize(
                    newPageSize * PageSize, 
                    limits.minPages * PageSize, 
                    limits.maxPages * PageSize);
        }

        private ExpandRet _ExpandSize(int newByteSize, int minBytes, int maxBytes)
        {
            if (newByteSize > maxBytes)
                return ExpandRet.Err_TooLarge;

            if (newByteSize < 0 || (this._data != null && newByteSize < _data.Length))
                return ExpandRet.Err_TooSmall;

            if (newByteSize == 0 && (this._data == null || this._data.Length == 0))
                return ExpandRet.Err_NoChange;

            return this._ExpandInternal(newByteSize);
        }

        private ExpandRet _ExpandInternal(int newByteSize)
        {
            ExpandRet ret = this.__ExpandInternal(newByteSize);

            if (ret == ExpandRet.Successful)
            {
                fixed (byte* pb = this.data)
                {
                    this._pdata = pb;
                }
            }

            return ret;
        }

        private ExpandRet __ExpandInternal(int newByteSize)
        { 
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

        public static int GetTypeIDSize(Bin.TypeID type)
        { 
            switch(type)
            { 
                case Bin.TypeID.Int32:
                case Bin.TypeID.Float32:
                case Bin.TypeID.FuncRef:
                case Bin.TypeID.Function:
                    return 4;

                case Bin.TypeID.Float64:
                case Bin.TypeID.Int64:
                    return 8;
            }

            throw new System.Exception(); // TODO: Error message
        }
    }
}
