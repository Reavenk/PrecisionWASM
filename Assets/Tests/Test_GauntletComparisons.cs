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
    public class Test_GauntletComparisons
    {
        const string TestTheme = "Compares";

        static List<IntPair> testKeysIntInt =
            new List<IntPair>
            {
                new IntPair(0,      0      ),
                new IntPair(50,     50     ),
                new IntPair(-99,    -99    ),
                new IntPair(-5675,  55567  ),
                new IntPair(20,     30     ),
                new IntPair(-20,    -30    ),
                new IntPair(12345,  54321  ),
                new IntPair(0,      33     ),
                new IntPair(33,     0      ),
                new IntPair(400400, 400400 ),
            };

        static List<Int64Pair> testKeysLongLong =
            new List<Int64Pair>
            {
                new Int64Pair(0,                    0      ),
                new Int64Pair(50,                   50     ),
                new Int64Pair(-99,                  -99    ),
                new Int64Pair(-5675,                55567  ),
                new Int64Pair(20,                   30     ),
                new Int64Pair(-20,                  -30    ),
                new Int64Pair(12345,                54321  ),
                new Int64Pair(0,                    33     ),
                new Int64Pair(33,                   0      ),
                new Int64Pair(400400,               400400 ),
                //
                new Int64Pair(123456780,            123456780      ),
                new Int64Pair(1234567850,           1234567850     ),
                new Int64Pair(-1234567899,          -1234567899    ),
                new Int64Pair(-123456785675,        1234567855567  ),
                new Int64Pair(1234567820,           1234567830     ),
                new Int64Pair(-1234567820,          -1234567830    ),
                new Int64Pair(1234567812345,        1234567854321  ),
                new Int64Pair(123456780,            1234567833     ),
                new Int64Pair(1234567833,           123456780      ),
                new Int64Pair(12345678400400,       12345678400400 ),
            };

        static List<FloatPair> testKeysFloatFloat =
            new List<FloatPair>
            {
                new FloatPair(float.NaN,                    float.NaN),
                new FloatPair(float.PositiveInfinity,       float.PositiveInfinity),
                new FloatPair(float.NegativeInfinity,       float.NegativeInfinity),
                new FloatPair(float.PositiveInfinity,       float.NegativeInfinity),
                new FloatPair(float.NaN,                    float.PositiveInfinity),
                new FloatPair(float.NaN,                    float.NegativeInfinity),
                new FloatPair(-0.0f,                        0.0f),
                new FloatPair(-float.NaN,                   float.NaN),
                new FloatPair(float.PositiveInfinity,       -float.NegativeInfinity),
                new FloatPair(float.NegativeInfinity,       -float.PositiveInfinity),
                new FloatPair(0.0f,                         0.0f),
                new FloatPair(-10.0f,                       10),
                new FloatPair(10.0f,                        -10.0f),
                new FloatPair(12345.678f,                   12345.678f),
                new FloatPair(901.1234f,                    567.89f),
                new FloatPair(9876654321.21f,               9876654321.0f),
                new FloatPair(9876654321.21f,               9876654321.21f)
            };

        static List<Float64Pair> testKeysDoubleDouble =
            new List<Float64Pair>
            {
                new Float64Pair(double.NaN,                    double.NaN),
                new Float64Pair(double.PositiveInfinity,       double.PositiveInfinity),
                new Float64Pair(double.NegativeInfinity,       double.NegativeInfinity),
                new Float64Pair(double.PositiveInfinity,       double.NegativeInfinity),
                new Float64Pair(double.NaN,                    double.PositiveInfinity),
                new Float64Pair(double.NaN,                    double.NegativeInfinity),
                new Float64Pair(-0.0,                           0.0),
                new Float64Pair(-double.NaN,                   double.NaN),
                new Float64Pair(double.PositiveInfinity,       -double.NegativeInfinity),
                new Float64Pair(double.NegativeInfinity,       -double.PositiveInfinity),
                new Float64Pair(0.0,                            0.0),
                new Float64Pair(-10.0,                          10),
                new Float64Pair(10.0,                           -10.0),
                new Float64Pair(12345.678,                      12345.678),
                new Float64Pair(901.1234,                       567.89),
                new Float64Pair(9876654321.21,                  9876654321.0),
                new Float64Pair(9876654321.21,                  9876654321.21)
            };

        [Test]
        public void Test_i32_eqz()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.eqz.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for(int i = -5; i < 5; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletBool(i == 0, ret, "i32.eqz", idx++, i);
            }
        }

        [Test]
        public void Test_i64_eqz()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.eqz.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for (long i = -5; i < 5; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletBool(i == 0, ret, "i64.eqz", idx++, i);
            }
        }

        [Test]
        public void Test_i32_eq()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.eq.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a == t.b, ret, "i32.eq", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_ne()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.ne.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a != t.b, ret, "i32.ne", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_lt_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.lt_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a < t.b, ret, "i32.lt_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_lt_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.lt_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((uint)t.a < (uint)t.b, ret, "i32.lt_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_gt_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.gt_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a > t.b, ret, "i32.gt_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_gt_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.gt_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((uint)t.a > (uint)t.b, ret, "i32.gt_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_le_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.le_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a <= t.b, ret, "i32.le_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_le_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.le_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((uint)t.a <= (uint)t.b, ret, "i32.le_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_ge_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.ge_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a >= t.b, ret, "i32.ge_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i32_ge_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.ge_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (IntPair t in testKeysIntInt)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((uint)t.a >= (uint)t.b, ret, "i32.ge_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_eq()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.eq.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a == t.b, ret, "i64.eq", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_ne()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.ne.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a != t.b, ret, "i64.ne", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_lt_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.lt_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a < t.b, ret, "i64.lt_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_lt_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.lt_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((ulong)t.a < (ulong)t.b, ret, "i64.lt_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_gt_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.gt_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a > t.b, ret, "i64.gt_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_gt_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.gt_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((ulong)t.a > (ulong)t.b, ret, "i64.gt_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_le_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.le_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a <= t.b, ret, "i64.le_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_le_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.le_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((ulong)t.a <= (ulong)t.b, ret, "i64.le_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_ge_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.ge_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a >= t.b, ret, "i64.ge_s", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_i64_ge_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.ge_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Int64Pair t in testKeysLongLong)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool((ulong)t.a >= (ulong)t.b, ret, "i64.ge_u", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f32_eq()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.eq.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (FloatPair t in testKeysFloatFloat)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a == t.b, ret, "f32.eq", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f32_ne()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.ne.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (FloatPair t in testKeysFloatFloat)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a != t.b, ret, "f32.ne", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f32_lt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.lt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (FloatPair t in testKeysFloatFloat)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a < t.b, ret, "f32.lt", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f32_gt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.gt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (FloatPair t in testKeysFloatFloat)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a > t.b, ret, "f32.gt", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f32_le()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.le.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (FloatPair t in testKeysFloatFloat)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a <= t.b, ret, "f32.le", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f32_ge()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.ge.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (FloatPair t in testKeysFloatFloat)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a >= t.b, ret, "f32.ge", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f64_eq()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.eq.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Float64Pair t in testKeysDoubleDouble)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a == t.b, ret, "f64.eq", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f64_ne()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.ne.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Float64Pair t in testKeysDoubleDouble)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a != t.b, ret, "f64.ne", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f64_lt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.lt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Float64Pair t in testKeysDoubleDouble)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a < t.b, ret, "f64.lt", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f64_gt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.gt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Float64Pair t in testKeysDoubleDouble)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a > t.b, ret, "f64.gt", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f64_le()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.le.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Float64Pair t in testKeysDoubleDouble)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a <= t.b, ret, "f64.le", idx++, t.a, t.b);
            }
        }

        [Test]
        public void Test_f64_ge()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.ge.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (Float64Pair t in testKeysDoubleDouble)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(t.a), PxPre.Datum.Val.Make(t.b));
                UnitUtil.CompareGaunletBool(t.a >= t.b, ret, "f64.ge", idx++, t.a, t.b);
            }
        }
    }
}