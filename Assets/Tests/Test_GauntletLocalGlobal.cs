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
    public class Test_GauntletLocalGlobal
    {
        const string TestTheme = "LocalGlobal";

        [Test]
        public void Test_GlobalMAD()
        {

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/GlobalMAD.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            if(ex.importData.Validate() == true)
                throw new System.Exception("GlobalMAD Validate let an empty Global through.");

            if(ex.importData.NumGlobals != 3)
                throw new System.Exception("GlobalMAD didn't load the correct number of globals.");

            List<string> moduleNames = new List<string>(ex.importData.ModuleNames());
            if(moduleNames.Count != 1 || moduleNames[0] != "env")
                throw new System.Exception("GlobalMAD didn't load globals module correctly.");

            // For now we don't expose a reflection to all the import entries so we just
            // blindly use these.
            PxPre.WASM.GlobalInt gAdd = new PxPre.WASM.GlobalInt(0, true);
            PxPre.WASM.GlobalInt gMul = new PxPre.WASM.GlobalInt(0, true);
            PxPre.WASM.GlobalInt gRes = new PxPre.WASM.GlobalInt(0, true);

            ex.importData.SetGlobal("env", "add", gAdd);
            ex.importData.SetGlobal("env", "mul", gMul);
            ex.importData.SetGlobal("env", "res", gRes);

            if (ex.importData.Validate() == false)
                throw new System.Exception("GlobalMAD Validate expecteded to pass but didn't.");

            for(int i = 0; i < 5; ++i)
            { 
                for(int j = 0; j < 15; j += 3)
                { 
                    for(int k = 60; k < 65; ++k)
                    { 
                        gAdd.Value = j;
                        gMul.Value = k;

                        List<PxPre.Datum.Val> rets = 
                            ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i));

                        if(rets.Count != 0)
                            throw new System.Exception("GlobalMAD Validate expecteded to pass but didn't.");

                        int expected = i * j + k;
                        if (gRes.Value != expected)
                            throw new System.Exception($"GlobalMAD failed for {i} * {j} + {k}, expected {expected} but got {gRes.Value}");
                    }
                }
            }
        }

        [Test]
        public void Test_local_tee()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/local.tee.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for(int i = 0; i < 5; ++i)
            { 
                for(int j = 50; j < 60; ++j)
                { 
                    List<PxPre.Datum.Val> rets = ex.Invoke(mod, "Testi32", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(j));
                    UnitUtil.CompareGaunletInt(i + j, rets[0], "Testi32[0]", idx, i, j);
                    UnitUtil.CompareGaunletInt(i, rets[1], "Testi32[1]", idx, i, j);
                    UnitUtil.CompareGaunletInt(i, rets[2], "Testi32[2]", idx, i, j);
                    ++idx;
                }
            }

            // The same bounds as the int test directly above, but prefixed
            // with "1234567890123" to really drive home how they're longs.
            for (long i = 12345678901230; i < 1234567890123; ++i) 
            {
                for (long j = 123456789012350; j < 123456789012360; ++j)
                {
                    List<PxPre.Datum.Val> rets = ex.Invoke(mod, "Testi64", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(j));
                    UnitUtil.CompareGaunletInt64(i + j, rets[0], "Testi64[0]", idx, i, j);
                    UnitUtil.CompareGaunletInt64(i, rets[1], "Testi64[1]", idx, i, j);
                    UnitUtil.CompareGaunletInt64(i, rets[2], "Testi64[2]", idx, i, j);
                    ++idx;
                }
            }

            for (float i = 0.0f; i < 3.0f; i += 0.25f)
            {
                for (float j = 50.0f; j < 66.0f; j += 3.33333f)
                {
                    List<PxPre.Datum.Val> rets = ex.Invoke(mod, "Testf32", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(j));
                    UnitUtil.CompareGaunletFloat(i + j, rets[0], "Testf32[0]", idx, i, j);
                    UnitUtil.CompareGaunletFloat(i, rets[1], "Testf32[1]", idx, i, j);
                    UnitUtil.CompareGaunletFloat(i, rets[2], "Testf32[2]", idx, i, j);
                    ++idx;
                }
            }

            for (double i = 0.0f; i < 3.0; i += 0.25)
            {
                for (double j = 50; j < 70; j += 3.33333)
                {
                    List<PxPre.Datum.Val> rets = ex.Invoke(mod, "Testf64", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(j));
                    UnitUtil.CompareGaunletFloat64(i + j, rets[0], "Testf64[0]", idx, i, j);
                    UnitUtil.CompareGaunletFloat64(i, rets[1], "Testf64[1]", idx, i, j);
                    UnitUtil.CompareGaunletFloat64(i, rets[2], "Testf64[2]", idx, i, j);
                    ++idx;
                }
            }
        }
    }
}
