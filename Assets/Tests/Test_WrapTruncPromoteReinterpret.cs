using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;


namespace Tests
{
    public class Test_WrapTruncPromoteReinterpret
    {
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
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i32.wrap_i64.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            // TODO: Check with explicit truth values
            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt() != (int)(tv % 4294967296))
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i32_trunc_f32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i32.trunc_f32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt() != (int)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i32_trunc_f32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i32.trunc_f32_u.wasm");
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
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i32.trunc_f64_s.wasm");
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

                if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt() != (int)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i32_trunc_f64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i32.trunc_f64_u.wasm");
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
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.extend_i32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt64() != (int)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i64_extend_i32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.extend_i32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (uint tv in uintTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetUInt64() != (ulong)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i64_trunc_f32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.trunc_f32_s.wasm");
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

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt64() != (long)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i64_trunc_f32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.trunc_f32_u.wasm");
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
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.trunc_f64_s.wasm");
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

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt64() != (long)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i64_trunc_f64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.trunc_f64_u.wasm");
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
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f32.convert_i32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        new PxPre.Datum.ValInt(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat() != tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f32_convert_i32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f32.convert_i32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (uint tv in uintTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat() != (float)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f32_convert_i64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f32.convert_i64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat() != (float)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f32_convert_i64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f32.convert_i64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (ulong tv in ulongTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat() != (float)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f32_demote_f64()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f32.demote_f64.wasm");
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

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat() != (float)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f64_convert_i32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f64.convert_i32_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetInt64() != tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f64_convert_i32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f64.convert_i32_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (uint tv in uintTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        new PxPre.Datum.ValUInt64(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat64() != (double)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f64_convert_i64_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f64.convert_i64_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (long tv in longTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        new PxPre.Datum.ValInt64(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat64() != tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f64_convert_i64_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f64.convert_i64_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (ulong tv in ulongTestValues)
            {
                PxPre.Datum.Val ret =
                    ex.Invoke_SingleRet(
                        mod,
                        "Test",
                        new PxPre.Datum.ValUInt64(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat64() != (double)tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f64_promote_f32()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f64.promote_f32.wasm");
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

                if (ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                if (ret.GetFloat64() != tv)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_i32_reinterpret_f32()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i32.reinterpret_f32.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            float[] testValues = new float[] { -9999.1234f, -45.5f, -45f, 0f, 45f, 45.5f, 9999.1234f };

            foreach (float tv in testValues)
            {
                PxPre.Datum.Val ret =
                ex.Invoke_SingleRet(
                    mod,
                    "Test",
                    PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("Invalid return type.");

                byte[] rb = System.BitConverter.GetBytes(tv);
                int i = System.BitConverter.ToInt32(rb, 0);

                if (ret.GetInt() != i)
                    throw new System.Exception("i32.reinterpret_f32 invalid return value.");
            }
        }

        [Test]
        public void Test_i64_reinterpret_f64()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/i64.reinterpret_f64.wasm");
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

                if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                    throw new System.Exception("Invalid return type.");

                byte[] rb = System.BitConverter.GetBytes(tv);
                long l = System.BitConverter.ToInt64(rb, 0);

                if (ret.GetInt64() != l)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f32_reinterpret_i32()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f32.reinterpret_i32.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach (int tv in intTestValues)
            {
                PxPre.Datum.Val ret =
                ex.Invoke_SingleRet(
                    mod,
                    "Test",
                    PxPre.Datum.Val.Make(tv));

                if (ret.wrapType != PxPre.Datum.Val.Type.Float)
                    throw new System.Exception("Invalid return type.");

                byte [] rb = System.BitConverter.GetBytes(tv);
                float f = System.BitConverter.ToSingle(rb, 0);

                if (float.IsNaN(ret.GetFloat()) == true && float.IsNaN(f) == true)
                    continue;

                if (ret.GetFloat() != f)
                    throw new System.Exception("Invalid return value.");
            }
        }

        [Test]
        public void Test_f64_reinterpret_i64()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/Gauntlet/f64.reinterpret_i64.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            foreach(long tv in longTestValues)
            {
                PxPre.Datum.Val ret =
                ex.Invoke_SingleRet(
                    mod,
                    "Test",
                    PxPre.Datum.Val.Make(tv));

                if(ret.wrapType != PxPre.Datum.Val.Type.Float64)
                    throw new System.Exception("Invalid return type.");

                double dr = System.BitConverter.Int64BitsToDouble(tv);

                if(double.IsNaN(ret.GetFloat()) == true && double.IsNaN(dr) == true)
                    continue;

                if (ret.GetFloat64() != dr)
                    throw new System.Exception("Invalid return value.");  
            }
        }
    }
}