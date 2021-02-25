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
        public readonly ImportDefinitions definition;

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

        public ImportModule(ImportDefinitions definition)
        {
            this.definition = definition;

            this.Reset();
        }

        public void Reset()
        {
            this.importFn   = new List<ImportFunction>();
            this.memories   = new List<Memory>();
            this.globals    = new List<Global>();
            this.tables     = new List<Table>();

            foreach (DefFunction df in definition.functions)
                this.importFn.Add(null);

            foreach (DefMem dm in definition.memories)
            {
                Memory mem = new Memory((int)dm.initialPages, (int)dm.minPages, (int)dm.maxPages);
                this.memories.Add(mem);
            }

            foreach (DefGlobal dg in definition.globals)
            {
                Global glob = new Global(dg.type, 1, dg.mut == Global.Mutability.Variable);
                this.globals.Add(glob);
            }

            foreach (DefTable dt in definition.tables)
            { 
                Table tabl = new Table(dt.type, (int)dt.elements);
                this.tables.Add(tabl);
            }

        }
    }
}
