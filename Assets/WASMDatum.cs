﻿// MIT License
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

using PxPre.WASM;

// Not in the PxPre.WASM namespace so the extention functions
// are easier to integrate with things outside the namespace.


public static class WASMDatum
{
    public static unsafe List<PxPre.Datum.Val> Invoke(
        this PxPre.WASM.ExecutionContext ex,
        PxPre.WASM.Module module,
        string fnName, params PxPre.Datum.Val[] ps)
    {
        int fnIdx = module.GetExportedFunctionID(fnName);

#if STRICT_PREWASM
        if (fnIdx == -1)
            throw new System.Exception("Missing module function.");
#endif
        if (fnIdx == -1)
            return null;

        return Invoke(ex, module, fnIdx, ps);
    }

    public static unsafe PxPre.Datum.Val Invoke_SingleRet(
        this PxPre.WASM.ExecutionContext ex,
        PxPre.WASM.Module module,
        string fnName, 
        params PxPre.Datum.Val[] ps)
    {
        int fnIdx = module.GetExportedFunctionID(fnName);

#if STRICT_PREWASM
        if(fnIdx == -1)
            throw new System.Exception("Missing module function.");
#endif
        if (fnIdx == -1)
            return null;

        List<PxPre.Datum.Val> rets = Invoke(ex, module, fnIdx, ps);

#if STRICT_PREWASM
        if (rets.Count != 1)
            throw new System.Exception("Call to WASM function expected to return 1 result, had illegal return values count.");
#endif
        return rets[0];
    }

    public static unsafe List<PxPre.Datum.Val> Invoke(
        this PxPre.WASM.ExecutionContext ex,
        PxPre.WASM.Module module,
        int index, 
        params PxPre.Datum.Val[] ps)
    {
        Function fn = module.functions[index];
        return Invoke(ex, fn, ps);
    }

    public static unsafe List<PxPre.Datum.Val> Invoke(
        this PxPre.WASM.ExecutionContext ex,
        PxPre.WASM.Function fn, 
        params PxPre.Datum.Val[] ps)
    {
        FunctionType fnty = fn.parentModule.types[(int)fn.typeidx];

        if (ps.Length < fnty.paramTypes.Count)
            throw new System.Exception($"Invalid parameter count, expected {fnty.paramTypes.Count} but got {ps.Length}");

        int origStackPos = ex.stackPos;

        List<PxPre.Datum.Val> ret = new List<PxPre.Datum.Val>();

        // Move the stack position right past where the parameters were written to.
        ex.stackPos -= (int)fn.totalStackSize;

        // Transfer parameters to a native version the bytecode works on
        fixed (byte* pb = fn.expression, pstk = ex.stack)
        {
            for (int i = 0; i < fnty.paramTypes.Count; ++i)
            {
                FunctionType.DataOrgInfo doi = fnty.paramTypes[i];

                if (doi.size == 4)
                {
                    if (doi.isFloat == true)
                        *(float*)(&pstk[ex.stackPos + fn.totalStackSize - doi.offset - 4]) = ps[i].GetFloat();
                    else
                        *(int*)(&pstk[ex.stackPos + fn.totalStackSize - doi.offset - 4]) = ps[i].GetInt();
                }
                else if (doi.size == 8)
                {
                    if (doi.isFloat == true)
                        *(double*)(&pstk[ex.stackPos + fn.totalStackSize - doi.offset - 8]) = ps[i].GetFloat64();
                    else
                        *(long*)(&pstk[ex.stackPos + fn.totalStackSize - doi.offset - 8]) = ps[i].GetInt64();
                }
                else
                { } // TODO: Error

            }

            ex.RunFunction(fn);

            // extract the output variables.
            for (int i = 0; i < fnty.resultTypes.Count; ++i)
            {
                FunctionType.DataOrgInfo doi = fnty.resultTypes[i];

                PxPre.Datum.Val v = null;
                if (doi.size == 4)
                {
                    if (doi.isFloat == true)
                        v = new PxPre.Datum.ValFloat(*(float*)(&pstk[ex.stackPos + fnty.totalResultSize - doi.offset - 4]));
                    else
                        v = new PxPre.Datum.ValInt(*(int*)(&pstk[ex.stackPos + fnty.totalResultSize - doi.offset - 4]));
                }
                else if (doi.size == 8)
                {
                    if (doi.isFloat == true)
                        v = new PxPre.Datum.ValFloat64(*(double*)(&pstk[ex.stackPos + fnty.totalResultSize - doi.offset - 8]));
                    else
                        v = new PxPre.Datum.ValInt64(*(long*)(&pstk[ex.stackPos + fnty.totalResultSize - doi.offset - 8]));
                }
                else
                { } // TODO: Error

                if (v != null)
                    ret.Add(v);
            }


            // Move the stack before the parameter positioning.
            ex.stackPos = origStackPos;
        }

        return ret;
    }
}
