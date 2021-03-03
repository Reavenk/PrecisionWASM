
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class Test_Wat2WASM_Demo
    {
        [Test]
        public void Test_Empty()
        {
            // https://webassembly.github.io/wabt/demo/wat2wasm/ : "empty"

            // Basic test for empty program parsing.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/empty.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();
        }

        [Test]
        public void Test_Simple()
        {
            // https://webassembly.github.io/wabt/demo/wat2wasm/ : "simple"

            // Basic test for function calling.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/simple.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<IntPair> tests = 
                new List<IntPair>
                {
                    new IntPair(10, 25),
                    new IntPair(50, 100),
                    new IntPair(-90, 50023),
                    new IntPair(50023, -90 ),
                    new IntPair(-90, -50023),
                    new IntPair(90, -50023),
                    new IntPair(654321, 123456)
                };

            foreach(IntPair np in tests)
            {
                int expected = np.a + np.b;
                Debug.Log($"Testing addTwo({np.a}, {np.b}) and expecting {expected}");


                PxPre.Datum.Val ret = 
                    ex.Invoke_SingleRet( 
                        mod, 
                        "addTwo", 
                        new PxPre.Datum.ValInt(np.a), 
                        new PxPre.Datum.ValInt(np.b));

                if (ret.GetInt() != expected)
                    throw new System.Exception($"Test of addTwo({np.a}, {np.b}) failed with result of {ret.GetInt()} instead of {expected}");
            }


        }

        // The equivalent of TestSamples/factorial.wasm
        double FFactorial(double d)
        {   
            if (d < 0)
                return 1.0;

            return d * FFactorial(d - 1.0);
        }

        [Test]
        public void Test_Factorial()
        {
            // https://webassembly.github.io/wabt/demo/wat2wasm/ : "factorial"

            // Basic test for recursive function calling.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/factorial.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<double> entries = new List<double>{ -90.0, 0.0, 1.0, 2.0, 4.0, 4.5, 7.0, 7.5, 9.0, 9.9, 10.0, 11.0 };

            foreach(double d in entries)
            { 
                double truth = FFactorial(d);
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "fac", new PxPre.Datum.ValFloat64(d));
            }
        }

        [Test]
        public void Test_Stuff()
        {
            // https://webassembly.github.io/wabt/demo/wat2wasm/ : "stuff"

            // Basic test for various features.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/stuff.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, true);
            ex.InvokeStart();

            if(mod.storeDecl.importFunctionsCt != 1)
                throw new System.Exception("Expected stuff.wasm to have 1 imported function");

            if(ex.tables.Count != 1)
                throw new System.Exception("Expected stuff.wasm to have 1 table");

            if(mod.GetExportedFunctionID("e") == -1)
                throw new System.Exception("Expected stuff.wasm to have an exported function \"e\"");

        }

        [Test]
        public void Test_MutableGlobals()
        {
            // https://webassembly.github.io/wabt/demo/wat2wasm/ : "mutable globals"

            // Basic test for imported globals that can be modified.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/mutable_globals.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            if(ex.importData.globals.Count != 1)
                throw new System.Exception("Expected mutable_globals.wasm to have 1 imported global");

            if(ex.importData.Validate() != false)
                throw new System.Exception("Expected mutable_globals.wasm to fail validation.");

            ex.importData.GetGlobalOrDefault("env", "g");

            if (ex.importData.Validate() == false)
                throw new System.Exception("Expected mutable_globals.wasm to pass validation.");

            if (ex.importData.globals[0].type != PxPre.WASM.Bin.TypeID.Int32)
                throw new System.Exception("Expected mutable_globals.wasm imported global to be of type f32");

            if (System.BitConverter.ToInt32(ex.importData.globals[0].store.data, 0) != 0)
                throw new System.Exception("Expected mutable_globals.wasm imported global to have a start value of 0");

            ex.Invoke(mod, "f");

            if (System.BitConverter.ToInt32(ex.importData.globals[0].store.data, 0) != 100)
                throw new System.Exception("Expected mutable_globals.wasm imported global expected to have an ending value of 100");

        }

        [Test]
        public void Test_SaturatingFloatToInt()
        {
            // Basic test for the saturating float to int feature.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/saturatingfloattoint.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List<FloatIntPair> tests = 
                new List<FloatIntPair>
                { 
                    //new FloatIntPair(float.PositiveInfinity, 2147483647), // unsigned can't be represented with Datum values
                    new FloatIntPair(float.NegativeInfinity, -2147483648),
                    new FloatIntPair(5000.0f, 5000),
                    new FloatIntPair(0.0f, 0),
                    new FloatIntPair(99.99f, 99),
                    new FloatIntPair(-99.99f, -99),
                };

            foreach(FloatIntPair fip in tests)
            { 
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "f", new PxPre.Datum.ValFloat(fip.f));

                if(ret.wrapType != PxPre.Datum.Val.Type.Int)
                    throw new System.Exception("saturingfloattoint.wasm expected i32 return.");

                if(ret.GetInt() != fip.n)
                    throw new System.Exception($"saturatingfloattoint.wasm with f{fip.f} expected {fip.n} but got {ret.GetInt()}");
            }
        }

        [Test]
        public void Test_SignExtension()
        {
            // Basic test for the sign extension function.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/signextension.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            List< IntPair> tests = 
                new List<IntPair>
                { 
                    new IntPair(0,      0),
                    new IntPair(127,    127),
                    new IntPair(128,    -128),
                    new IntPair(255,    -1),
                };

            foreach(IntPair ip in tests)
            {
                PxPre.Datum.Val ret = ex.Invoke_SingleRet(mod, "f", new PxPre.Datum.ValInt(ip.a));
                
                if(ret.GetInt() != ip.b)
                    throw new System.Exception($"signextension.wasm with f({ip.a}) expected {ip.b} but got {ret.GetInt()}");
            }

        }

        [Test]
        public void Test_MultiValue()
        {
            // Basic test for function with multiple parameters and multiple result values.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/multivalue.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);

            UnitUtil.AssertHasStart(mod, false);
            ex.InvokeStart();

            PxPre.Datum.Val ret =
                ex.Invoke_SingleRet(
                    mod,
                    "reverseSub",
                    new PxPre.Datum.ValInt(10),
                    new PxPre.Datum.ValInt(3));

            if(ret.GetInt() != -7)
                throw new System.Exception($"multivalue.wasm expected a return value of -7, instead got {ret.GetInt()}");
        }

        [Test]
        public void Test_BulkMemory()
        {
            // Basic test for bulk memory feature to write large arrays of data with repeating
            // content.
            PxPre.WASM.Module mod = UnitUtil.LoadUnitTestModule("TestSamples/bulkmemory.wasm");
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(mod);
            ex.InvokeStart();

            ex.Invoke(mod, "fill", PxPre.Datum.Val.Make(0), PxPre.Datum.Val.Make(13), PxPre.Datum.Val.Make(5));
            ex.Invoke(mod, "fill", PxPre.Datum.Val.Make(10), PxPre.Datum.Val.Make(77), PxPre.Datum.Val.Make(7));
            ex.Invoke(mod, "fill", PxPre.Datum.Val.Make(20), PxPre.Datum.Val.Make(255), PxPre.Datum.Val.Make(1000));

            string res = "13,13,13,13,13,0,0,0,0,0,77,77,77,77,77,77,77,0,0,0,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255";
            string [] resToks = res.Split(new char[]{',' }, System.StringSplitOptions.RemoveEmptyEntries);

            List<byte> rb = new List<byte>();
            foreach(string str in resToks)
                rb.Add(byte.Parse(str));

            for(int i = 0; i < rb.Count; ++i)
            { 
                if(rb[i] != ex.memories[0].store.data[i])
                    throw new System.Exception("bulkmemory.wasm memory result not matching expected end state");
            }
        }
    }
}
