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

using NUnit.Framework;
namespace Tests
{
    public class Test_GauntletConversions
    {
        const string TestTheme = "Conversions";

        public static float[] floatTestValues = 
            new float[] { -9999.1234f, -45.5f, -45.0f, -0.75f, 0.0f, 0.75f, 45.0f, 45.5f, 9999.1234f };

        public static long[] longTestValues = 
            new long[] { -123456789123, -12345, 0, 1, 5, 12345, 123456789123 };

        ulong [] ulongTestValues =
            new ulong[] { 0, 10, 55, 999, 4294967296, 6442450944, 18446744073709551615 };

        double[] doubleTestValues = 
            new double[] { -1234567890123.1234, -45.5, -45, -0.75, 0.0, 0.75, 45, 45.5, 1234567890123.1234 };

        int [] intTestValues = 
            new int[] { -999, -45, 0, 45, 999 };

        uint[] uintTestValues =
            new uint[] { 0, 1, 10, 55, 999, 123456789, 4294967295 };


        [Test]
        public void Test_i32_wrap_i64  ()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.wrap_i64.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt((int)(tv % 4294967296), ret, "i32.wrap_i64", idx++, tv);
            }
        }

        [Test]
        public void Test_i32_trunc_f32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_f32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt( (int)tv, ret, "i32.trunc_f32_s", idx++, tv);
            }
        }

        [Test]
        public void Test_i32_trunc_f32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_f32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (float tv in floatTestValues)
            {
                bool shouldCatch = false;
                bool didCatch = false;

                PxPre.Datum.Val ret = null;
                try
                {
                    shouldCatch = (tv <= -1.0f);

                    ret =
                        ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                }
                catch(System.Exception /*except*/)
                {
                    didCatch = true;
                }
                if(shouldCatch != didCatch)
                    throw new System.Exception("Exception catching expectations not held.");

                if(shouldCatch == true)
                    continue;

                if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetUInt() != (uint)tv)
                    throw new System.Exception("Invalid return value.");

            }
        }

        [Test]
        public void Test_i32_trunc_f64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_f64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt((int)tv, ret, "i32.trunc_f64_s", idx++, tv);
            }
        }

        [Test]
        public void Test_i32_trunc_f64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.trunc_f64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (double tv in doubleTestValues)
            {
                bool shouldCatch = false;
                bool didCatch = false;

                PxPre.Datum.Val ret = null;
                try
                {
                    shouldCatch = (tv <= -1.0);

                    ret =
                        ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                }
                catch (System.Exception /*except*/)
                {
                    didCatch = true;
                }
                if (shouldCatch != didCatch)
                    throw new System.Exception("Exception catching expectations not held.");

                if (shouldCatch == true)
                    continue;

                if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetUInt() != (uint)tv)
                    throw new System.Exception("Invalid return value.");

            }
        }

        [Test]
        public void Test_i64_extend_i32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.extend_i32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt64((long)tv, ret, "i64.extend_i32_s.wasm", idx++, tv);
            }
        }

        [Test]
        public void Test_i64_extend_i32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.extend_i32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (uint tv in uintTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt64((long)tv, ret, "i64.extend_i32_u", idx++, tv);
            }
        }

        [Test]
        public void Test_i64_trunc_f32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_f32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt64((long)tv, ret, "i64.trunc_f32_s", idx++, tv);
            }
        }

        [Test]
        public void Test_i64_trunc_f32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_f32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (float tv in floatTestValues)
            {
                bool shouldCatch = false;
                bool didCatch = false;

                PxPre.Datum.Val ret = null;
                try
                {
                    shouldCatch = (tv <= -1.0f);

                    ret =
                        ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                }
                catch (System.Exception /*except*/)
                {
                    didCatch = true;
                }
                if (shouldCatch != didCatch)
                    throw new System.Exception("Exception catching expectations not held.");

                if (shouldCatch == true)
                    continue;

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetUInt64() != (ulong)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i64_trunc_f64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_f64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletInt64((long)tv, ret, "i64.trunc_f64_s", idx++, tv);
            }
        }

        [Test]
        public void Test_i64_trunc_f64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.trunc_f64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (double tv in doubleTestValues)
            {
                bool shouldCatch = false;
                bool didCatch = false;

                PxPre.Datum.Val ret = null;
                try
                {
                    shouldCatch = (tv <= -1.0);

                    ret =
                        ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                }
                catch (System.Exception /*except*/)
                {
                    didCatch = true;
                }
                if (shouldCatch != didCatch)
                    throw new System.Exception("Exception catching expectations not held.");

                if (shouldCatch == true)
                    continue;

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetUInt64() != (ulong)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f32_convert_i32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.convert_i32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", new PxPre.Datum.ValInt(tv));
                UnitUtil.CompareGaunletFloat(tv, ret, "f32.convert_i32_s", idx++, tv);
            }
        }

        [Test]
        public void Test_f32_convert_i32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.convert_i32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (uint tv in uintTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat(tv, ret, "f32.convert_i32_u", idx++, tv);
            }
        }

        [Test]
        public void Test_f32_convert_i64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.convert_i64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat( tv, ret, "f32.convert_i64_s", idx++, tv);
            }
        }

        [Test]
        public void Test_f32_convert_i64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.convert_i64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (ulong tv in ulongTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat( tv, ret, "f32.convert_i64_u", idx++, tv);
            }
        }

        [Test]
        public void Test_f32_demote_f64()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.demote_f64.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat((float)tv, ret, "f32.demote_f64", idx++, tv);
            }
        }

        [Test]
        public void Test_f64_convert_i32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.convert_i32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(tv, ret, "f64.convert_i32_s", idx++, tv);
            }
        }

        [Test]
        public void Test_f64_convert_i32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.convert_i32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (uint tv in uintTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", new PxPre.Datum.ValUInt(tv));
                UnitUtil.CompareGaunletFloat64((double)tv , ret, "f64.convert_i32_u", idx++, tv);
            }
        }

        [Test]
        public void Test_f64_convert_i64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.convert_i64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", new PxPre.Datum.ValInt64(tv));
                UnitUtil.CompareGaunletFloat64( tv, ret, "f64.convert_i64_s", idx++, tv);
            }
        }

        [Test]
        public void Test_f64_convert_i64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.convert_i64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (ulong tv in ulongTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", new PxPre.Datum.ValUInt64(tv));
                UnitUtil.CompareGaunletFloat64(tv, ret, "f64.convert_i64_u", idx++, tv);
            }
        }

        [Test]
        public void Test_f64_promote_f32()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.promote_f32.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (float tv in floatTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                UnitUtil.CompareGaunletFloat64(tv, ret, "f64.promote_f32", idx++, tv);
            }
        }

        [Test]
        public void Test_i32_reinterpret_f32()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.reinterpret_f32.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            float[] testValues = new float[] { -9999.1234f, -45.5f, -45f, 0f, 45f, 45.5f, 9999.1234f };

            int idx = 0;
            foreach (float tv in testValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));

                byte[] rb = System.BitConverter.GetBytes(tv);
                int i = System.BitConverter.ToInt32(rb, 0);

                UnitUtil.CompareGaunletInt(i, ret, "i32.reinterpret_f32", idx++, tv);
            }
        }

        [Test]
        public void Test_i64_reinterpret_f64()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.reinterpret_f64.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (double tv in doubleTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));

                byte[] rb = System.BitConverter.GetBytes(tv);
                long l = System.BitConverter.ToInt64(rb, 0);

                UnitUtil.CompareGaunletInt64(l, ret, "i64.reinterpret_f64", ++idx, tv);
            }
        }

        [Test]
        public void Test_f32_reinterpret_i32()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.reinterpret_i32.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));

                byte [] rb = System.BitConverter.GetBytes(tv);
                float f = System.BitConverter.ToSingle(rb, 0);

                UnitUtil.CompareGaunletFloat(f, ret, "f32.reinterpret_i32", idx++, tv);
            }
        }

        [Test]
        public void Test_f64_reinterpret_i64()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.reinterpret_i64.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach(long tv in longTestValues)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(tv));
                double dr = System.BitConverter.Int64BitsToDouble(tv);
                UnitUtil.CompareGaunletFloat64(dr, ret, "f64.reinterpret_i64", idx++, tv);
            }
        }

        [Test]
        public void Test_i32_extend8_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.extend8_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for(int i = -500; i < 500; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(i));

                byte [] rb = System.BitConverter.GetBytes(i);
                sbyte low = (sbyte)rb[0];

                UnitUtil.CompareGaunletInt( low, ret, "i32.extend8_s", idx++, i);
            }
        }

        [Test]
        public void Test_i32_extend16_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.extend16_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for (int i = -500; i < 500; ++i)
            {
                short v = (short)(i * 251); // Multiply by something near, but less than 256. Preferrably an odd number.
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(v));
                UnitUtil.CompareGaunletInt(v, ret, "i32.extend16_s", idx, v);
            }
        }

        [Test]
        public void Test_i64_extend8_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.extend8_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for (long i = -500; i < 500; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(i));

                byte[] rb = System.BitConverter.GetBytes(i);
                sbyte low = (sbyte)rb[0];

                UnitUtil.CompareGaunletInt64(low, ret, "i64.extend8_s", idx++, i);
            }
        }

        [Test]
        public void Test_i64_extend16_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.extend16_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for(long i = -500; i < 500; ++i)
            {
                long val = i * 251;

                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(val));

                byte[] rb = System.BitConverter.GetBytes(val);
                short low = System.BitConverter.ToInt16(rb, 0);

                UnitUtil.CompareGaunletInt64(low, ret, "i64.extend16_s", idx++, val);
            }
        }

        [Test]
        public void Test_i64_extend32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.extend32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            for (long i = -500; i < 500; ++i)
            {
                long val = i * 65432;

                PxPre.Datum.Val ret = ex.Invoke_SingleRet( mod, "Test", PxPre.Datum.Val.Make(val));

                byte[] rb = System.BitConverter.GetBytes(val);
                int low = System.BitConverter.ToInt32(rb, 0);

                UnitUtil.CompareGaunletInt64(low, ret, "i64.extend32_s", idx++, val);
            }
        }
    }
}