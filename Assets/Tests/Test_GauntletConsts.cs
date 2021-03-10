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
    public class Test_GauntletConsts
    {
        const string TestTheme = "Consts";

        [Test]
        public void Test_i32_const()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i32.const(12345678).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test");
            UnitUtil.CompareGaunletInt(12345678, ret, "i32.const(12345678).wasm", 0);
        }

        [Test]
        public void Test_i64_const()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/i64.const(12345678901234).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test");
            UnitUtil.CompareGaunletInt64(12345678901234, ret, "i64.const(12345678901234).wasm", 0);
        }

        [Test]
        public void Test_f32_const()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f32.const(12345678.12345).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test");
            UnitUtil.CompareGaunletFloat(12345678.12345f, ret, "f32.const(12345678.12345).wasm", 0);
        }

        [Test]
        public void Test_f64_const()
        {
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule($"TestSamples/Gauntlet/{TestTheme}/f64.const(123456789012345.123456789012345).wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "Test");
            UnitUtil.CompareGaunletFloat64(123456789012345.123456789012345, ret, "f64.const(123456789012345.123456789012345).wasm", 0);
        }
    }
}