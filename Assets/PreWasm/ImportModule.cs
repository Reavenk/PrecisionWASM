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

        /// <summary>
        /// The number of imported functions.
        /// </summary>
        public int NumFunctions { get => this.storeDecl.importFunctionsCt; }

        /// <summary>
        /// The number of imported memories.
        /// </summary>
        public int NumMemories { get => this.storeDecl.importMemsCt; }

        /// <summary>
        /// The number of imported globals.
        /// </summary>
        public int NumGlobals { get => this.storeDecl.importGlobalsCt; }

        /// <summary>
        /// The number of imported tables.
        /// </summary>
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

        public void ResetToDefaults()
        { 
            this.Reset();

            foreach(IndexEntry ie in this.storeDecl.IndexingMemory)
            {
                if(ie.type == IndexEntry.FnIdxType.Local)
                    continue;

                this.memories[ie.index] = 
                    this.storeDecl.GetImportMemDef(ie.module, ie.fieldname).Value.CreateDefault(null);
            }

            foreach (IndexEntry ie in this.storeDecl.IndexingGlobal)
            {
                if (ie.type == IndexEntry.FnIdxType.Local)
                    continue;

                this.globals[ie.index] = 
                    this.storeDecl.GetImportGlobalDef(ie.module, ie.fieldname).Value.CreateDefault();
            }

            foreach (IndexEntry ie in this.storeDecl.IndexingTable)
            {
                if (ie.type == IndexEntry.FnIdxType.Local)
                    continue;

                this.tables[ie.index] = 
                    this.storeDecl.GetImportTableDef(ie.module, ie.fieldname).Value.CreateDefault(null);
            }
        }

        public bool Validate(bool throwOnErr = false)
        { 
            if(this.storeDecl.importFunctionsCt != 0)
            {
                if(this.importFn == null || this.importFn.Count != this.storeDecl.importFunctionsCt)
                {
                    if(throwOnErr == true)
                        throw new System.Exception("Module imports do not match expected definition.");

                    return false;
                }

                for(int i = 0; i < this.importFn.Count; ++i)
                { 
                    if(this.importFn[i] == null)
                    {
                        if (throwOnErr == true)
                            throw new System.Exception("Missing imported function.");

                        return false;
                    }
                }
            }

            if(this.storeDecl.importMemsCt != 0)
            { 
                if(this.memories == null || this.tables.Count != this.storeDecl.importTablesCt)
                { 
                    if(throwOnErr == true)
                        throw new System.Exception("Module tables do not match expected definition.");

                    return false;
                }

                for(int i = 0; i < this.memories.Count; ++i)
                { 
                    if(this.memories[i] == null)
                    { 
                        if(throwOnErr == true)
                            throw new System.Exception("Missing imported memory.");

                        return false;
                    }
                }
            }

            if(this.storeDecl.importGlobalsCt != 0)
            { 
                if(this.globals == null || this.globals.Count != this.storeDecl.importGlobalsCt)
                { 
                    if(throwOnErr == true)
                        throw new System.Exception("Module globals do not match expected definition.");

                    return false;
                }

                for(int i = 0; i < this.globals.Count; ++i)
                { 
                    if(this.globals[i] == null)
                    {
                        if (throwOnErr == true)
                            throw new System.Exception("Missing imported global.");

                        return false;
                    }
                }
            }

            if(this.storeDecl.importTablesCt != 0)
            { 
                if(this.tables == null || this.tables.Count != this.storeDecl.importTablesCt)
                {
                    if(throwOnErr == true)
                        throw new System.Exception("Module tables do not match expected definition.");

                    return false;
                }

                for(int i = 0; i < this.tables.Count; ++i)
                { 
                    if(this.tables[i] == null)
                    {
                        if (throwOnErr == true)
                            throw new System.Exception("Missing imported table.");

                        return false;
                    }
                }
            }

            return true;
        }

        public bool SetFunction(string module, string field, ImportFunction ifn)
        { 
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if(mr == null)
                return false;

            DefFunction df;
            if(mr.functions.TryGetValue(field, out df) == false)
                return false;

            ifn.SetFunctionType(df.fnType);

            IndexEntry ie = this.storeDecl.IndexingFunction[df.index];

            this.importFn[ie.index] = ifn;
            return true;
        }

        public bool SetMemory(string module, string field, Memory memory)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return false;

            DefMem dm;
            if (mr.memories.TryGetValue(field, out dm) == false)
                return false;

            if(memory != null)
            { 
                if(
                    dm.initialPages < memory.limits.minPages || 
                    dm.initialPages > memory.limits.maxPages ||
                    dm.limits.minPages != memory.limits.minPages ||
                    dm.limits.maxPages != memory.limits.maxPages )
                { 
                    return false;
                }
                
            }

            IndexEntry ie = this.storeDecl.IndexingMemory[dm.index];

            this.memories[ie.index] = memory;
            return true;
        }

        public bool SetGlobal(string module, string field, Global global)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return false;

            DefGlobal dg;
            if (mr.globals.TryGetValue(field, out dg) == false)
                return false;

            if(global != null)
            { 
                if(
                    dg.type != global.type || 
                    dg.mut != global.mutable)
                {
                    return false;
                }
            }

            IndexEntry ie = this.storeDecl.IndexingGlobal[dg.index];
            this.globals[ie.index] = global;
            return true;
        }

        public bool SetTable(string module, string field, Table table)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return false;

            DefTable dt;
            if (mr.tables.TryGetValue(field, out dt) == false)
                return false;

            if (table != null)
            {
                if (
                    dt.type != table.type ||
                    dt.limits.maxEntries != table.limits.maxEntries)
                {
                    return false;
                }
            }

            IndexEntry ie = this.storeDecl.IndexingTable[dt.index];
            this.tables[ie.index] = table;
            return true;
        }

        public ImportFunction GetFunction(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefFunction df;
            if (mr.functions.TryGetValue(field, out df) == false)
                return null;

            IndexEntry ie = this.storeDecl.IndexingFunction[df.index];
            return this.importFn[ie.index];
        }

        public ImportFunction GetFunction(int exportFunctionIndex)
        {
            return this.importFn[exportFunctionIndex];
        }

        public Memory GetMemory(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefMem dm;
            if (mr.memories.TryGetValue(field, out dm) == false)
                return null;

            IndexEntry ie = this.storeDecl.IndexingMemory[dm.index];
            return this.memories[ie.index];
        }


        public Global GetGlobal(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefGlobal dg;
            if (mr.globals.TryGetValue(field, out dg) == false)
                return null;

            IndexEntry ie = this.storeDecl.IndexingGlobal[dg.index];
            return this.globals[ie.index];
        }

        public Table GetTable(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefTable dt;
            if (mr.tables.TryGetValue(field, out dt) == false)
                return null;

            IndexEntry ie = this.storeDecl.IndexingTable[dt.index];
            return this.tables[ie.index];
        }

        public Memory GetMemoryOrDefault(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefMem dm;
            if (mr.memories.TryGetValue(field, out dm) == false)
                return null;

            IndexEntry ie = this.storeDecl.IndexingMemory[dm.index];

            if(this.memories[ie.index] == null)
                this.memories[ie.index] = dm.CreateDefault(null);

            return this.memories[ie.index];
        }

        public Global GetGlobalOrDefault(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefGlobal dg;
            if (mr.globals.TryGetValue(field, out dg) == false)
                return null;


            IndexEntry ie = this.storeDecl.IndexingGlobal[dg.index];
            if(this.globals[ie.index] == null)
                this.globals[ie.index] = dg.CreateDefault();

            return this.globals[ie.index];
        }

        public Table GetTableOrDefault(string module, string field)
        {
            StoreDeclarations.ModuleRecord mr = this.storeDecl.GetModuleRecord(module);
            if (mr == null)
                return null;

            DefTable dt;
            if (mr.tables.TryGetValue(field, out dt) == false)
                return null;

            IndexEntry ie = this.storeDecl.IndexingTable[dt.index];
            if(this.tables[ie.index] == null)
                this.tables[ie.index] = dt.CreateDefault(null);

            return this.tables[ie.index];
        }

        public IEnumerable<string> ModuleNames()
        { 
            return this.storeDecl.ModuleNames();
        }
    }
}
