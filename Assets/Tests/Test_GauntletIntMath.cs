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
    public class Test_GauntletIntMath
    {
        const string TestTheme = "IntMath";

        public static List<long> GenerateTestSamples()
        { 
            List<long> ret = new List<long>();
            ret.Add(0);
            ret.Add(-1);
            ret.Add(1);
            ret.Add(byte.MinValue);
            ret.Add(byte.MaxValue);
            ret.Add(sbyte.MinValue);
            ret.Add(sbyte.MaxValue);

            ret.Add(short.MinValue);
            ret.Add(short.MaxValue);
            ret.Add(ushort.MinValue);
            ret.Add(ushort.MaxValue);

            ret.Add(int.MinValue);
            ret.Add(int.MaxValue);
            ret.Add(uint.MinValue);
            ret.Add(uint.MaxValue);

            ret.Add(long.MinValue);
            ret.Add(long.MaxValue);
            ret.Add((long)ulong.MinValue);
            unchecked { ret.Add((long)(ulong.MaxValue)); }

            System.Random rand = new System.Random(0);
            byte [] rb = new byte[8];

            for(int i = 0; i < 10; ++i)
            { 
                rand.NextBytes(rb);
                ret.Add(System.BitConverter.ToInt64(rb, 0));

                rand.NextBytes(rb);
                ret.Add(System.BitConverter.ToInt32(rb, 0));

                rand.NextBytes(rb);
                ret.Add(System.BitConverter.ToInt16(rb, 0));
            }

            return ret;
        }

        // All tests will be with this dataset. Smaller and unsigned sets will simply
        // do sign conversions and truncate. Besides the explicit values added, what
        // actually matters is diversity in the values. It's not like the float gauntlet
        // that's tedious because of the error codes and edge cases.
        static List<long> testSamples = GenerateTestSamples();

        

        [Test]
        public void Test_i32_clz()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.clz.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for(int i = 0; i < testSamples.Count; ++i)
            { 
                int nv = (int)testSamples[i];

                int lz = 0;
                for(; lz < 32; ++lz)
                { 
                    if( (nv&(1<<(31-lz))) != 0)
                        break;
                }

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv));
                UnitUtil.CompareGaunletInt(lz, ret, "i32.clz", i, nv);
            }
        }
            
        [Test]
        public void Test_i32_ctz()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.ctz.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < testSamples.Count; ++i)
            {
                int nv = (int)testSamples[i];

                int rz = 0;
                for (; rz < 32; ++rz)
                {
                    if ((nv & (1<<rz)) != 0)
                        break;
                }

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv));
                UnitUtil.CompareGaunletInt(rz, ret, "i32.ctz", i, nv);
            }
        }
            
        [Test]
        public void Test_i32_popcnt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.popcnt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < testSamples.Count; ++i)
            {
                int nv = (int)testSamples[i];

                int zct = 0;
                for (int j = 0; j < 32; ++j)
                {
                    if ((nv & (1 << j)) != 0)
                        ++zct;
                }

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv));
                UnitUtil.CompareGaunletInt(zct, ret, "i32.popcnt", i, nv);
            }
        }
            
        [Test]
        public void Test_i64_clz()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.clz.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < testSamples.Count; ++i)
            {
                long nv = testSamples[i];

                long lz = 0;
                for (; lz < 64; ++lz)
                {
                    if ((nv & (1 << (63 - (int)lz))) != 0)
                        break;
                }

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv));
                UnitUtil.CompareGaunletLong(lz, ret, "i64.clz", i, nv);
            }
        }
            
        [Test]
        public void Test_i64_ctz()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.ctz.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < testSamples.Count; ++i)
            {
                long nv = testSamples[i];

                long rz = 0;
                for (; rz < 64; ++rz)
                {
                    if ((nv & (1 << (int)rz)) != 0)
                        break;
                }

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv));
                UnitUtil.CompareGaunletLong(rz, ret, "i64.ctz", i, nv);
            }
        }
            
        [Test]
        public void Test_i64_popcnt()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.popcnt.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            for (int i = 0; i < testSamples.Count; ++i)
            {
                long nv = testSamples[i];

                long zct = 0;
                for (int j = 0; j < 64; ++j)
                {
                    if ((nv & (1 << j)) != 0)
                        ++zct;
                }

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv));
                UnitUtil.CompareGaunletLong(zct, ret, "i64.popcnt", i, nv);
            }
        }
            
        [Test]
        public void Test_i32_add()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.add.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach(long a in testSamples)
            { 
                foreach(long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    UnitUtil.CompareGaunletInt(na + nb, ret, "i32.add", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_sub()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.sub.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    UnitUtil.CompareGaunletInt(na - nb, ret, "i32.sub", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_mul()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.mul.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    UnitUtil.CompareGaunletInt(na * nb, ret, "i32.mul", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_div_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.div_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret;
                    if (nb == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                        }
                        catch(System.Exception)
                        { 
                            threw = true;
                        }

                        if(threw == false)
                            throw new System.Exception("Missing expected exception for integer divide by zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletInt(na / nb, ret, "i32.div_s", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_div_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.div_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            // While this is for unsigned values, casting it to ints should be find as long as 
            // both the results and the truth are casted at the end.
            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    uint na = (uint)a;
                    uint nb = (uint)b;

                    PxPre.Datum.Val ret;
                    if (nb == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        }

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for integer divide by zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }
                    UnitUtil.CompareGaunletInt((int)(na / nb), ret, "i32.div_u", idx, (int)na, (int)nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_rem_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.rem_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = null;
                    if (nb == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        } 

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for remainder to zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletInt(na % nb, ret, "i32.rem_s", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_rem_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.rem_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    uint na = (uint)a;
                    uint nb = (uint)b;

                    PxPre.Datum.Val ret = null;
                    if (nb == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        }

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for remainder to zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletInt((int)(na % nb), ret, "i32.rem_u", idx, (int)na, (int)nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_and()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.and.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    UnitUtil.CompareGaunletInt(na & nb, ret, "i32.and", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_or()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.or.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    UnitUtil.CompareGaunletInt(na | nb, ret, "i32.or", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_xor()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.xor.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    int na = (int)a;
                    int nb = (int)b;

                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(na), PxPre.Datum.Val.Make(nb));
                    UnitUtil.CompareGaunletInt(na ^ nb, ret, "i32.or", idx, na, nb);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i32_shl()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.shl.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int shiftL = -20;
            int idx = 0;
            for (; idx  < testSamples.Count; ++idx)
            {
                int nv = (int)testSamples[idx];

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv), PxPre.Datum.Val.Make(shiftL));
                UnitUtil.CompareGaunletInt(nv << shiftL, ret, "i32.shl", idx, nv, shiftL);

                ++shiftL;
                if(shiftL > 40)
                    shiftL = -5; // Don't need to go back so far in the negatives the second time around.
            }

            IntTripplet[] extraTests = 
                new IntTripplet[]
                { 
                    new IntTripplet(-1,        13,     -8192),
                    new IntTripplet(-8192,     8192,   -8192),
                    new IntTripplet(8192,      -8192,  8192),
                    new IntTripplet(3,        -79,     393216),
                    new IntTripplet(3,        79,      98304)
                };

            foreach(IntTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletInt(it.c, ret, "i32.shl", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i32_shr_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.shr_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int shiftR = -20;
            int idx = 0;
            for (; idx < testSamples.Count; ++idx, ++shiftR)
            {
                if (shiftR > 40)
                    shiftR = -5; // Don't need to go back so far in the negatives the second time around.

                int nv = (int)testSamples[idx];

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv), PxPre.Datum.Val.Make(shiftR));
                UnitUtil.CompareGaunletInt(nv >> shiftR, ret, "i32.shr_s", idx, nv, shiftR);
            }

            IntTripplet[] extraTests =
                new IntTripplet[]
                {
                    new IntTripplet(12345678,      6,      192901),
                    new IntTripplet(12345678,      38,     192901),
                    new IntTripplet(12345678,      -6,     0),
                    new IntTripplet(12345678,      -16,    188),
                    new IntTripplet(12345678,      -48,    188),
                    // The 8 on the very left means the high bit is on
                    new IntTripplet(unchecked((int)0x81234567),    -5,    -16),
                    new IntTripplet(unchecked((int)0x81234567),    -37,   -16),
                    new IntTripplet(unchecked((int)0x81234567),    5,      -66512341),
                    new IntTripplet(unchecked((int)0x81234567),    37,     -66512341),
                };

            foreach (IntTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletInt(it.c, ret, "i32.shr_s", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i32_shr_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.shr_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int shiftR = -20;
            int idx = 0;
            for (; idx < testSamples.Count; ++idx)
            {
                uint nv = (uint)testSamples[idx];

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv), PxPre.Datum.Val.Make(shiftR));
                UnitUtil.CompareGaunletInt((int)(nv >> shiftR), ret, "i32.shr_u", idx, (int)nv, shiftR);

                ++shiftR;
                if (shiftR > 40)
                    shiftR = -5; // Don't need to go back so far in the negatives the second time around.
            }

            IntTripplet[] extraTests =
                new IntTripplet[]
                {
                    new IntTripplet(12345678,      6,      192901),
                    new IntTripplet(12345678,      38,     192901),
                    new IntTripplet(12345678,      -6,     0),
                    new IntTripplet(12345678,      -16,    188),
                    new IntTripplet(12345678,      -48,    188),
                    // The 8 on the very left means the high bit is on
                    new IntTripplet(unchecked((int)0x81234567),    -5,    16),
                    new IntTripplet(unchecked((int)0x81234567),    -37,   16),
                    new IntTripplet(unchecked((int)0x81234567),    5,      67705387),
                    new IntTripplet(unchecked((int)0x81234567),    37,     67705387),
                };

            foreach (IntTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletInt(it.c, ret, "i32.shr_u", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i32_rotl()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.rotl.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            IntTripplet[] extraTests =
            new IntTripplet[]
            {
                new IntTripplet(12345678,      6,      790123392),
                new IntTripplet(12345678,      38,     790123392),
                new IntTripplet(12345678,      -6,     939716997),
                new IntTripplet(12345678,      -16,    1632501948),
                new IntTripplet(12345678,      -48,    1632501948),
                // The 8 on the very left means the high bit is on
                new IntTripplet(unchecked((int)0x81234567),    -5,     1007229483),
                new IntTripplet(unchecked((int)0x81234567),    -37,    1007229483),
                new IntTripplet(unchecked((int)0x81234567),    5,      610839792),
                new IntTripplet(unchecked((int)0x81234567),    37,     610839792),
            };

            int idx = 0;
            foreach (IntTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletInt(it.c, ret, "i32.rotl", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i32_rotr()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.rotr.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            IntTripplet[] extraTests =
            new IntTripplet[]
            {
                new IntTripplet(12345678,      6,      939716997),
                new IntTripplet(12345678,      38,     939716997),
                new IntTripplet(12345678,      -6,     790123392),
                new IntTripplet(12345678,      -16,    1632501948),
                new IntTripplet(12345678,      -48,    1632501948),
                // The 8 on the very left means the high bit is on
                new IntTripplet(unchecked((int)0x81234567),    -5,     610839792),
                new IntTripplet(unchecked((int)0x81234567),    -37,    610839792),
                new IntTripplet(unchecked((int)0x81234567),    5,      1007229483),
                new IntTripplet(unchecked((int)0x81234567),    37,     1007229483),
            };

            int idx = 0;
            foreach (IntTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletInt(it.c, ret, "i32.rotr", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i64_add()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.add.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    UnitUtil.CompareGaunletLong(a + b, ret, "i64.add", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_sub()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.sub.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    UnitUtil.CompareGaunletLong(a - b, ret, "i64.sub", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_mul()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.mul.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    UnitUtil.CompareGaunletLong(a * b, ret, "i64.mul", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_div_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.div_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret;
                    if (b == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        }

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for integer divide by zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletLong(a / b, ret, "i64.div_s", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_div_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.div_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    ulong ua = (ulong)a;
                    ulong ub = (ulong)b;

                    PxPre.Datum.Val ret;
                    if (ub == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ua), PxPre.Datum.Val.Make(ub));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        }

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for integer divide by zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ua), PxPre.Datum.Val.Make(ub));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletLong((long)(ua / ub), ret, "i64.div_u", idx, (long)ua, (long)ub);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_rem_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.rem_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = null;
                    if (b == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        }

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for remainder to zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletLong(a % b, ret, "i64.rem_s", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_rem_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.rem_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    ulong ua = (ulong)a;
                    ulong ub = (ulong)b;

                    PxPre.Datum.Val ret = null;
                    if (ub == 0)
                    {
                        bool threw = false;
                        try
                        {
                            ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ua), PxPre.Datum.Val.Make(ub));
                        }
                        catch (System.Exception)
                        {
                            threw = true;
                        }

                        if (threw == false)
                            throw new System.Exception("Missing expected exception for remainder to zero.");

                        ++idx;
                        continue;
                    }

                    try
                    {
                        ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(ua), PxPre.Datum.Val.Make(ub));
                    }
                    catch (System.Exception)
                    {
                        // Overflow is still throwing exceptions, so they need to be caught and they
                        // will be ignored for now.
                        ++idx;
                        continue;
                    }

                    UnitUtil.CompareGaunletLong((long)(ua % ub), ret, "i32.rem_u", idx, (long)ua, (long)ub);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_and()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.and.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    UnitUtil.CompareGaunletLong(a & b, ret, "i64.and", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_or()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.or.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    UnitUtil.CompareGaunletLong(a | b, ret, "i64.or", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_xor()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.xor.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            int idx = 0;
            foreach (long a in testSamples)
            {
                foreach (long b in testSamples)
                {
                    PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(a), PxPre.Datum.Val.Make(b));
                    UnitUtil.CompareGaunletLong(a ^ b, ret, "i64.or", idx, a, b);
                    ++idx;
                }
            }
        }
            
        [Test]
        public void Test_i64_shl()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.shl.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            long shiftL = -20;
            int idx = 0;
            for (; idx < testSamples.Count; ++idx)
            {
                long nv = testSamples[idx];

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv), PxPre.Datum.Val.Make(shiftL));
                UnitUtil.CompareGaunletLong(nv << (int)shiftL, ret, "i64.shl", idx, nv, shiftL);

                ++shiftL;
                if (shiftL > 40)
                    shiftL = -5; // Don't need to go back so far in the negatives the second time around.
            }

            LongTripplet[] extraTests =
                new LongTripplet[]
                {
                    new LongTripplet(-1,        13,     -8192),
                    new LongTripplet(-8192,     8192,   -8192),
                    new LongTripplet(8192,      -8192,  8192),
                    new LongTripplet(3,        -79,     1688849860263936),
                    new LongTripplet(3,        79,      98304)
                };

            foreach (LongTripplet lt in extraTests) 
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(lt.a), PxPre.Datum.Val.Make(lt.b));
                UnitUtil.CompareGaunletLong(lt.c, ret, "i64.shl", idx, lt.a, lt.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i64_shr_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.shr_s.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            long shiftR = -20;
            int idx = 0;
            for (; idx < testSamples.Count; ++idx, ++shiftR)
            {
                if (shiftR > 40)
                    shiftR = -5; // Don't need to go back so far in the negatives the second time around.

                long nv = testSamples[idx];

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv), PxPre.Datum.Val.Make(shiftR));
                UnitUtil.CompareGaunletLong(nv >> (int)shiftR, ret, "i64.shr_s", idx, nv, shiftR);
            }

            LongTripplet[] extraTests =
                new LongTripplet[]
                {
                    new LongTripplet(12345678,      6,      192901),
                    new LongTripplet(12345678,      38,     0),
                    new LongTripplet(12345678,      -6,     0),
                    new LongTripplet(12345678,      -16,    0),
                    new LongTripplet(12345678,      -48,    188),
                    // The 8 on the very left means the high bit is on
                    new LongTripplet(unchecked((long)0x8123456789012345),    -50,    -557945953836028),
                    new LongTripplet(unchecked((long)0x8123456789012345),    -370,   -557945953836028),
                    new LongTripplet(unchecked((long)0x8123456789012345),    50,     -8120),
                    new LongTripplet(unchecked((long)0x8123456789012345),    370,     -8120),
                };

            foreach (LongTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletLong(it.c, ret, "i64.shr_s", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i64_shr_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.shr_u.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            long shiftR = -20;
            int idx = 0;
            for (; idx < testSamples.Count; ++idx, ++shiftR)
            {
                if (shiftR > 40)
                    shiftR = -5; // Don't need to go back so far in the negatives the second time around.

                ulong nv = (ulong)testSamples[idx];

                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(nv), PxPre.Datum.Val.Make(shiftR));
                UnitUtil.CompareGaunletLong((long)(nv >> (int)shiftR), ret, "i64.shr_u", idx, (long)nv, shiftR);
            }

            LongTripplet[] extraTests =
                new LongTripplet[]
                {
                    new LongTripplet(12345678,      6,      192901),
                    new LongTripplet(12345678,      38,     0),
                    new LongTripplet(12345678,      -6,     0),
                    new LongTripplet(12345678,      -16,    0),
                    new LongTripplet(12345678,      -48,    188),
                    // The 8 on the very left means the high bit is on
                    new LongTripplet(unchecked((long)0x8123456789012345),    -50,       567953953006596),
                    new LongTripplet(unchecked((long)0x8123456789012345),    -370,      567953953006596),
                    new LongTripplet(unchecked((long)0x8123456789012345),    50,        8264),
                    new LongTripplet(unchecked((long)0x8123456789012345),    370,       8264),
                };

            foreach (LongTripplet it in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(it.a), PxPre.Datum.Val.Make(it.b));
                UnitUtil.CompareGaunletLong(it.c, ret, "i64.shr_s", idx, it.a, it.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i64_rotl()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.rotl.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            LongTripplet[] extraTests =
            new LongTripplet[]
            {
                new LongTripplet(12345678,      6,      790123392),
                new LongTripplet(12345678,      38,     3393554128444588032),
                new LongTripplet(12345678,      -6,     4035225266124157317),
                new LongTripplet(12345678,      -16,    7011541669862441148),
                new LongTripplet(12345678,      -48,    809086353408),
                // The 8 on the very left means the high bit is on
                new LongTripplet(unchecked((long)0x8123456789012345),    -50,     -3361406881127899064),
                new LongTripplet(unchecked((long)0x8123456789012345),    -370,    -3361406881127899064),
                new LongTripplet(unchecked((long)0x8123456789012345),    50,      -8280425860874492924),
                new LongTripplet(unchecked((long)0x8123456789012345),    370,     -8280425860874492924),
            };

            int idx = 0;
            foreach (LongTripplet lt in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(lt.a), PxPre.Datum.Val.Make(lt.b));
                UnitUtil.CompareGaunletLong(lt.c, ret, "i64.rotl", idx, lt.a, lt.b);
                ++idx;
            }
        }
            
        [Test]
        public void Test_i64_rotr()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.rotr.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            LongTripplet[] extraTests =
            new LongTripplet[]
            {
                new LongTripplet(12345678,      6,      4035225266124157317),
                new LongTripplet(12345678,      38,     828504425889792),
                new LongTripplet(12345678,      -6,     790123392),
                new LongTripplet(12345678,      -16,    809086353408),
                new LongTripplet(12345678,      -48,    7011541669862441148),
                // The 8 on the very left means the high bit is on
                new LongTripplet(unchecked((long)0x8123456789012345),    -50,     -8280425860874492924),
                new LongTripplet(unchecked((long)0x8123456789012345),    -370,    -8280425860874492924),
                new LongTripplet(unchecked((long)0x8123456789012345),    50,      -3361406881127899064),
                new LongTripplet(unchecked((long)0x8123456789012345),    370,     -3361406881127899064),
            };

            int idx = 0;
            foreach (LongTripplet lt in extraTests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(lt.a), PxPre.Datum.Val.Make(lt.b));
                UnitUtil.CompareGaunletLong(lt.c, ret, "i64.rotr", idx, lt.a, lt.b);
                ++idx;
            }
        }                                                                                                                                        
    
    }
}