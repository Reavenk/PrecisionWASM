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
    }
}