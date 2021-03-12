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
    /// Unity tests for function systems, including imported functions.
    /// </summary>
    public class Test_Function
    {
        const string TestTheme = "Function";

        [Test]
        public void Test_multi_Add10_Reflection()
        {
            // Tests importing host functions from lambda functions
            // Test retrieving multiple return values.

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/host_multi.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);

            ex.importData.SetFunction(
                "import", 
                "DoThing", 
                new PxPre.WASM.ImportFunction_Lam(
                    (x)=>
                    { 
                        List<object> rets = new List<object>();
                        rets.Add((int)x[0] + 10);
                        rets.Add((long)x[1] + 10);
                        rets.Add((float)x[2] + 10.0f);
                        rets.Add((double)x[3] + 10.0);
                        return rets;
                    }));

            ex.InvokeStart();

            for(int i = 0; i < 10; ++i)
            {
                int paramA = 10 * i;
                long paramB = 13 * i;
                float paramC = 17.5f * i;
                double paramD = 123.456 * i;

                List<PxPre.Datum.Val> ret =
                    ex.Invoke(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(paramA),
                        PxPre.Datum.Val.Make(paramB),
                        PxPre.Datum.Val.Make(paramC),
                        PxPre.Datum.Val.Make(paramD));

                UnitUtil.CompareGaunletInt(     paramA + 10,    ret[0], "host_multi", i, paramA, paramB, paramC, paramD);
                UnitUtil.CompareGaunletInt64(   paramB + 10,    ret[1], "host_multi", i, paramA, paramB, paramC, paramD);
                UnitUtil.CompareGaunletFloat(   paramC + 10.0f, ret[2], "host_multi", i, paramA, paramB, paramC, paramD);
                UnitUtil.CompareGaunletFloat64( paramD + 10.0,  ret[3], "host_multi", i, paramA, paramB, paramC, paramD);
            }
        }

        /// <summary>
        /// The host function used in Test_host_sum4_Reflection().
        /// </summary>
        /// <param name="a">WASM function parameter</param>
        /// <param name="b">WASM function parameter</param>
        /// <param name="c">WASM function parameter</param>
        /// <param name="d">WASM function parameter</param>
        /// <returns>a + b + c + d, summed together when casted as int64s, with the final 
        /// value being that summed int64 casted to an int32.</returns>
        public static int Add4AsInt(int a, long b, float c, double d)
        {
            return (int)((long)a + (long)b + (long)c + (long)d);
        }

        [Test]
        public void Test_host_sum4_Reflection()
        {
            // Tests importing host functions using C# reflection methods.

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/host_sum4.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);

            System.Reflection.MethodInfo importedFn = typeof(Test_Function).GetMethod("Add4AsInt", System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.Public);
            ex.importData.SetFunction("import", "sum4", new PxPre.WASM.ImportFunction_Refl(importedFn));

            ex.InvokeStart();

            int paramA = 10;
            long paramB = 20;
            float paramC = 30.0f;
            double paramD = 40.0;

            int expected = (int)((long)paramA + (long)paramB + (long)paramC + (long)paramD);

            PxPre.Datum.Val ret = 
                ex.Invoke_SingleRet(
                    mod, 
                    "AddFour", 
                    PxPre.Datum.Val.Make(paramA), 
                    PxPre.Datum.Val.Make(paramB), 
                    PxPre.Datum.Val.Make(paramC), 
                    PxPre.Datum.Val.Make(paramD));

            UnitUtil.CompareGaunletInt(expected, ret, "host_sum4", 0, paramA, paramB, paramC, paramD);
        }

        [Test]
        public void Test_sum4()
        {
            // Tests taking in multiple parameters of different types.
            //
            // Implements the equivalent of Add4AsInt() in WASM, so it can be used to calculate
            // the expectation.

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/sum4.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < 10; ++i)
            {
                int paramA = 10 * i;
                long paramB = 13 * i;
                float paramC = 17.5f * i;
                double paramD = 123.456 * i;

                int expected = Add4AsInt(paramA, paramB, paramC, paramD);

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(paramA), PxPre.Datum.Val.Make(paramB), PxPre.Datum.Val.Make(paramC), PxPre.Datum.Val.Make(paramD));
                UnitUtil.CompareGaunletInt(expected, ret, "sum4", i, paramA, paramB, paramC, paramD);
            }
        }

        [Test]
        public void Test_multi_Add10()
        {
            // Tests taking in multiple parameters of different types and returning
            // multiple return values of different types.
            //
            // This is the equivalent of Test_multi_Add10_Reflection() but with the
            // math performed in WASM.

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/multi_Add10.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < 10; ++i)
            {
                int paramA = 10 * i;
                long paramB = 13 * i;
                float paramC = 17.5f * i;
                double paramD = 123.456 * i;

                List<PxPre.Datum.Val> ret = ex.Invoke(mod, "Test_Add10", PxPre.Datum.Val.Make(paramA), PxPre.Datum.Val.Make(paramB), PxPre.Datum.Val.Make(paramC), PxPre.Datum.Val.Make(paramD));

                UnitUtil.CompareGaunletInt(         paramA + 10,    ret[0], "multi_Add10", i, paramA, paramB, paramC, paramD);
                UnitUtil.CompareGaunletInt64(       paramB + 10,    ret[1], "multi_Add10", i, paramA, paramB, paramC, paramD);
                UnitUtil.CompareGaunletFloat(       paramC + 10.0f, ret[2], "multi_Add10", i, paramA, paramB, paramC, paramD);
                UnitUtil.CompareGaunletFloat64(     paramD + 10.0,  ret[3], "multi_Add10", i, paramA, paramB, paramC, paramD);
            }
        }
    }
}
