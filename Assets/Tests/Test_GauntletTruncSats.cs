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
    public class Test_GauntletTruncSats
    {
        const string TestTheme = "TruncSats";

        [Test]
        public void Test_i32_trunc_sat_f32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_sat_f32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<float, int>> testKeys = 
                new List<MixedPair<float, int>>
                {
                    new MixedPair<float, int>(0,                                    0),
                    new MixedPair<float, int>(float.NaN,                            0),
                    new MixedPair<float, int>(float.PositiveInfinity,               2147483647),
                    new MixedPair<float, int>(float.NegativeInfinity,               -2147483648),
                    new MixedPair<float, int>(12345.678f,                           12345),
                    new MixedPair<float, int>(-12345.678f,                          -12345),
                    new MixedPair<float, int>(123451234512345.678f,                 2147483647),
                    new MixedPair<float, int>(-123451234512345.678f,                -2147483648),
                    new MixedPair<float, int>(999999999999999123451234512345.678f,  2147483647),
                    new MixedPair<float, int>(-999999999999999123451234512345.678f, -2147483648)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletInt(t.b, ret, "i32.trunc_sat_f32_s", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i32_trunc_sat_f32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_sat_f32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<float, uint>> testKeys =
                new List<MixedPair<float, uint>>
                {
                    new MixedPair<float, uint>(0,                                    0),
                    new MixedPair<float, uint>(float.NaN,                            0),
                    new MixedPair<float, uint>(float.PositiveInfinity,               4294967295),
                    new MixedPair<float, uint>(float.NegativeInfinity,               0),
                    new MixedPair<float, uint>(12345.678f,                           12345),
                    new MixedPair<float, uint>(-12345.678f,                          0),
                    new MixedPair<float, uint>(123451234512345.678f,                 4294967295),
                    new MixedPair<float, uint>(-123451234512345.678f,                0),
                    new MixedPair<float, uint>(999999999999999123451234512345.678f,  4294967295),
                    new MixedPair<float, uint>(-999999999999999123451234512345.678f, 0)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletInt((int)t.b, ret, "i32.trunc_sat_f32_s", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i32_trunc_sat_f64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_sat_f64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<double, int>> testKeys =
                new List<MixedPair<double, int>>
                {
                    new MixedPair<double, int>(0,                                   0),
                    new MixedPair<double, int>(double.NaN,                          0),
                    new MixedPair<double, int>(double.PositiveInfinity,             2147483647),
                    new MixedPair<double, int>(double.NegativeInfinity,             -2147483648),
                    new MixedPair<double, int>(12345.678,                           12345),
                    new MixedPair<double, int>(-12345.678,                          -12345),
                    new MixedPair<double, int>(123451234512345.678,                 2147483647),
                    new MixedPair<double, int>(-123451234512345.678,                -2147483648),
                    new MixedPair<double, int>(999999999999999123451234512345.678,  2147483647),
                    new MixedPair<double, int>(-999999999999999123451234512345.678, -2147483648)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletInt(t.b, ret, "i32.trunc_sat_f64_s", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i32_trunc_sat_f64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_sat_f64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<double, uint>> testKeys =
                new List<MixedPair<double, uint>>
                {
                    new MixedPair<double, uint>(0,                                   0),
                    new MixedPair<double, uint>(double.NaN,                          0),
                    new MixedPair<double, uint>(double.PositiveInfinity,             4294967295),
                    new MixedPair<double, uint>(double.NegativeInfinity,             0),
                    new MixedPair<double, uint>(12345.678,                           12345),
                    new MixedPair<double, uint>(-12345.678,                          0),
                    new MixedPair<double, uint>(123451234512345.678,                 4294967295),
                    new MixedPair<double, uint>(-123451234512345.678,                0),
                    new MixedPair<double, uint>(999999999999999123451234512345.678,  4294967295),
                    new MixedPair<double, uint>(-999999999999999123451234512345.678, 0)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletInt((int)t.b, ret, "i32.trunc_sat_f64_u", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i64_trunc_sat_f32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_sat_f32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<float, long>> testKeys =
                new List<MixedPair<float, long>>
                {
                    new MixedPair<float, long>(0,                                       0),
                    new MixedPair<float, long>(float.NaN,                               0),
                    new MixedPair<float, long>(float.PositiveInfinity,                  9223372036854775807),
                    new MixedPair<float, long>(float.NegativeInfinity,                  -9223372036854775808),
                    new MixedPair<float, long>(12345.678f,                              12345),
                    new MixedPair<float, long>(-12345.678f,                             -12345),
                    new MixedPair<float, long>(123451234512345.678f,                    123451234844672),
                    new MixedPair<float, long>(-123451234512345.678f,                   -123451234844672),
                    new MixedPair<float, long>(999999999999999123451234512345.678f,     9223372036854775807),
                    new MixedPair<float, long>(-999999999999999123451234512345.678f,    -9223372036854775808)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletLong(t.b, ret, "i64.trunc_sat_f32_s", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i64_trunc_sat_f32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_sat_f32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<float, ulong>> testKeys =
                new List<MixedPair<float, ulong>>
                {
                    new MixedPair<float, ulong>(0,                                       0),
                    new MixedPair<float, ulong>(float.NaN,                               0),
                    new MixedPair<float, ulong>(float.PositiveInfinity,                  18446744073709551615),
                    new MixedPair<float, ulong>(float.NegativeInfinity,                  0),
                    new MixedPair<float, ulong>(12345.678f,                              12345),
                    new MixedPair<float, ulong>(-12345.678f,                             0),
                    new MixedPair<float, ulong>(123451234512345.678f,                    123451234844672),
                    new MixedPair<float, ulong>(-123451234512345.678f,                   0),
                    new MixedPair<float, ulong>(999999999999999123451234512345.678f,     18446744073709551615),
                    new MixedPair<float, ulong>(-999999999999999123451234512345.678f,    0)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletLong((long)t.b, ret, "i64.trunc_sat_f32_s", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i64_trunc_sat_f64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_sat_f64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<double, long>> testKeys =
                new List<MixedPair<double, long>>
                {
                    new MixedPair<double, long>(0,                                      0),
                    new MixedPair<double, long>(double.NaN,                             0),
                    new MixedPair<double, long>(double.PositiveInfinity,                9223372036854775807),
                    new MixedPair<double, long>(double.NegativeInfinity,                -9223372036854775808),
                    new MixedPair<double, long>(12345.678,                              12345),
                    new MixedPair<double, long>(-12345.678,                             -12345),
                    new MixedPair<double, long>(123451234512345.678,                    123451234512345),
                    new MixedPair<double, long>(-123451234512345.678,                   -123451234512345),
                    new MixedPair<double, long>(999999999999999123451234512345.678,     9223372036854775807),
                    new MixedPair<double, long>(-999999999999999123451234512345.678,    -9223372036854775808)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletLong(t.b, ret, "i64.trunc_sat_f64_s", idx++, 0 /*operand listing supported*/);
            }
        }

        [Test]
        public void Test_i64_trunc_sat_f64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_sat_f64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<MixedPair<double, ulong>> testKeys =
                new List<MixedPair<double, ulong>>
                {
                    new MixedPair<double, ulong>(0,                                      0),
                    new MixedPair<double, ulong>(double.NaN,                             0),
                    new MixedPair<double, ulong>(double.PositiveInfinity,                18446744073709551615),
                    new MixedPair<double, ulong>(double.NegativeInfinity,                0),
                    new MixedPair<double, ulong>(12345.678,                              12345),
                    new MixedPair<double, ulong>(-12345.678,                             0),
                    new MixedPair<double, ulong>(123451234512345.678,                    123451234512345),
                    new MixedPair<double, ulong>(-123451234512345.678,                   0),
                    new MixedPair<double, ulong>(999999999999999123451234512345.678,     18446744073709551615),
                    new MixedPair<double, ulong>(-999999999999999123451234512345.678,    0)
                };

            int idx = 0;
            foreach (var t in testKeys)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a));
                UnitUtil.CompareGaunletLong((long)t.b, ret, "i64.trunc_sat_f64_u", idx++, 0 /*operand listing supported*/);
            }
        }
    }
}
