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
using UnityEngine;

namespace Tests
{
    public static class UnitUtil
    {
        public static byte[] LoadTestBytes(string path)
        {
            byte[] rb = System.IO.File.ReadAllBytes(path);
            if (rb == null || rb.Length == 0)
                throw new System.Exception("Test loaded empty or missing binary file.");

            Debug.Log($"Loading bytes {path} with byte count of {rb.Length}");

            return rb;
        }

        public static PxPre.WASM.Module LoadUnitTestModule(string path)
        {
            byte[] rb = LoadTestBytes(path);

            Debug.Log("Loading binary");
            PxPre.WASM.Module mod = PxPre.WASM.Module.LoadBinary(rb);
            if (mod == null)
                throw new System.Exception($"Error, failure parsing {path} to WASM module");


            Debug.Log($"Loaded with {mod.storeDecl.IndexingFunction.Count} functions total.");
            Debug.Log($"Loaded with {mod.functions.Count} functions locally.");

            return mod;
        }

        public static void AssertHasStart(PxPre.WASM.Module mod, bool hasStart)
        {
            if (hasStart == false)
            {
                if (mod.startFnIndex != 0xFFFFFFFF)
                    throw new System.Exception("Module expected to not have a starting function, but does.");
            }
            else
            {
                if (mod.startFnIndex == 0xFFFFFFFF)
                    throw new System.Exception("Module expected to have a starting function, but doesn't.");
            }
        }

        public static bool FloatEpsilon(float a, float b, float eps = 0.00001f)
        { 
            return Mathf.Abs(a - b) < eps;
        }

        public static void RunBiNOpGaunletThroughTripplet(
            PxPre.WASM.ExecutionContext exProgInst,
            PxPre.WASM.Module mod,
            IntTripplet it,
            string testName,
            int testID)
        {
            Debug.Log($"Running binop float test for {testName}, test number {testID}");

            PxPre.Datum.Val ret =
            exProgInst.Invoke_SingleRet(
                mod,
                "Test",
                PxPre.Datum.Val.Make(it.a),
                PxPre.Datum.Val.Make(it.b));

            if (ret.wrapType != PxPre.Datum.Val.Type.Int)
                throw new System.Exception("Invalid return type : expected int.");

            int nret = ret.GetInt();
            CompareGaunlet(it.c, nret, testName, testID, it.a, it.b);
        }

        public static void RunBiNOpGaunletThroughTripplet(
            PxPre.WASM.ExecutionContext exProgInst,
            PxPre.WASM.Module mod,
            LongTripplet lt,
            string testName,
            int testID)
        {
            Debug.Log($"Running binop int64 test for {testName}, test number {testID}");

            PxPre.Datum.Val ret =
            exProgInst.Invoke_SingleRet(
                mod,
                "Test",
                PxPre.Datum.Val.Make(lt.a),
                PxPre.Datum.Val.Make(lt.b));

            if (ret.wrapType != PxPre.Datum.Val.Type.Int64)
                throw new System.Exception("Invalid return type : expected int64.");

            long lret = ret.GetInt64();

            CompareGaunlet(lt.c, lret, testName, testID, lt.a, lt.b);

        }

        private static void ThrowGauntletError(string testName, int testId, string reason, object expected, object result, params object [] operands)
        {
            throw new System.Exception($"Invalid return value for {testName}, test {testId} with operands ({string.Join(", ", operands)}): {reason}.");
        }

        private static void CompareGaunlet<ty>(ty expected, ty result, string testName, int testId, params object[] operands)
            where ty : System.IEquatable<ty>
        {

            Debug.Log($"Testing {testName} with operands ({string.Join(", ", operands)}), expecting the value {expected} and with {result}");

            // https://stackoverflow.com/a/8982693
            if (!EqualityComparer<ty>.Default.Equals(expected, result))
                ThrowGauntletError(testName, testId, $"Expected {expected} but got {result}", expected, result, operands);
        }

        public static void CompareGaunletBool(bool expected, PxPre.Datum.Val valRes, string testName, int testId, params object[] operands)
        {
            if (valRes.wrapType != PxPre.Datum.Val.Type.Int)
                ThrowGauntletError(testName, testId, "Incorrect type, expected int (bool).", expected, valRes.GetInt(), operands);

            CompareGaunlet((expected == true) ? 1 : 0, valRes.GetInt(), testName, testId, operands);
        }

        public static void CompareGaunletInt(int expected, PxPre.Datum.Val valRes, string testName, int testId, params object [] operands)
        {
            if (valRes.wrapType != PxPre.Datum.Val.Type.Int)
                ThrowGauntletError(testName, testId, "Incorrect type, expected int.", expected, valRes.GetInt(), operands);

            CompareGaunlet(expected, valRes.GetInt(), testName, testId, operands);
        }

