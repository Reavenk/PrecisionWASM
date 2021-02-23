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
    public class ImportModule
    {
        // It's unknown if we want to do this for modules (have them all based off a class)
        // but 
        public class ImportEntry
        {
            public virtual FunctionImportEntry CastToFunctionImport() 
            { return null; }

            public virtual TableImportEntry CastToTableImport()
            { return null; }

            public virtual GlobalTypeEntry CastToGlobalImport()
            { return null; }

            public virtual MemoryTypeEntry CastToMemoryImport()
            { return null; }
        }

        public class FunctionImportEntry : ImportEntry
        {
            public ImportFunction importFn;

            public FunctionImportEntry(ImportFunction ifn)
            { 
                this.importFn = ifn;
            }

            public override FunctionImportEntry CastToFunctionImport()
            { return this; }
        }

        public class TableImportEntry : ImportEntry
        {

            public override TableImportEntry CastToTableImport()
            { return null; }

        }

        public class GlobalTypeEntry : ImportEntry
        {
            public Bin.TypeID type;
            public bool mutable;

            // TODO: Data payload

            public override GlobalTypeEntry CastToGlobalImport()
            { return this; }
        }

        public class MemoryTypeEntry : ImportEntry
        {
            public override MemoryTypeEntry CastToMemoryImport()
            { return this; }
        }

        public Dictionary<string, ImportEntry> importedMembers = 
            new Dictionary<string, ImportEntry>();
    }
}
