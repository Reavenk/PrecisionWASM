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

    public string loadTargetEmpty;
    public string loadTargetSimple;
    public string loadTargetFactorial;

    [Multiline(10)]
    public string testParse;

    public PxPre.WASM.Module session = null;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Test Binary Empty") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetSimple);
            this.session = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.session);
            Debug.Log("Loaded Empty");
        }

        if(GUILayout.Button("Test Binary addTwo(10, 25)") == true)
        { 
            byte [] rb = System.IO.File.ReadAllBytes(this.loadTargetSimple);
            this.session = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.session);
            List<PxPre.Datum.Val> values = 
                ex.RunFunction(
                    "addTwo", 
                    new PxPre.Datum.ValInt(10),
                    new PxPre.Datum.ValInt(25));

            if(values.Count > 0)
                Debug.Log(values[0].GetString());
        }

        if(GUILayout.Button("Test Binary fac(9)") == true)
        {
            byte[] rb = System.IO.File.ReadAllBytes(this.loadTargetFactorial);
            this.session = PxPre.WASM.Module.LoadBinary(rb);

            PxPre.WASM.ExecutionContext ex = new PxPre.WASM.ExecutionContext(this.session);
            List<PxPre.Datum.Val> values =
                ex.RunFunction(
                    "fac",
                    new PxPre.Datum.ValInt(9));

            if (values.Count > 0)
                Debug.Log(values[0].GetString());
        }



        if(GUILayout.Button("Parse") == true)
        { 
            byte [] rb = PxPre.WASM.Parser.ParseAndCompile(testParse);
            Debug.Log("Parsed!");
        }
    }
}
