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
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

namespace Tests
{
    public class Test_GauntletFloatMath
    {
        const string TestTheme = "FloatMath";

        public static float[] floatTestValues =
            new float[] { float.NaN, float.NegativeInfinity, -9999.1234f, -45.5f, -45.0f, -0.75f, -0.0f, 0.0f, 0.75f, 45.0f, 45.5f, 9999.1234f, float.PositiveInfinity };

        public static long[] longTestValues =
            new long[] { long.MinValue, -123456789123, -12345, 0, 1, 5, 12345, 123456789123, long.MaxValue };

        ulong[] ulongTestValues =
            new ulong[] {ulong.MinValue,  0, 10, 55, 999, 4294967296, 6442450944, 18446744073709551615, ulong.MaxValue };

        double[] doubleTestValues =
            new double[] { double.NaN, double.MinValue, -1234567890123.1234, -45.5, -45, -0.75, -0.0, 0.0, 0.75, 45, 45.5, 1234567890123.1234, double.MaxValue };

        int[] intTestValues =
            new int[] { int.MinValue, -999, -45, 0, 45, 999, int.MaxValue };

        uint[] uintTestValues =
            new uint[] { uint.MinValue, 0, 1, 10, 55, 999, 123456789, 4294967295, uint.MaxValue };

        public static void ThrowGauntletFloatError(string testName, int testId, string reason, float expected, float result, params float[] operands)
        {
            throw new System.Exception($"Invalid return value for {testName}, test {testId} with operands ({string.Join(", ", operands)}): {reason}.");
        }

