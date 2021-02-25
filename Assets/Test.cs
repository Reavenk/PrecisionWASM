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

        if (GUILayout.Button("Parse") == true)
        { 
            byte [] rb = PxPre.WASM.Parser.ParseAndCompile(testParse);
            Debug.Log("Parsed!");
        }
    }
}
