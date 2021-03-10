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
    /// Unit test for memory store operators.
    /// </summary>
    public class Test_GauntletMemory
    {
        const string TestTheme = "Memory";

        const int PageSize = 64 * 1024;

        public static void CheckPageSizeOfMemory0(PxPre.WASM.ExecutionContext ex, int targetPageSize, string testName, int testID)
        { 
            if(ex.memories == null)
                throw new System.Exception($"Error on test {testName} for check {testID} : ExecutionContext is lacking expected local memory.");

            if(ex.memories.Count < 0)
                throw new System.Exception($"Error on test {testName} for check {testID} : ExecutionContext is lacking expected local memory.");

            if (ex.memories[0].CurByteSize != PageSize * targetPageSize)
                throw new System.Exception($"Error on test {testName} for check {testID} : Memory isn't the expected size of {PageSize * targetPageSize}, instead it's {ex.memories[0].CurByteSize}.");
        }

        [Test]
        public void Test_MemoryMax1()
        {
            string testName = "MemoryManips_Max1";

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/{testName}.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            // Make sure the truth is correct before we compare against it later.
            byte [] rb = UnitUtil.GetTestString_U();
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test_Size");
            UnitUtil.CompareGaunletInt(1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Grow size 0 does nothing, including no error code
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(0));
            UnitUtil.CompareGaunletInt(1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Handle error on negative numbers
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(-1));
            UnitUtil.CompareGaunletInt(-1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Handle error on this example since the max page size for this WASM is set to 1.
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(1));
            UnitUtil.CompareGaunletInt(-1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);
        }

        [Test]
        public void Test_MemoryMax10()
        {
            string testName = "MemoryManips_Max10";

            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/{testName}.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            // Make sure the truth is correct before we compare against it later.
            byte[] rb = UnitUtil.GetTestString_U();
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test_Size");
            UnitUtil.CompareGaunletInt(1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Grow size 0 does nothing, including no error code
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(0));
            UnitUtil.CompareGaunletInt(1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Handle error on negative numbers
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(-1));
            UnitUtil.CompareGaunletInt(-1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 1, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // The Max10 is the same WASM bytecode as Max1, except the max page size
            // has been changed to 10.
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(1));
            UnitUtil.CompareGaunletInt(1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 2, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Add 5 more pages
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(5));
            UnitUtil.CompareGaunletInt(2, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 7, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);

            // Add 5 more pages (should fail, too large)
            ret = ex.Invoke_SingleRet(mod, "Test_Exp", PxPre.Datum.Val.Make(5));
            UnitUtil.CompareGaunletInt(-1, ret, testName, 0);
            CheckPageSizeOfMemory0(ex, 7, testName, 0);
            UnitUtil.TestBytesMatchesForLen(rb, ex.memories[0].store.data, rb.Length, testName, -1);
        }
    }
}
