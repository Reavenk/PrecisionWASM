﻿// MIT License
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

namespace PxPre.WASM
{
    unsafe public class Table
    {
        public readonly Bin.TypeID type;
        public readonly LimitEntries limits;
        public DataStore store;

        public int CurByteSize
        {
            get => this.store.data != null ? this.store.data.Length : 0;
        }

        public int MaxByteSize
        {
            get => this.limits.maxEntries;
        }

        public Table(int initEntriesCt, Bin.TypeID type, LimitEntries limits)
        {
            this.type = type;
            this.limits = limits;

            int typeSz = DataStore.GetTypeIDSize(type);
            if(limits.dataTypeSize != typeSz)
                throw new System.Exception(); // TODO: Error msg

            this.store = new DataStore(initEntriesCt, limits);
        }

        public DataStore.ExpandRet ExpandEntriesCt(int newEntriesCt)
        { 
            return this.store.ExpandEntries(newEntriesCt, this.limits);
        }
    }
}