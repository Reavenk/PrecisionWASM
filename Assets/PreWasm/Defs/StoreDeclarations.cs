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
    public class StoreDeclarations
    {
        public class ModuleRecord
        {
            public readonly string moduleName;

            public Dictionary<string, DefGlobal> globals        = new Dictionary<string, DefGlobal>();
            public Dictionary<string, DefTable> tables          = new Dictionary<string, DefTable>();
            public Dictionary<string, DefFunction> functions    = new Dictionary<string, DefFunction>();
            public Dictionary<string, DefMem> memories          = new Dictionary<string, DefMem>();

            public ModuleRecord(string moduleName)
            { 
                this.moduleName = moduleName;
            }
        }

        public List<DefGlobal>      globals     = new List<DefGlobal>();
        public List<DefTable>       tables      = new List<DefTable>();
        public List<DefFunction>    functions   = new List<DefFunction>();
        public List<DefMem>         memories    = new List<DefMem>();

        public int importGlobalsCt = 0;
        public int importTablesCt = 0;
        public int importFunctionsCt = 0;
        public int importMemsCt = 0;

        Dictionary<string, ModuleRecord> moduleLookup = 
            new Dictionary<string, ModuleRecord>();

        private ModuleRecord GetOrCreateRecord(string module)
        { 
            ModuleRecord mr;
            if(this.moduleLookup.TryGetValue(module, out mr) == false)
            {
                mr = new ModuleRecord(module);
                this.moduleLookup.Add(module, mr);
            }

            return mr;
        }

        public void AddGlobal(DefGlobal global)
        { 
            this.globals.Add(global);
        }

        public void AddGlobal(string module, string fieldname, DefGlobal global)
        {
            this.globals.Add(global);
            this.GetOrCreateRecord(module).globals.Add(fieldname, global);

            ++this.importGlobalsCt;
        }

        public void AddGlobal(string module, string fieldname, Bin.TypeID type, int elements, bool mutable)
        {
            DefGlobal global = new DefGlobal(type, elements, mutable);
            this.AddGlobal(module, fieldname, global);
        }

        public void AddTable(DefTable table)
        { 
            this.tables.Add(table);
        }

        public void AddTable(string module, string fieldname, DefTable table)
        { 
            this.tables.Add(table);
            this.GetOrCreateRecord(module).tables.Add(fieldname, table);

            ++this.importTablesCt;
        }

        public void AddTable(Bin.TypeID type, uint initialElements, uint maxElements, uint flags)
        {
            DefTable table = new DefTable(type, initialElements, maxElements, flags);
            this.tables.Add(table);
        }

        public void AddFunction(DefFunction function)
        { 
            this.functions.Add(function);
        }

        public void AddFunction(string module, string fieldname, DefFunction function)
        {
            this.functions.Add(function);
            this.GetOrCreateRecord(module).functions.Add(fieldname, function);

            ++this.importFunctionsCt;
        }

        public void AddFunction(string module, string fieldname, FunctionType fnTy)
        {
            this.AddFunction(module, fieldname, new DefFunction(fnTy));
        }

        public void AddMemory(DefMem memory)
        { 
            this.memories.Add(memory);
        }

        public void AddMemory(string module, string fieldname, DefMem memory)
        { 
            this.memories.Add(memory);
            this.GetOrCreateRecord(module).memories.Add(fieldname, memory);

            ++this.importMemsCt;
        }

        public void AddMemory(uint initialPageCt, uint minPageCt, uint maxPageCt, uint flags)
        { 
            DefMem mem = new DefMem(initialPageCt, minPageCt, maxPageCt, flags);
            this.AddMemory(mem);
        }
    }
}
