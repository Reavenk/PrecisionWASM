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

        public readonly Module parentModule;

        public List<DefGlobal>      globals     = new List<DefGlobal>();
        public List<DefTable>       tables      = new List<DefTable>();
        public List<DefFunction>    functions   = new List<DefFunction>();
        public List<DefMem>         memories    = new List<DefMem>();

        List<IndexEntry> indexingFunction       = new List<IndexEntry>();
        List<IndexEntry> indexingGlobal         = new List<IndexEntry>();
        List<IndexEntry> indexingMemory         = new List<IndexEntry>();
        List<IndexEntry> indexingTable          = new List<IndexEntry>();

        public IReadOnlyList<IndexEntry> IndexingFunction   { get { return this.indexingFunction;   } }
        public IReadOnlyList<IndexEntry> IndexingGlobal     { get { return this.indexingGlobal;     } }
        public IReadOnlyList<IndexEntry> IndexingMemory     { get { return this.indexingMemory;     } }
        public IReadOnlyList<IndexEntry> IndexingTable      { get { return this.indexingTable;      } }

        public int importFunctionsCt    { get; private set; } = 0;
        public int importMemsCt         { get; private set; } = 0;
        public int importGlobalsCt      { get; private set; } = 0;
        public int importTablesCt       { get; private set; } = 0;

        public int localFunctionCt      { get; private set; } = 0;
        public int localMemsCt          { get; private set; } = 0;
        public int localGlobalCt        { get; private set; } = 0;
        public int localTablesCt        { get; private set; } = 0;

        Dictionary<string, ModuleRecord> moduleLookup = 
            new Dictionary<string, ModuleRecord>();

        public StoreDeclarations(Module parentModule)
        { 
            this.parentModule = parentModule;
        }

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

        public ModuleRecord GetModuleRecord(string module)
        {
            ModuleRecord mr;
            this.moduleLookup.TryGetValue(module, out mr);
            return mr;
        }

        public IEnumerable<string> ModuleNames()
        {
            return this.moduleLookup.Keys;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        //      Add Function Declarations
        //
        ////////////////////////////////////////////////////////////////////////////////

        public void AddFunctionLoc(FunctionType fnTy)
        {
            this.indexingFunction.Add(IndexEntry.CreateLocal(this.localFunctionCt));

            DefFunction df = new DefFunction(this.functions.Count, fnTy);
            this.functions.Add(df);

            ++this.localFunctionCt;
        }

        public void AddFunctionImp(string module, string fieldname, FunctionType fnTy)
        {
            this.indexingFunction.Add(IndexEntry.CreateImport(this.importFunctionsCt, module, fieldname));

            DefFunction df = new DefFunction(this.importFunctionsCt, fnTy);
            this.functions.Add(df);

            this.GetOrCreateRecord(module).functions.Add(fieldname, df);

            ++this.importFunctionsCt;
        }


        ////////////////////////////////////////////////////////////////////////////////
        //
        //      Add Memory Declarations
        //
        ////////////////////////////////////////////////////////////////////////////////

        public void AddMemoryLoc(uint initialPageCt, uint minPageCt, uint maxPageCt /*, uint flags*/)
        {
            this.indexingMemory.Add(IndexEntry.CreateLocal(this.localMemsCt));

            DefMem mem = new DefMem(this.memories.Count, initialPageCt, minPageCt, maxPageCt );
            this.memories.Add(mem);

            ++this.localMemsCt;
        }

        public void AddMemoryImp(string module, string fieldname, uint initialPageCt, uint minPageCt, uint maxPageCt)
        {
            this.indexingMemory.Add(IndexEntry.CreateImport(this.importMemsCt, module, fieldname));

            DefMem mem = new DefMem(this.memories.Count, initialPageCt, minPageCt, maxPageCt );
            this.memories.Add(mem);

            this.GetOrCreateRecord(module).memories.Add(fieldname, mem);
            
            ++this.importMemsCt;
        }


        ////////////////////////////////////////////////////////////////////////////////
        //
        //      Add Global Declarations
        //
        ////////////////////////////////////////////////////////////////////////////////

        public void AddGlobalLoc(Bin.TypeID type, bool mutable)
        { 
            this.indexingGlobal.Add(IndexEntry.CreateLocal(this.localGlobalCt));

            DefGlobal global = new DefGlobal(this.globals.Count, type, 1, mutable);
            this.globals.Add(global);

            ++this.localGlobalCt;
        }

        public void AddGlobalImp(string module, string fieldname, Bin.TypeID type, bool mutable)
        {
            this.indexingGlobal.Add(IndexEntry.CreateImport(this.importGlobalsCt, module, fieldname));

            DefGlobal global = new DefGlobal(this.globals.Count, type, 1, mutable);
            this.globals.Add(global);

            this.GetOrCreateRecord(module).globals.Add(fieldname, global);
        
            ++this.importGlobalsCt;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        //      Add Table Declarations
        //
        ////////////////////////////////////////////////////////////////////////////////

        public void AddTableLoc(Bin.TypeID type, uint initialElements, uint maxElements)
        { 
            this.indexingTable.Add(IndexEntry.CreateLocal(this.localTablesCt));

            DefTable table = new DefTable(this.tables.Count, type, initialElements, 0, maxElements);
            this.tables.Add(table);

            ++this.localTablesCt;
        }

        public void AddTableImp(string module, string fieldname, Bin.TypeID type, uint initialElements, uint maxElements)
        {
            this.indexingTable.Add(IndexEntry.CreateImport(this.importTablesCt, module, fieldname));
        
            DefTable table = new DefTable(this.tables.Count, type, initialElements, 0, maxElements);
            this.tables.Add(table);

            this.GetOrCreateRecord(module).tables.Add(fieldname, table);
        
            ++this.importTablesCt;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        //      Add Get Defs
        //
        ////////////////////////////////////////////////////////////////////////////////

        public DefGlobal ? GetImportGlobalDef(string module, string field)
        {
            ModuleRecord m;
            if (this.moduleLookup.TryGetValue(module, out m) == false)
                return null;

            DefGlobal dg;
            if(m.globals.TryGetValue(field, out dg) == false)
                return null;

            return dg;
        }

        public DefTable ? GetImportTableDef(string module, string field)
        {
            ModuleRecord m;
            if (this.moduleLookup.TryGetValue(module, out m) == false)
                return null;

            DefTable dt;
            if(m.tables.TryGetValue(field, out dt) == false)
                return null;

            return dt;
        }

        public DefFunction? GetImportFunctionDef(string module, string field)
        {
            ModuleRecord m;
            if (this.moduleLookup.TryGetValue(module, out m) == false)
                return null;

            DefFunction df;
            if (m.functions.TryGetValue(field, out df) == false)
                return null;

            return df;
        }

        public DefMem? GetImportMemDef(string module, string field)
        {
            ModuleRecord m;
            if (this.moduleLookup.TryGetValue(module, out m) == false)
                return null;

            DefMem dm;
            if (m.memories.TryGetValue(field, out dm) == false)
                return null;

            return dm;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        //      Enumerate Entries
        //
        ////////////////////////////////////////////////////////////////////////////////

        public IEnumerable<DefFunction> EnumerateFunctionDefs()
        { 
            foreach(KeyValuePair<string, ModuleRecord> kvp in this.moduleLookup)
            {
                foreach(KeyValuePair<string, DefFunction> df in kvp.Value.functions)
                    yield return df.Value;
            }
        }

        public IEnumerable<DefMem> EnumeratorMemories()
        {
            foreach (KeyValuePair<string, ModuleRecord> kvp in this.moduleLookup)
            {
                foreach(KeyValuePair<string, DefMem> dm in kvp.Value.memories)
                    yield return dm.Value;
            }
        }

        public IEnumerable<DefGlobal> EnumerateGlobals()
        {
            foreach (KeyValuePair<string, ModuleRecord> kvp in this.moduleLookup)
            {
                foreach(KeyValuePair<string, DefGlobal> dg in kvp.Value.globals)
                    yield return dg.Value;
            }
        }

        public IEnumerable<DefTable> EnumerateTableDefs()
        {
            foreach (KeyValuePair<string, ModuleRecord> kvp in this.moduleLookup)
            {
                foreach(KeyValuePair<string, DefTable> dt in kvp.Value.tables)
                    yield return dt.Value;
            }
        }


    }
}