        [Test]
        public void Test_f32_abs()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.abs.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)System.Math.Abs(tv), ret, "f32.abs", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f32_neg()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.neg.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret =  ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat(-tv, ret, "f32.neg", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f32_ceil()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.ceil.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)System.Math.Ceiling(tv), ret, "f32.ceil", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f32_floor()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.floor.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)System.Math.Floor(tv), ret, "f32.ceil", idx++ , tv);
            }
        }
        
        [Test]
        public void Test_f32_trunc()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.trunc.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)System.Math.Truncate(tv), ret, "f32.trunc", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f32_nearest()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.nearest.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)System.Math.Round(tv), ret, "f64.nearest", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f32_sqrt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.sqrt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)System.Math.Sqrt(tv), ret, "f32.sqrt", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_abs()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.abs.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(System.Math.Abs(tv), ret, "f64.abs", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_neg()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.neg.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(-tv, ret, "f64.neg", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_ceil()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.ceil.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(System.Math.Ceiling(tv), ret, "f64.ceil", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_floor()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.floor.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(System.Math.Floor(tv), ret, "f64.floor", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_trunc()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.trunc.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(System.Math.Truncate(tv), ret, "f64.trunc", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_nearest()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.nearest.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(System.Math.Round(tv), ret, "f64.nearest", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f64_sqrt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.sqrt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(System.Math.Sqrt(tv), ret, "f64.abs", idx++, tv);
            }
        }
        
        [Test]
        public void Test_f32_add()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.add.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (float tva in floatTestValues)
            {
                foreach (float tvb in floatTestValues)
                {
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                        throw new System.Exception("Invalid return type.");

                    if (float.IsNaN(tva) == true || float.IsNaN(tvb) == true)
                    {
                        if (float.IsNaN(ret.GetFloat()) == false)
                            throw new System.Exception("Invalid return value.");
                    }
                    else if(
                        // inf + -inf = NaN
                        float.IsInfinity(tva) == true && 
                        float.IsInfinity(tvb) == true &&
                        tva != tvb)
                    {
                        if (float.IsNaN(ret.GetFloat()) == false)
                            throw new System.Exception("Invalid return value.");
                    }
                    else if (ret.GetFloat() != tva + tvb)
                        throw new System.Exception("Invalid return value.");
                }
            }
        }
        
        [Test]
        public void Test_f32_sub()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.sub.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tva in floatTestValues)
            {
                foreach (float tvb in floatTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat(tva - tvb, ret, "f32.mul", idx++, tva, tvb);
                }
            }
        }
        
        [Test]
        public void Test_f32_mul()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.mul.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tva in floatTestValues)
            {
                foreach (float tvb in floatTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat(tva * tvb, ret, "f32.mul", idx++, tva, tvb);
                }
            }
        }
        
        [Test]
        public void Test_f32_div()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.div.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tva in floatTestValues)
            {
                foreach (float tvb in floatTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat(tva / tvb, ret, "f32.div", idx++, tva, tvb);
                }
            } 
        }
        
        [Test]
        public void Test_f32_min()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.min.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<FloatTrippplet> testItems =
                new List<FloatTrippplet>
                { 
                    // NaN checks
                    new FloatTrippplet(float.NaN,               float.NaN,                  float.NaN),
                    new FloatTrippplet(float.PositiveInfinity,  float.NaN,                  float.NaN),
                    new FloatTrippplet(float.NegativeInfinity,  float.NaN,                  float.NaN),
                    new FloatTrippplet(float.NaN,               float.PositiveInfinity,     float.NaN),
                    new FloatTrippplet(float.NaN,               float.NegativeInfinity,     float.NaN),
                    new FloatTrippplet(float.NaN,               1234.567f,                  float.NaN),
                    new FloatTrippplet(8912.456f,               float.NaN,                  float.NaN),
                    // Negative Infinity checks
                    new FloatTrippplet(float.NegativeInfinity,  float.NegativeInfinity,     float.NegativeInfinity),
                    new FloatTrippplet(float.NegativeInfinity,  56.78f,                     float.NegativeInfinity),
                    new FloatTrippplet(float.NegativeInfinity,  -90.12f,                    float.NegativeInfinity),
                    new FloatTrippplet(90.12f,                  float.NegativeInfinity,     float.NegativeInfinity),
                    // Positive Infinity checks
                    new FloatTrippplet(float.PositiveInfinity,  float.PositiveInfinity,     float.PositiveInfinity),
                    new FloatTrippplet(1234.567f,               float.PositiveInfinity,     1234.567f),
                    new FloatTrippplet(-8912.345f,              float.PositiveInfinity,     -8912.345f),
                    new FloatTrippplet(float.PositiveInfinity,  1234.567f,                  1234.567f),
                    new FloatTrippplet(float.PositiveInfinity,  -8912.345f,                 -8912.345f),
                    // Misc Normal checks
                    new FloatTrippplet(0.0f,                    0.0f,                       0.0f),
                    new FloatTrippplet(50.0f,                   0.0f,                       0.0f),
                    new FloatTrippplet(-50.0f,                  0.0f,                       -50.0f),
                    new FloatTrippplet(78.9f,                   12.3f,                      12.3f),
                    new FloatTrippplet(-78.9f,                  -12.3f,                     -78.9f),
                };

            for(int i = 0; i < testItems.Count; ++i)
            { 
                FloatTrippplet ft = testItems[i];
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ft.a), PxPre.Datum.Val.Make(ft.b));
                UnitUtil.CompareGaunletFloat(ft.c, ret, "f32.copysign", i, ft.a, ft.b);
            }
        }
        
        [Test]
        public void Test_f32_max()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.max.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<FloatTrippplet> testItems =
                new List<FloatTrippplet>
                { 
                    // NaN checks
                    new FloatTrippplet(float.NaN,               float.NaN,                  float.NaN),
                    new FloatTrippplet(float.PositiveInfinity,  float.NaN,                  float.NaN),
                    new FloatTrippplet(float.NegativeInfinity,  float.NaN,                  float.NaN),
                    new FloatTrippplet(float.NaN,               float.PositiveInfinity,     float.NaN),
                    new FloatTrippplet(float.NaN,               float.NegativeInfinity,     float.NaN),
                    new FloatTrippplet(float.NaN,               1234.567f,                  float.NaN),
                    new FloatTrippplet(8912.456f,               float.NaN,                  float.NaN),
                    // Negative Infinity checks
                    new FloatTrippplet(float.NegativeInfinity,  float.NegativeInfinity,     float.NegativeInfinity),
                    new FloatTrippplet(float.NegativeInfinity,  56.78f,                     56.78f),
                    new FloatTrippplet(float.NegativeInfinity,  -90.12f,                    -90.12f),
                    new FloatTrippplet(90.12f,                  float.NegativeInfinity,     90.12f),
                    // Positive Infinity checks
                    new FloatTrippplet(float.PositiveInfinity,  float.PositiveInfinity,     float.PositiveInfinity),
                    new FloatTrippplet(1234.567f,               float.PositiveInfinity,     float.PositiveInfinity),
                    new FloatTrippplet(-8912.345f,              float.PositiveInfinity,     float.PositiveInfinity),
                    new FloatTrippplet(float.PositiveInfinity,  1234.567f,                  float.PositiveInfinity),
                    new FloatTrippplet(float.PositiveInfinity,  -8912.345f,                 float.PositiveInfinity),
                    // Misc Normal checks
                    new FloatTrippplet(0.0f,                    0.0f,                       0.0f),
                    new FloatTrippplet(50.0f,                   0.0f,                       50.0f),
                    new FloatTrippplet(-50.0f,                  0.0f,                       0.0f),
                    new FloatTrippplet(78.9f,                   12.3f,                      78.9f),
                    new FloatTrippplet(-78.9f,                  -12.3f,                     -12.3f),
                };

            for (int i = 0; i < testItems.Count; ++i)
            {
                FloatTrippplet ft = testItems[i];
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ft.a), PxPre.Datum.Val.Make(ft.b));
                UnitUtil.CompareGaunletFloat(ft.c, ret, "f32.max", i, ft.a, ft.b);
            }
        }
        
        [Test]
        public void Test_f32_copysign()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.copysign.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<FloatTrippplet> testItems = 
                new List<FloatTrippplet>
                { 
                    // Simple zero check
                    new FloatTrippplet(0.0f,        0.0f,           0.0f),
                    // NaN as magnitude returning NaN check
                    new FloatTrippplet(float.NaN,   float.NaN,      float.NaN),
                    new FloatTrippplet(float.NaN,   0.0f,           float.NaN),
                    new FloatTrippplet(float.NaN,   20.1f,          float.NaN),
                    new FloatTrippplet(float.NaN,   -20.1f,         float.NaN),
                    // NaN as sign returning valid value check
                    // I'm having trouble coding in negative NaNs, (actually, positive ones)
                    // so these tests are going to be disabled ATM
                    // (wleu 03/01/2021)
                    //new FloatTrippplet(99.99f,      float.NaN,      99.99f),
                    //new FloatTrippplet(99.99f,      -float.NaN,     -99.99f),
                    // Negative zero check
                    new FloatTrippplet(0.0f,        1.23f,          0.0f),
                    new FloatTrippplet(0.0f,        -1.23f,         -0.0f),
                    // Misc other values
                    new FloatTrippplet(88.88f,      20.1f,          88.88f),
                    new FloatTrippplet(88.88f,      -21.2f,         -88.88f),
                    new FloatTrippplet(77.77f,      22.3f,          77.77f),
                    new FloatTrippplet(77.77f,      -22.4f,         -77.77f),
                    //
                    new FloatTrippplet(-88.88f,      20.1f,          88.88f),
                    new FloatTrippplet(-88.88f,      -21.2f,         -88.88f),
                    new FloatTrippplet(-77.77f,      22.3f,          77.77f),
                    new FloatTrippplet(-77.77f,      -22.4f,         -77.77f),
                };


            int idx = 0;
            foreach (FloatTrippplet ft in testItems)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ft.a), PxPre.Datum.Val.Make(ft.b));
                UnitUtil.CompareGaunletFloat(ft.c, ret, "f32.copysign", idx++, ft.a, ft.b);
            }
        }
        
        [Test]
        public void Test_f64_add()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.add.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tva in doubleTestValues)
            {
                foreach (double tvb in doubleTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat64(tva + tvb, ret, "f64.add", idx++, tva, tvb);
                }
            }
        }
        
        [Test]
        public void Test_f64_sub()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.sub.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tva in doubleTestValues)
            {
                foreach (double tvb in doubleTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat64(tva - tvb, ret, "f64.sub", idx++, tva, tvb);
                }
            }
        }
        
        [Test]
        public void Test_f64_mul()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.mul.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tva in doubleTestValues)
            {
                foreach (double tvb in doubleTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat64(tva * tvb, ret, "f64.mul", idx++, tva, tvb);
                }
            }
        }
        
        [Test]
        public void Test_f64_div()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.div.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tva in doubleTestValues)
            {
                foreach (double tvb in doubleTestValues)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(tva), PxPre.Datum.Val.Make(tvb));
                    UnitUtil.CompareGaunletFloat64(tva / tvb, ret, "f64.div", idx++, tva, tvb);
                }
            }
        }
        
        [Test]
        public void Test_f64_min()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.min.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<DoubleTripplet> testItems =
                new List<DoubleTripplet>
                { 
                    // NaN checks
                    new DoubleTripplet(float.NaN,               float.NaN,                  float.NaN),
                    new DoubleTripplet(float.PositiveInfinity,  float.NaN,                  float.NaN),
                    new DoubleTripplet(float.NegativeInfinity,  float.NaN,                  float.NaN),
                    new DoubleTripplet(float.NaN,               float.PositiveInfinity,     float.NaN),
                    new DoubleTripplet(float.NaN,               float.NegativeInfinity,     float.NaN),
                    new DoubleTripplet(float.NaN,               1234.567,                   float.NaN),
                    new DoubleTripplet(8912.456,                float.NaN,                  float.NaN),
                    // Negative Infinity checks
                    new DoubleTripplet(float.NegativeInfinity,  float.NegativeInfinity,     float.NegativeInfinity),
                    new DoubleTripplet(float.NegativeInfinity,  56.78,                      float.NegativeInfinity),
                    new DoubleTripplet(float.NegativeInfinity,  -90.12,                     float.NegativeInfinity),
                    new DoubleTripplet(90.12,                   float.NegativeInfinity,     float.NegativeInfinity),
                    // Positive Infinity checks
                    new DoubleTripplet(float.PositiveInfinity,  float.PositiveInfinity,     float.PositiveInfinity),
                    new DoubleTripplet(1234.567,                float.PositiveInfinity,      1234.567),
                    new DoubleTripplet(-8912.345,               float.PositiveInfinity,      -8912.345),
                    new DoubleTripplet(float.PositiveInfinity,  1234.567,                   1234.567),
                    new DoubleTripplet(float.PositiveInfinity,  -8912.345,                  -8912.345),
                    // Misc Normal checks
                    new DoubleTripplet(0.0,                     0.0,                        0.0),
                    new DoubleTripplet(50.0,                    0.0,                        0.0),
                    new DoubleTripplet(-50.0,                   0.0f,                       -50.0),
                    new DoubleTripplet(78.9,                    12.3,                       12.3),
                    new DoubleTripplet(-78.9,                  -12.3,                       -78.9),
                };

            int idx = 0;
            foreach (DoubleTripplet dt in testItems)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(dt.a), PxPre.Datum.Val.Make(dt.b));
                UnitUtil.CompareGaunletFloat64(dt.c, ret, "f64.min", idx++, dt.a, dt.b);
            }
        }
        
        [Test]
        public void Test_f64_max()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.max.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<DoubleTripplet> testItems =
                new List<DoubleTripplet>
                { 
                    // NaN checks
                    new DoubleTripplet(double.NaN,                  double.NaN,                double.NaN),
                    new DoubleTripplet(double.PositiveInfinity,     double.NaN,                double.NaN),
                    new DoubleTripplet(double.NegativeInfinity,     double.NaN,                double.NaN),
                    new DoubleTripplet(double.NaN,                  double.PositiveInfinity,   double.NaN),
                    new DoubleTripplet(double.NaN,                  double.NegativeInfinity,   double.NaN),
                    new DoubleTripplet(double.NaN,                  1234.567,                  double.NaN),
                    new DoubleTripplet(8912.456,                    double.NaN,                double.NaN),
                    // Negative Infinity checks
                    new DoubleTripplet(double.NegativeInfinity,     double.NegativeInfinity,   double.NegativeInfinity),
                    new DoubleTripplet(double.NegativeInfinity,     56.78,                     56.78),
                    new DoubleTripplet(double.NegativeInfinity,     -90.12,                    -90.12),
                    new DoubleTripplet(90.12,                       double.NegativeInfinity,   90.12),
                    // Positive Infinity checks
                    new DoubleTripplet(double.PositiveInfinity,     double.PositiveInfinity,    double.PositiveInfinity),
                    new DoubleTripplet(1234.567,                    double.PositiveInfinity,    double.PositiveInfinity),
                    new DoubleTripplet(-8912.345,                   double.PositiveInfinity,    double.PositiveInfinity),
                    new DoubleTripplet(double.PositiveInfinity,     1234.567,                   double.PositiveInfinity),
                    new DoubleTripplet(double.PositiveInfinity,     -8912.345,                  double.PositiveInfinity),
                    // Misc Normal checks
                    new DoubleTripplet(0.0,                         0.0f,                       0.0f),
                    new DoubleTripplet(50.0,                        0.0f,                       50.0),
                    new DoubleTripplet(-50.0,                       0.0f,                       0.0),
                    new DoubleTripplet(78.9,                        12.3,                      78.9),
                    new DoubleTripplet(-78.9,                       -12.3,                     -12.3),
                };

            int idx = 0;
            foreach (DoubleTripplet dt in testItems)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(dt.a), PxPre.Datum.Val.Make(dt.b));
                UnitUtil.CompareGaunletFloat64(dt.c, ret, "f64.max", idx++, dt.a, dt.b);
            }
        }
        
        [Test]
        public void Test_f64_copysign()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.copysign.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<DoubleTripplet> testItems =
                new List<DoubleTripplet>
                { 
                    // Simple zero check
                    new DoubleTripplet(0.0f,            0.0,            0.0),
                    // NaN as magnitude returning NaN check
                    new DoubleTripplet(double.NaN,      double.NaN,     double.NaN),
                    new DoubleTripplet(double.NaN,      0.0,            double.NaN),
                    new DoubleTripplet(double.NaN,      20.1,           double.NaN),
                    new DoubleTripplet(double.NaN,      -20.1,          double.NaN),
                    // Negative zero check
                    new DoubleTripplet(0.0,             1.23,           0.0f),
                    new DoubleTripplet(0.0,             -1.23,          -0.0f),
                    // Misc other values
                    new DoubleTripplet(88.88,           20.1,           88.88),
                    new DoubleTripplet(88.88,           -21.2,          -88.88),
                    new DoubleTripplet(77.77,           22.3,           77.77),
                    new DoubleTripplet(77.77,           -22.4,          -77.77),
                    //
                    new DoubleTripplet(-88.88,          20.1,           88.88),
                    new DoubleTripplet(-88.88,          -21.2,          -88.88),
                    new DoubleTripplet(-77.77,          22.3,           77.77),
                    new DoubleTripplet(-77.77,          -22.4,          -77.77),
                };

            int idx = 0;
            foreach (DoubleTripplet dt in testItems)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(dt.a), PxPre.Datum.Val.Make(dt.b));
                UnitUtil.CompareGaunletFloat64(dt.c, ret, "f64.copysign", idx++, dt.a, dt.b);
            }
        }
    }
}