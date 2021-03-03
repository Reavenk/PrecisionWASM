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
    public class Test_GauntletDataOps
    {
        const string TestTheme = "DataOps";

        // Matches the text string in the unit test WASMs
        const string MatchingTestDefMem = "I am the very model of a modern Major-Gineral\nI've information vegetable, animal, and mineral,\nI know the kings of England, and I quote the fights historical\nFrom Marathon to Waterloo, in order categorical;";

        // A prime number used to deterministicly generate pseuo-random numbers.
        // The method to generate random numbers also allows (encourages) bit overflows.
        const int LargePrime = 35317; // https://en.wikipedia.org/wiki/List_of_prime_numbers

        public static byte [] GetTestString_U()
        {
            return System.Text.ASCIIEncoding.ASCII.GetBytes(MatchingTestDefMem);
        }

        const int BytesIn_8_Bits = 1;
        const int BytesIn_16_Bits = 2;
        const int BytesIn_32_Bits = 4;

        /// <summary>
        /// A GetTestString_* used to set pulling signed data. Because the memory of the 
        /// unit tests are ASCII strings, they will never have a sign bit flipped on
        /// without some coercion.
        /// </summary>
        /// <param name="mem">The memory to perform the same operation on, to keep
        /// the memory for the tests equivalent.</param>
        /// <returns></returns>
        public static byte [] GetTestString_S(PxPre.WASM.Memory mem)
        { 
            // We add a sign bit to every other byte, or else the ASCII string will never 
            // have any negative values in it since every ASCII character code has the sign
            // bit off.
            byte [] rb = System.Text.ASCIIEncoding.ASCII.GetBytes(MatchingTestDefMem);
            for(int i = 0; i < rb.Length; i += 3)
            { 
                rb[i] = (byte)(rb[i] | (1 << 7));
                mem.store.data[i] = (byte)(mem.store.data[i] | (1<<7));
            }
            return rb;
        }

        public static void TestBytesMatchesForLen(byte [] rbtruth, byte [] rbtest, int len, string testName, int testId)
        { 
            for(int i = 0; i < len; ++i)
            { 
                if(rbtruth[i] != rbtest[i])
                    throw new System.Exception($"Error for test {testName} on iteration {testId} : the byte array index {i} expected {rbtruth[i]} but got {rbtest[i]}");
            }
        }

        public static void WriteBytes(byte [] dst, byte [] src, int offset)
        { 
            for(int i = 0; i < src.Length; ++i)
                dst[i + offset] = src[i];
        }

        public static void WriteBytesNum(byte[] dst, byte[] src, int offset, int num)
        {
            for (int i = 0; i < num; ++i)
                dst[i + offset] = src[i];
        }

        [Test]
        public void Test_i32_load()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.load(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte [] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletInt(System.BitConverter.ToInt32(rb, i), ret, "i32.load(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i64_load()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong(System.BitConverter.ToInt64(rb, i), ret, "i64.load(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_f32_load()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.load(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletFloat(System.BitConverter.ToSingle(rb, i), ret, "f32.load(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_f64_load()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.load(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletFloat64(System.BitConverter.ToDouble(rb, i), ret, "f64.load(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i32_load8_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.load8_s(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_S(ex.memories[0]);

            // There's an issue with signed items, in that we're testing on ASCII, which doesn't have 
            // a character code with the sign bit toggled on.
            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletInt((sbyte)rb[i], ret, "i32.load8_s(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i32_load8_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.load8_u(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletInt((sbyte)rb[i], ret, "i32.load8_u(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i32_load16_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.load16_s(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_S(ex.memories[0]);

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletInt(System.BitConverter.ToInt16(rb, i), ret, "i64.load(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i32_load16_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.load16_u(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletInt(System.BitConverter.ToUInt16(rb, i), ret, "i32.load16_u(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i64_load8_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load8_s(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_S(ex.memories[0]);

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong((sbyte)rb[i], ret, "i64.load8s(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i64_load8_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load8_u(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong(rb[i], ret, "i64.load8u(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i64_load16_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load16_s(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();


            byte[] rb = GetTestString_S(ex.memories[0]);

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong(System.BitConverter.ToInt16(rb, i), ret, "i64.load16_s(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i64_load16_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load16_u(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong(System.BitConverter.ToUInt16(rb, i), ret, "i64.load16_s(gineral).wasm", i, i);
            }
        }

        [Test]
        public void Test_i64_load32_s()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load32_s(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_S(ex.memories[0]);

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong(System.BitConverter.ToInt32(rb, i), ret, "i64.load32_s(gineral)", i, i);
            }
        }

        [Test]
        public void Test_i64_load32_u()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.load32_u(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] rb = GetTestString_U();

            for (int i = 0; i < 10; ++i)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(i));
                UnitUtil.CompareGaunletLong(System.BitConverter.ToUInt32(rb, i), ret, "i64.load32_u(gineral)", i, i);
            }
        }

        [Test]
        public void Test_i32_store()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.store(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte [] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i32.store(gineral)", -1);

            for(int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                int insVal = (int)((int)i * LargePrime);
                WriteBytes(rb, System.BitConverter.GetBytes(insVal), i);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i32.store(gineral)", i);
            }
        }

        [Test]
        public void Test_i64_store()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.store(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i64.store(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                long insVal = (long)((long)i * LargePrime);
                WriteBytes(rb, System.BitConverter.GetBytes(insVal), i);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i64.store(gineral)", i);
            }
        }

        [Test]
        public void Test_f32_store()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.store(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "f32.store(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                float insVal = (float)((float)(i * LargePrime) / 1000.0f);
                WriteBytes(rb, System.BitConverter.GetBytes(insVal), i);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "f32.store(gineral)", i);
            }
        }

        [Test]
        public void Test_f64_store()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.store(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "f64.store(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                double insVal = (double)((double)(i * LargePrime) / 1000.0);
                WriteBytes(rb, System.BitConverter.GetBytes(insVal), i);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "f64.store(gineral)", i);
            }
        }

        [Test]
        public void Test_i32_store8()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.store8(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i32.store8(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                int insVal = (int)((int)i * LargePrime);
                WriteBytesNum(rb, System.BitConverter.GetBytes(insVal), i, BytesIn_8_Bits);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i32.store8(gineral)", i);
            }
        }

        [Test]
        public void Test_i32_store16()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.store16(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i32.store16(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                int insVal = (int)((int)i * LargePrime);
                WriteBytesNum(rb, System.BitConverter.GetBytes(insVal), i, BytesIn_16_Bits);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i32.store16(gineral)", i);
            }
        }

        [Test]
        public void Test_i64_store8()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.store8(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i64.store8(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                long insVal = (long)((long)i * LargePrime);
                WriteBytesNum(rb, System.BitConverter.GetBytes(insVal), i, BytesIn_8_Bits);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i64.store8(gineral)", i);
            }
        }

        [Test]
        public void Test_i64_store16()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.store16(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i64.store16(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                long insVal = (long)((long)i * LargePrime);
                WriteBytesNum(rb, System.BitConverter.GetBytes(insVal), i, BytesIn_16_Bits);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i64.store16(gineral)", i);
            }
        }

        [Test]
        public void Test_i64_store32()                                                                                      
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.store32(gineral).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            byte[] mem = ex.memories[0].store.data;
            byte[] rb = GetTestString_U();
            TestBytesMatchesForLen(mem, rb, rb.Length, "i64.store32(gineral)", -1);

            for (int i = 0; i < 10; ++i)
            {
                // IDE Might mention the outer cast is unneccessary. Leave that warning in for saftey
                long insVal = (long)((long)i * LargePrime);
                WriteBytesNum(rb, System.BitConverter.GetBytes(insVal), i, BytesIn_32_Bits);
                ex.Invoke(mod, "Test", PxPre.Datum.Val.Make(i), PxPre.Datum.Val.Make(insVal));
                TestBytesMatchesForLen(rb, mem, rb.Length, "i64.store32(gineral)", i);
            }
        }
    }
}