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

        public const uint PageSize = 64 * 1024;

        byte [] _data = null;
        byte * _pdata = null;

        public byte [] data {get{return this._data; } }
        public byte * pdata {get{return this._pdata; } }

        public readonly uint storeAlignment = 1;

        public int CurByteSize 
        {
            get
            { 
                return this._data != null ? this._data.Length : 0; 
            } 
        }

        /// <summary>
        /// The constructor of a DataStore for raw bytes.
        /// 
        /// This function appears to be unused. It may be slated for removal.
        /// </summary>
        /// <param name="initialByteSize">The initial size of the memory.</param>
        /// <param name="limits">The limits to the memory store.</param>
        /// <param name="defaults">The default values.</param>
        /// <param name="globSrc">The source for global variables is offsets in the defaults 
        /// are global expressions.</param>
        public  DataStore(uint initialByteSize, Limits limits, List<DefSegment> defaults, ExecutionContext globSrc)
        {
            this.storeAlignment = 1;

            this.ExpandBytes(initialByteSize, limits);
            this.WriteDefSegmentsData(defaults, globSrc);
        }

        /// <summary>
        /// The constructor of a DataStore for raw bytes, without a limits specification.
        /// 
        /// This function is used to allocate memory for global variables.
        /// </summary>
        /// <param name="initalByteSize">The size of the data in bytes.</param>
        public DataStore(uint initalByteSize)
        {
            this._ExpandInternal(initalByteSize);
        }

        /// <summary>
        /// The constructor of a DataStore for a memory.
        /// </summary>
        /// <param name="initialPageSize"></param>
        /// <param name="limits"></param>
        /// <param name="defaults"></param>
        /// <param name="globSrc"></param>
        public  DataStore(uint initialPageSize, LimitsPaged limits, List<DefSegment> defaults, ExecutionContext globSrc)
        {   
            this.storeAlignment = PageSize;

            this.ExpandPages_OrThrow(initialPageSize, limits);

            // Write the default values, or expand the store if (for some reason) we need 
            // even more memory than the initial specific page size.
            uint defaultSet = this.WriteDefSegmentsData(defaults, globSrc);
            if (defaultSet != 0)
            { 
                uint reqPgCt = defaultSet / PageSize;
                if(defaultSet % PageSize != 0)
                    ++reqPgCt;

                this.ExpandPages_OrThrow(reqPgCt, limits);
                if(this.WriteDefSegmentsData(defaults, globSrc) != 0)
                    throw new System.Exception("Unknown issue setting default store values for Memory.");
            }
            
        }

        public DataStore(uint initialEntryCt, LimitEntries limits, List<DefSegment> defaults, ExecutionContext globSrc)
        {
            this.storeAlignment = limits.dataTypeSize;

            this.ExpandEntries_OrThrow(initialEntryCt, limits);

            uint defaultSet = this.WriteDefSegmentsData(defaults, globSrc);
            if (defaultSet != 0)
            {
                uint reEleCt = defaultSet / limits.dataTypeSize;
                if (defaultSet % limits.dataTypeSize != 0)
                    ++reEleCt;

                this.ExpandEntries_OrThrow(reEleCt, limits);
                if(this.WriteDefSegmentsData(defaults, globSrc) != 0)
                    throw new System.Exception("Unknown issue setting default store values for Table.");
            }

        }

        /// <summary>
        /// Write DefSegments (that represent's a store's default values) into the store.
        /// </summary>
        /// <param name="defaults">The segments of data that make up the store's default values.</param>
        /// <param name="globSrc">The source for globals if the offset came from an expression
        /// that referenced a global value.</param>
        /// <returns>0, if the writes were successful. Else, the minimum number of bytes for
        /// a successful write is returned.</returns>
        private uint WriteDefSegmentsData(List<DefSegment> defaults, ExecutionContext globSrc)
        { 
            if(defaults == null || defaults.Count == 0)
                return 0; // Nothing to write

            uint max = 0;
            foreach(DefSegment ds in defaults)
                max = System.Math.Max(max, ds.GetEndIndex(globSrc));

            // Non-zero values are errors - signal the invoking calling that we didn't
            // write because there wasn't enough space available, and return how much
            // minimum space is needed.
            if(this.CurByteSize < max)
                return max;

            foreach(DefSegment ds in defaults)
            { 
                uint offset = ds.EvaluateOffset(globSrc);
                for(uint i = 0; i < ds.data.Length; ++i)
                    this.data[offset + i] = ds.data[i];
            }

            return 0; // Success
        }

        public ExpandRet ExpandBytes(uint newByteSize, Limits limits)
        {
            return 
                this._ExpandSize(
                    newByteSize, 
                    limits.minBytes, 
                    limits.maxBytes);
        }

        public ExpandRet ExpandEntries(uint newEntriesCt, LimitEntries limits)
        {
            return
                this._ExpandSize(
                    newEntriesCt * limits.dataTypeSize,
                    limits.minEntries * limits.dataTypeSize,
                    limits.maxEntries * limits.dataTypeSize);
        }

        public ExpandRet ExpandEntries_OrThrow(uint newEntriesCt, LimitEntries limits, bool throwOnNoChange = false)
        { 
            ExpandRet ret = this.ExpandEntries(newEntriesCt, limits);
            this.ThrowErrorForExpandReturn(ret, throwOnNoChange);
            return ret;
        }

        public ExpandRet ExpandPages(uint newPageSize, LimitsPaged limits)
        { 
            uint maxSize = (uint)System.Math.Min( uint.MaxValue, (long)limits.maxPages * PageSize);

            return 
                this._ExpandSize(
                    newPageSize * PageSize, 
                    limits.minPages * PageSize, 
                    maxSize);
        }

        public ExpandRet ExpandPages_OrThrow(uint newPageSize, LimitsPaged limits, bool throwOnNoChange = false)
        {
            ExpandRet ret = this.ExpandPages(newPageSize, limits);
            this.ThrowErrorForExpandReturn(ret, throwOnNoChange);
            return ret;
        }

        private void ThrowErrorForExpandReturn(ExpandRet ret, bool throwOnNoChange = false)
        {
            if (throwOnNoChange == true && ret == ExpandRet.Err_NoChange)
                throw new System.Exception("Attempting to expand data store did not result in any change.");

            if (ret == ExpandRet.Err_TooLarge)
                throw new System.Exception("Attempting to expand data store larger than the maximum size.");

            if (ret == ExpandRet.Err_TooSmall)
                throw new System.Exception("Attempting to expand data store smaller than the minimum size.");
        }

        private ExpandRet _ExpandSize(uint newByteSize, uint minBytes, uint maxBytes)
        {
            if (newByteSize > maxBytes)
                return ExpandRet.Err_TooLarge;

            if (this._data != null && newByteSize < _data.Length)
                return ExpandRet.Err_TooSmall;

            if (newByteSize == 0 && (this._data == null || this._data.Length == 0))
                return ExpandRet.Err_NoChange;

            return this._ExpandInternal(newByteSize);
        }

        private ExpandRet _ExpandInternal(uint newByteSize)
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

        private ExpandRet __ExpandInternal(uint newByteSize)
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

        public static uint GetTypeIDSize(Bin.TypeID type)
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
