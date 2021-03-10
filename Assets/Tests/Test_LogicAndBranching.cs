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
    /// Unit tests for logic operators, and operations that jump the instruction
    /// pointer.
    /// </summary>
    public class Test_LogicAndBranching
    {
        const string TestTheme = "Branching";

        static int RandSeed = 50;

        // When RandSeed is used for srand() on https://webassembly.studio/, these
        // are the first few entries of rand().
        //
        // Rand() makes the test more dense, but is necessary or else lots of branching
        // gets optimized out by their compilers - their cursed effective and optimal
        // compilers!
        static int[] RandTestKeys = 
            new int[] 
            { 
                1943526997, 
                693048727, 
                1850007113, 
                1925127658, 
                2054429178, 
                498528091, 
                848882219 
            };

        public int ForLoop_RefImpl(int idx)
        {
            float f = 2.111f;
            for (int i = 0; i < 20; ++i)
            {
                f *= 2.0f;
                f += (float)i;
            }
            return (int)f + idx;
        }

        [Test]
        public void Test_Rand()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/Rand.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            UnitUtil.ValidateExportedGlobal_i32(ex, "__heap_base",  66576);
            UnitUtil.ValidateExportedGlobal_i32(ex, "__data_end",   1032);


            ex.Invoke(mod, "srand", PxPre.Datum.Val.Make(RandSeed));

            int idx = 0;
            foreach(int k in RandTestKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "rand");
                UnitUtil.CompareGaunletInt(k, ret, "Rand", idx++);
            }
            
        }

        [Test]
        public void Test_ForLoop()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/ForLoop.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int expected = ForLoop_RefImpl(10);
            PxPre.Datum.Val rets = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(10), PxPre.Datum.Val.Make(10));
        }

        [Test]
        public void Test_IfElseChain()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/IfElseChain.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            UnitUtil.ValidateExportedGlobal_i32(ex, "__heap_base", 66576);
            UnitUtil.ValidateExportedGlobal_i32(ex, "__data_end", 1032);

            ex.Invoke(mod, "srand", PxPre.Datum.Val.Make(RandSeed));
            PxPre.Datum.Val ret;
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(0, ret, "IfElseChain", 0, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(1, ret, "IfElseChain", 1, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(2, ret, "IfElseChain", 2, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(3, ret, "IfElseChain", 3, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(4, ret, "IfElseChain", 4, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);

            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(-1, ret, "IfElseChain", 5, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(RandTestKeys[0]), PxPre.Datum.Val.Make(RandTestKeys[1]), PxPre.Datum.Val.Make(RandTestKeys[2]), PxPre.Datum.Val.Make(RandTestKeys[3]), PxPre.Datum.Val.Make(RandTestKeys[4]));
            UnitUtil.CompareGaunletInt(-1, ret, "IfElseChain", 6, RandTestKeys[0], RandTestKeys[1], RandTestKeys[2], RandTestKeys[3], RandTestKeys[4]);

        }

        [Test]
        public void Test_br_table()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/br_table.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val ret;
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(0), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(3, ret, "br_table", 0, 0, 3, 6, 9, 12);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(1), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(6, ret, "br_table", 1, 1, 3, 6, 9, 12);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(2), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(9, ret, "br_table", 2, 2, 3, 6, 9, 12);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(12, ret, "br_table", 3, 3, 3, 6, 9, 12);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(4), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(-1, ret, "br_table", 4, 4, 3, 6, 9, 12);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(100), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(-1, ret, "br_table", 5, 100, 3, 6, 9, 12);
            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(-100), PxPre.Datum.Val.Make(3), PxPre.Datum.Val.Make(6), PxPre.Datum.Val.Make(9), PxPre.Datum.Val.Make(12));
            UnitUtil.CompareGaunletInt(-1, ret, "br_table", 6, -100, 3, 6, 9, 12);
        }

        [Test]
        public void Test_Switch()
        {
            // This is a misnomer - the C++ code used to generate the WAT was a siwtch, but 
            // it's more of a table lookup. The code for Test_br_table is more of an authentic
            // switch branching.

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/Switch.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<IntPair> testKeys = 
                new List<IntPair>
                { 
                    new IntPair(0, 10),
                    new IntPair(1, 13),
                    new IntPair(2, 17),
                    new IntPair(3, 20),
                    new IntPair(4, 100),
                    new IntPair(5, 1000),
                    new IntPair(6, -1),
                    new IntPair(-100, -1),
                    new IntPair(100, -1)
                };

            int idx = 0;
            foreach(IntPair ip in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ip.a));
                UnitUtil.CompareGaunletInt(ip.b, ret, "Switch", idx++, ip.a);
            }
        }
    }
}