        public static void CompareGaunletFloat(float expected, PxPre.Datum.Val valRes, string testName, int testId, params object [] operands)
        {
            if (valRes.wrapType != PxPre.Datum.Val.Type.Float)
                ThrowGauntletError(testName, testId, "Incorrect type, expected float.", expected, valRes.GetFloat(), operands);

            if(float.IsNaN(expected) == true)
            { 
                if(float.IsNaN(valRes.GetFloat()) == false)
                    ThrowGauntletError(testName, testId, "Mishandled NaN.", expected, valRes.GetFloat(), operands);

                return;
            }

            CompareGaunlet(expected, valRes.GetFloat(), testName, testId, operands);
        }

        public static void CompareGaunletLong(long expected, PxPre.Datum.Val valRes, string testName, int testId, params object [] operands)
        {
            if (valRes.wrapType != PxPre.Datum.Val.Type.Int64)
                ThrowGauntletError(testName, testId, "Incorrect type, expected int64.", expected, valRes.GetInt(), operands);

            CompareGaunlet(expected, valRes.GetInt64(), testName, testId, operands);
        }

        public static void CompareGaunletFloat64(double expected, PxPre.Datum.Val valRes, string testName, int testId, params object[] operands)
        {
            if (valRes.wrapType != PxPre.Datum.Val.Type.Float64)
                ThrowGauntletError(testName, testId, "Incorrect type, expected int64.", expected, valRes.GetInt(), operands);

            if(double.IsNaN(expected) == true)
            {
                if (double.IsNaN(valRes.GetFloat64()) == false)
                    ThrowGauntletError(testName, testId, "Mishandled NaN.", expected, valRes.GetFloat(), operands);

                return;
            }

            CompareGaunlet(expected, valRes.GetFloat64(), testName, testId, operands);
        }

        public static void ExecuteAndCompareIntGuarded(
            System.Func<int> fn, 
            PxPre.WASM.Module mod,
            PxPre.WASM.ExecutionContext exexCtx,
            string testName, 
            int testId, 
            params PxPre.Datum.Val [] operands)
        { 

            List<string> operandStr = new List<string>();
            foreach(PxPre.Datum.Val v in operands)
                operandStr.Add(v.GetString());

            PxPre.Datum.Val ret = null;
            int truth = 0;

            System.Exception truthException = null;
            System.Exception testException = null;

            try
            { 
                truth = fn.Invoke();
            }
            catch(System.Exception ex)
            {
                truthException = ex;
            }

            try
            { 
                ret = exexCtx.Invoke_SingleRet(mod, "Test", operands);
            }
            catch(System.Exception ex)
            { 
                testException = ex;
            }

            if(truthException != null)
            { 
                if(testException == null)
                    ThrowGauntletError(testName, testId, "Did not throw expected exception.", null, null, operandStr.ToString());

                return;
            }
            else if (testException != null)
                ThrowGauntletError(testName, testId, "Uncountered unexpected exception.", null, null, operandStr.ToString());

            CompareGaunletInt(truth, ret, testName, testId, operandStr.ToArray());
        }

        public static void ExecuteAndCompareInt64Guarded(
            System.Func<long> fn,
            PxPre.WASM.Module mod,
            PxPre.WASM.ExecutionContext exexCtx,
            string testName,
            int testId,
            params PxPre.Datum.Val[] operands)
        {

            List<string> operandStr = new List<string>();
            foreach (PxPre.Datum.Val v in operands)
                operandStr.Add(v.GetString());

            PxPre.Datum.Val ret = null;
            long truth = 0;

            System.Exception truthException = null;
            System.Exception testException = null;

            try
            {
                truth = fn.Invoke();
            }
            catch (System.Exception ex)
            {
                truthException = ex;
            }

            try
            {
                ret = exexCtx.Invoke_SingleRet(mod, "Test", operands);
            }
            catch (System.Exception ex)
            {
                testException = ex;
            }

            if (truthException != null)
            {
                if (testException == null)
                    ThrowGauntletError(testName, testId, "Did not throw expected exception.", null, null, operandStr.ToString());

                return;
            }
            else if (testException != null)
                ThrowGauntletError(testName, testId, "Uncountered unexpected exception.", null, null, operandStr.ToString());

            CompareGaunletLong(truth, ret, testName, testId, operandStr.ToArray());
        }

        public static IEnumerable<IntPair> PermuZipLongToInt(List<long> lstA, List<long> lstB)
        { 
            foreach(long a in lstA)
            { 
                foreach(long b in lstB)
                { 
                    yield return new IntPair((int)a, (int)b);
                }
            }
        }

        public static IEnumerable<UIntPair> PermuZipLongToUInt(List<long> lstA, List<long> lstB)
        {
            foreach (long a in lstA)
            {
                foreach (long b in lstB)
                {
                    yield return new UIntPair((uint)a, (uint)b);
                }
            }
        }

        public static IEnumerable<Int64Pair> PermuZipLongToInt64(List<long> lstA, List<long> lstB)
        {
            foreach (long a in lstA)
            {
                foreach (long b in lstB)
                {
                    yield return new Int64Pair(a, b);
                }
            }
        }

        public static IEnumerable<UInt64Pair> PermuZipLongToUInt64(List<long> lstA, List<long> lstB)
        {
            foreach (long a in lstA)
            {
                foreach (long b in lstB)
                {
                    yield return new UInt64Pair((ulong)a, (ulong)b);
                }
            }
        }
    }
}