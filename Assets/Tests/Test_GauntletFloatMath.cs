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

        public static void RunBiNOpGaunletThroughTripplet(
            PxPre.WASM.ExecutionContext exProgInst, 
            PxPre.WASM.Module mod,
            FloatTrippplet ft, 
            string testName, 
            int testID)
        {
            Debug.Log( $"Running binop float test for {testName}, test number {testID}");

            PxPre.Datum.Val ret =
            exProgInst.Invoke_SingleRet(
                mod,
                "Test",
                PxPre.Datum.Val.Make(ft.a),
                PxPre.Datum.Val.Make(ft.b));

            if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                throw new System.Exception("Invalid return type : expected float.");

            float fret = ret.GetFloat();
            CompareGaunletFloat(ft.c, fret, testName, testID, ft.a, ft.b);
        }

        public static void RunBiNOpGaunletThroughTripplet(
            PxPre.WASM.ExecutionContext exProgInst,
            PxPre.WASM.Module mod,
            DoubleTripplet dt,
            string testName,
            int testID)
        {
            Debug.Log($"Running binop float test for {testName}, test number {testID}");

            PxPre.Datum.Val ret =
            exProgInst.Invoke_SingleRet(
                mod,
                "Test",
                PxPre.Datum.Val.Make(dt.a),
                PxPre.Datum.Val.Make(dt.b));

            if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                throw new System.Exception("Invalid return type : expected float64.");

            double fret = ret.GetFloat64();

            CompareGaunletDouble(dt.c, fret, testName, testID, dt.a, dt.b);
                
        }

        public static void CompareGaunletFloat(float expected, float result, string testName, int testId, params float [] operands)
        {
            if (float.IsNaN(expected) == true)
            {
                if (float.IsNaN(result) == false)
                    throw new System.Exception($"Invalid return value for {testName} : Mishandled NaN.");
            }
            else if (expected != result)
                throw new System.Exception($"Invalid return value for {testName} : Unexpected result, expected {expected}, received {result}.");
        }

        public static void CompareGaunletFloat( float expected, PxPre.Datum.Val valRes, string testName, int testId, params float [] operands)
        {
            if(valRes.wrapType != PxPre.Datum.Val.Type.Float)
                throw new System.Exception($"Invalid return value for {testName} : Expected float return.");
            
            CompareGaunletFloat(expected, valRes.GetFloat(), testName, testId, operands);
        }

        public static void ThrowGauntletDoubleError(string testName, int testId, string reason, double expected, double result, params double[] operands)
        {
            throw new System.Exception($"Invalid return value for {testName}, test {testId} with operands ({string.Join(", ", operands)}): {reason}.");
        }

        public static void CompareGaunletDouble(double expected, double result, string testName, int testId, params double [] operands)
        {
            if (double.IsNaN(expected) == true)
            {
                if (double.IsNaN(result) == false)
                    ThrowGauntletDoubleError(testName, testId, "Mishandled NaN", expected, result, operands);
            }
            else if (expected != result)
            {
                ThrowGauntletDoubleError(
                    testName, 
                    testId, 
                    $"Expected {expected} but got {result}", 
                    expected, 
                    result, 
                    operands);
            }
        }

        public static void CompareGaunletDouble(double expected, PxPre.Datum.Val valRes, string testName, int testId, params double[] operands)
        {
            if (valRes.wrapType != PxPre.Datum.Val.Type.Float64)
                ThrowGauntletDoubleError(testName, testId, "Invalid return type, expected Float64", expected, valRes.GetFloat64(), operands);

            CompareGaunletDouble(expected, valRes.GetFloat64(), testName, testId, operands);
        }



        [Test]
        public void Test_f32_abs()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.abs.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for(int i = 0; i < floatTestValues.Length; ++i)
            {
                float tv = floatTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletFloat((float)System.Math.Abs(tv), ret.GetFloat(), "f32.abs", i, tv);
            }
        }
        
        [Test]
        public void Test_f32_neg()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.neg.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < floatTestValues.Length; ++i)
            {
                float tv = floatTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletFloat(-tv, ret.GetFloat(), "f32.neg", i, tv);
            }
        }
        
        [Test]
        public void Test_f32_ceil()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.ceil.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < floatTestValues.Length; ++i)
            {
                float tv = floatTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletFloat((float)System.Math.Ceiling(tv), ret.GetFloat(), "f32.ceil", i, tv);
            }
        }
        
        [Test]
        public void Test_f32_floor()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.floor.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                if (float.IsNaN(tv) == true)
                {
                    if (float.IsNaN(ret.GetFloat()) == false)
                        throw new System.Exception("Invalid return value : Mishandled NaN.");
                }
                // Not only do we give leniency for FPE, we need to also check 
                // exact equality for handling infinity operations.
                else if (ret.GetFloat() != tv && UnitUtil.FloatEpsilon( ret.GetFloat(), Mathf.Floor(tv)) == false)
                    throw new System.Exception("Invalid return value.");
            }
        }
        
        [Test]
        public void Test_f32_trunc()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.trunc.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < floatTestValues.Length; ++i)
            {
                float tv = floatTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletFloat((float)System.Math.Truncate(tv), ret.GetFloat(), "f32.trunc", i, tv);
            }
        }
        
        [Test]
        public void Test_f32_nearest()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.nearest.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < floatTestValues.Length; ++i)
            {
                float tv = floatTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletFloat((float)System.Math.Round(tv), ret, "f64.nearest", i, tv);
            }
        }
        
        [Test]
        public void Test_f32_sqrt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.sqrt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < floatTestValues.Length; ++i)
            {
                float tv = floatTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletFloat((float)System.Math.Sqrt(tv), ret, "f32.sqrt", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_abs()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.abs.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < doubleTestValues.Length; ++i)
            {
                double tv = doubleTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletDouble(System.Math.Abs(tv), ret, "f64.abs", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_neg()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.neg.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < doubleTestValues.Length; ++i)
            {
                double tv = doubleTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletDouble(-tv, ret, "f64.neg", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_ceil()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.ceil.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < doubleTestValues.Length; ++i)
            {
                double tv = doubleTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletDouble(System.Math.Ceiling(tv), ret, "f64.ceil", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_floor()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.floor.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < doubleTestValues.Length; ++i)
            {
                double tv = doubleTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletDouble(System.Math.Floor(tv), ret, "f64.floor", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_trunc()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.trunc.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < doubleTestValues.Length; ++i)
            {
                double tv = doubleTestValues[i];

                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletDouble(System.Math.Truncate(tv), ret, "f64.trunc", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_nearest()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.nearest.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < doubleTestValues.Length; ++i)
            {
                double tv = doubleTestValues[i];
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                CompareGaunletDouble(System.Math.Round(tv), ret, "f64.nearest", i, tv);
            }
        }
        
        [Test]
        public void Test_f64_sqrt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.sqrt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                if (double.IsNaN(ret.GetFloat64()) != double.IsNaN(System.Math.Sqrt(tv)))
                    throw new System.Exception("Invalid handling float errors");

                if (double.IsNaN(ret.GetFloat64()) == true)
                    continue;

                if (ret.GetFloat64() != System.Math.Sqrt(tv))
                    throw new System.Exception("Invalid return value.");
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletFloat(tva - tvb, ret, "f32.mul", idx, tva, tvb);
                    ++idx;
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletFloat(tva * tvb, ret, "f32.mul", idx, tva, tvb);
                    ++idx;
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletFloat(tva / tvb, ret, "f32.mul", idx, tva, tvb);
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
                RunBiNOpGaunletThroughTripplet(ex, mod, ft, "f32.min", i);
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
                RunBiNOpGaunletThroughTripplet(ex, mod, ft, "f32.min", i);
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


                for (int i = 0; i < testItems.Count; ++i)
                {
                    FloatTrippplet ft = testItems[i];
                    RunBiNOpGaunletThroughTripplet(ex, mod, ft, "f32.copysign", i);
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletDouble(tva + tvb, ret, "f64.add", idx, tva, tvb);
                    ++idx;
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletDouble(tva - tvb, ret, "f64.sub", idx, tva, tvb);
                    ++idx;
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletDouble(tva * tvb, ret, "f64.mul", idx, tva, tvb);
                    ++idx;
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
                    PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tva),
                        PxPre.Datum.Val.Make(tvb));

                    CompareGaunletDouble(tva / tvb, ret, "f64.div", idx, tva, tvb);
                    ++idx;
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

            for (int i = 0; i < testItems.Count; ++i)
            {
                DoubleTripplet ft = testItems[i];
                RunBiNOpGaunletThroughTripplet(ex, mod, ft, "f64.min", i);
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

            for (int i = 0; i < testItems.Count; ++i)
            {
                DoubleTripplet ft = testItems[i];
                RunBiNOpGaunletThroughTripplet(ex, mod, ft, "f64.max", i);
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


            for (int i = 0; i < testItems.Count; ++i)
            {
                DoubleTripplet dt = testItems[i];
                RunBiNOpGaunletThroughTripplet(ex, mod, dt, "f64.copysign", i);
            }
        }
    }
}