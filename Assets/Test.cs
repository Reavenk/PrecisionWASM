using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [System.Serializable]
    public class UnitTest
    {
        [Multiline(10)]
        public string testScript;
    }

    public UnitTest [] test;

    // Some tests. This will probably eventually be converted
    // into formal (NUnit) unit tests when things get mature.
    // (wleu 02/22/2021)
    public string loadTargetEmpty;
    public string loadTargetSimple;
    public string loadTargetFactorial;
    public string loadTargetStuff;
    public string loadTargetMutableGlobals;
    public string loadTargetSaturatingFloatToInt;
    public string loadTargetSignExtension;
    public string loadTargetMultiValue;
    public string loadTargetBulkMemory;

    [Multiline(10)]
    public string testParse;

    public PxPre.WASM.Module module = null;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnGUI()
    {
        //
        // Tests and binaries for them at https://webassembly.github.io/wabt/demo/wat2wasm/
        //

        if (GUILayout.Button("Test Binary Empty") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetSimple);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            Debug.Log("Loaded Empty");
        }

        if(GUILayout.Button("Test Binary addTwo(10, 25)") == true)
        { 
            byte [] rb = System.IO.File.ReadAllBytes(this.loadTargetSimple);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            List<PxPre.Datum.Val> values = 
                ex.Invoke(
                    this.module, // TODO: Correct this when session gets removed from ExecutionContexts
                    "addTwo", 
                    new PxPre.Datum.ValInt(10),
                    new PxPre.Datum.ValInt(25));

            if(values.Count > 0)
                Debug.Log(values[0].GetString());
        }

        if(GUILayout.Button("Test Binary fac(9)") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetFactorial);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            List<PxPre.Datum.Val> values =
                ex.Invoke(
                    this.module,
                    "fac",
                    new PxPre.Datum.ValInt(9));

            if (values.Count > 0)
                Debug.Log(values[0].GetString());
        }

        if(GUILayout.Button("Test Binary stuff") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetStuff);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            ex.InvokeStart(this.module, true);
        }

        if(GUILayout.Button("Test Binary mutable globals") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetMutableGlobals);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            ex.Invoke(this.module, "f");
        }

        if(GUILayout.Button("Test Binary saturating float-to-int") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetSaturatingFloatToInt);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            // TODO: Handle infinities
            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            PxPre.Datum.Val ret = ex.Invoke_SingleRet(this.module, "f", new PxPre.Datum.ValFloat(5.5f));
            Debug.Log(ret.GetString());
        }

        if(GUILayout.Button("Test Binary sign extension") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetSignExtension);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            PxPre.Datum.Val ret = ex.Invoke_SingleRet(this.module, "f", new PxPre.Datum.ValInt(128));
            Debug.Log(ret.GetString());
        }

        if(GUILayout.Button("Test Binary multi value") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetMultiValue);
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            PxPre.Datum.Val ret = 
                ex.Invoke_SingleRet(
                    this.module,
                    "reverseSub", 
                    new PxPre.Datum.ValInt(10),
                    new PxPre.Datum.ValInt(3));

            Debug.Log(ret.GetString());
        }

        if (GUILayout.Button("Test Binary bulk memory") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetBulkMemory); 
            this.module = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.module);
            ex.Invoke(this.module, "fill", PxPre.Datum.Val.Make(0), PxPre.Datum.Val.Make(13), PxPre.Datum.Val.Make(5)); 
            ex.Invoke(this.module, "fill", PxPre.Datum.Val.Make(10), PxPre.Datum.Val.Make(77), PxPre.Datum.Val.Make(7));
            ex.Invoke(this.module, "fill", PxPre.Datum.Val.Make(20), PxPre.Datum.Val.Make(255), PxPre.Datum.Val.Make(1000));
        }

        if (GUILayout.Button("Parse") == true)
        { 
            byte [] rb = PxPre.WASM.Parser.ParseAndCompile(testParse);
            Debug.Log("Parsed!");
        }
    }
}
