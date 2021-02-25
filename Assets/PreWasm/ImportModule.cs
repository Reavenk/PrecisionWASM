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
    /// <summary>
    /// The imported module data of an ExecutionContext.
    /// </summary>
    public class ImportModule
    {
        public readonly StoreDeclarations storeDecl;

        /// <summary>
        /// Functions that are provided by outside sources in an input block.
        /// </summary>
        public List<ImportFunction> importFn    = null;

        /// <summary>
        /// Memory that is defined as an import.
        /// </summary>
        public List<Memory> memories            = null;

        /// <summary>
        /// Globals that are defined as an input.
        /// </summary>
        public List<Global> globals             = null;

        /// <summary>
        /// Tables that are defined as an input.
        /// </summary>
        public List<Table> tables               = null;

        public int NumFunctions { get => this.storeDecl.importFunctionsCt; }
        public int NumMemories { get => this.storeDecl.importMemsCt; }
        public int NumGlobals { get => this.storeDecl.importGlobalsCt; }
        public int NumTables { get => this.storeDecl.importTablesCt; }

        public ImportModule(StoreDeclarations storeDecl)
        {
            this.storeDecl = storeDecl;

            this.Reset();
        }

        public void Reset()
        {
            this.importFn   = new List<ImportFunction>();
            this.memories   = new List<Memory>();
            this.globals    = new List<Global>();
            this.tables     = new List<Table>();

            for(int i = 0; i < this.storeDecl.importFunctionsCt; ++i)
                this.importFn.Add(null);

            for(int i = 0; i < this.storeDecl.importMemsCt; ++i)
                this.memories.Add(null);

            for(int i = 0; i < this.storeDecl.importGlobalsCt; ++i)
                this.globals.Add(null);

            for(int i = 0; i < this.storeDecl.importTablesCt; ++i)
                this.tables.Add(null);
        }
    }
}
