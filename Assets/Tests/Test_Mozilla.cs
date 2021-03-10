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
    /// Unit tests derived from WATs on Mozilla's WASM documentation
    /// 
    /// https://developer.mozilla.org/en-US/docs/WebAssembly/Understanding_the_text_format
    /// </summary>
    public class Test_Mozilla
    {
        const string TestTheme = "Mozilla";

        [Test]
        public void Test_VTable()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/{TestTheme}/vtable.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val v0 = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(0));
            UnitUtil.CompareGaunletInt(42, v0, "vtable", 0, 0);

            PxPre.Datum.Val v1 = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(1));
            UnitUtil.CompareGaunletInt(13, v1, "vtable", 1, 1);

            bool threw = false;
            try
            { 
                PxPre.Datum.Val v2 = ex.Invoke_SingleRet(mod, "Test", PxPre.Datum.Val.Make(2));
            }
            catch(System.Exception)
            { 
                threw = true;
            }

            if(threw == false)
                throw new System.Exception("Expected out of bounds exception, but test did not throw.");
        }
    }
}