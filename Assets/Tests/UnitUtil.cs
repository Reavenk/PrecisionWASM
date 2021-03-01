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