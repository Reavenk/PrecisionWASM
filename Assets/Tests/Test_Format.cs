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

using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
    /// <summary>
    /// Unit tests to make sure the WASM format is parsing correctly.
    /// </summary>
    public class Test_Format
    {
        const string TestTheme = "Format";

        [Test]
        public void Test_ImportMemories()
        {
            string testName = "import_memories";

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/{testName}.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, true);

            if(mod.storeDecl.importGlobalsCt != 3)
                throw new System.Exception($"{testName} expected to have 2 imported globals.");

            if (mod.storeDecl.importMemsCt != 1)
                throw new System.Exception($"{testName} expected to have 1 imported memory.");

            if(mod.storeDecl.importTablesCt != 1)
                throw new System.Exception($"{testName} expected to have 1 imported table.");

            HashSet<string> mods = new HashSet<string>(mod.storeDecl.ModuleNames());
            if(mods.Count != 1 || mods.Contains("foo") == false)
                throw new System.Exception($"{testName} expected to have 1 imported module named foo.");

            PxPre.WASM.DefGlobal? g0 = mod.storeDecl.GetImportGlobalDef("foo", "global_0");
            if(g0.HasValue == false || g0.Value.mut != PxPre.WASM.Global.Mutability.Const || g0.Value.type != PxPre.WASM.Bin.TypeID.Int32)
                throw new System.Exception($"{testName} error check with global_0");

            PxPre.WASM.DefGlobal? g1 = mod.storeDecl.GetImportGlobalDef("foo", "global_1");
            if (g1.HasValue == false || g1.Value.mut != PxPre.WASM.Global.Mutability.Const || g1.Value.type != PxPre.WASM.Bin.TypeID.Float64)
                throw new System.Exception($"{testName} error check with global_1");

            PxPre.WASM.DefGlobal? g2 = mod.storeDecl.GetImportGlobalDef("foo", "global_2");
            if (g2.HasValue == false || g2.Value.mut != PxPre.WASM.Global.Mutability.Variable || g2.Value.type != PxPre.WASM.Bin.TypeID.Int64)
                throw new System.Exception($"{testName} error check with global_2");

            PxPre.WASM.DefMem? m = mod.storeDecl.GetImportMemDef("foo", "mem");
            if(m.HasValue == false || m.Value.limits.minPages != 10 || m.Value.limits.maxPages != 10)
                throw new System.Exception($"{testName} error check with memory mem");

            PxPre.WASM.DefTable? t = mod.storeDecl.GetImportTableDef("foo", "table");
            if(t.HasValue == false || t.Value.type != PxPre.WASM.Bin.TypeID.FuncRef || t.Value.limits.minEntries != 5 || t.Value.limits.maxEntries != 20)
                throw new System.Exception($"{testName} error check with table table");
        }
    }
}