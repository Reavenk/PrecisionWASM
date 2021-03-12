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

namespace PxPre.WASM
{
    public class ImportFunction_Refl : ImportFunction
    {
        FunctionType fnTyVali = null;

        object invokingObject;
        System.Reflection.MethodInfo function;

        public ImportFunction_Refl(object fn, string functionName)
        { 
            System.Type ty = fn.GetType();
            this.function = ty.GetMethod(functionName);

            if(this.function == null)
                throw new System.Exception("Could not find a function matching the requested name for the host function.");

            this.ValidateFunction_Init();
        }

        public ImportFunction_Refl(object fn, System.Reflection.MethodInfo mi)
        { 
            this.invokingObject = fn;
            this.function = mi;

            this.ValidateFunction_Init();
        }

        public ImportFunction_Refl(System.Reflection.MethodInfo staticMeth)
        {
            this.invokingObject = null;
            this.function = staticMeth;

            this.ValidateFunction_Init();
        }

        private void ValidateFunction_Init()
        { 
            if(this.function == null)
                throw new System.Exception("Attempting to set null as host function.");
        }

        private void ValidateFunction_FirstUse(FunctionType fnTy)
        { 
            if(fnTy == null)
                throw new System.Exception("Attempting to validate against null function type.");

            if(fnTy.resultTypes.Count > 1)
                throw new System.Exception("Functions with multiple return values are not supported for imported reflection host functions.");

            if(fnTy.resultTypes.Count > 0)
            {
                System.Type retTy = this.function.ReturnType;
                switch(fnTy.resultTypes[0].type)
                { 
                    case Bin.TypeID.Int32:
                        if(retTy.FullName != "System.Int32" && retTy.FullName != "System.UInt32")
                            throw new System.Exception("Reflected host function did not match return type of int32.");
                        break;

                    case Bin.TypeID.Int64:
                        if (retTy.FullName != "System.Int64" && retTy.FullName != "System.UInt64")
                            throw new System.Exception("Reflected host function did not match return type of int64.");
                        break;

                    case Bin.TypeID.Float32:
                        if (retTy.FullName != "System.Single")
                            throw new System.Exception("Reflected host function did not match return type of float32.");
                        break;

                    case Bin.TypeID.Float64:
                        if (retTy.FullName != "System.Double")
                            throw new System.Exception("Reflected host function did not match return type of float64.");
                        break;

                    default:
                        throw new System.Exception("Unknown return type of reflection matched host function.");
                }
            }

            System.Reflection.ParameterInfo [] paramTys = this.function.GetParameters();
            if (paramTys.Length != fnTy.paramTypes.Count)
                throw new System.Exception("Reflected host function did not match parameter count.");

            for(int i = 0; i < fnTy.paramTypes.Count; ++i)
            {
                System.Reflection.ParameterInfo curPI = paramTys[i];
                switch (fnTy.paramTypes[i].type)
                { 
                    case Bin.TypeID.Int32:
                        if(curPI.ParameterType.FullName != "System.Int32" && curPI.ParameterType.FullName != "System.UInt32")
                            throw new System.Exception($"Reflected host function did not match parameter {i} to int32.");
                        break;

                    case Bin.TypeID.Int64:
                        if(curPI.ParameterType.FullName != "System.Int64" && curPI.ParameterType.FullName != "System.UInt64")
                            throw new System.Exception($"Reflected host function did not match parameter {i} to int64.");
                        break;

                    case Bin.TypeID.Float32:
                        if(curPI.ParameterType.FullName != "System.Single")
                            throw new System.Exception($"Reflected host function did not match parameter {i} to float32.");
                        break;

                    case Bin.TypeID.Float64:
                        if (curPI.ParameterType.FullName != "System.Double")
                            throw new System.Exception($"Reflected host function did not match parameter {i} to float64.");
                        break;

                    default:
                        throw new System.Exception("Unknown parameter of reflection matched host function.");
                }
            }

            // Cache the function type to flag the function as validated, and to store
            // the function type it was validated for.
            this.fnTyVali = fnTy;
        }

        public override byte[] InvokeImpl(ImportFunctionUtil utils)
        {
            if(this.fnTyVali == null)
                this.ValidateFunction_FirstUse(utils.functionType);
            else if(this.fnTyVali != utils.functionType)
                throw new System.Exception("Attempting to use reflected function instance for multiple types. No allowed.");

            List<object> lstParams = utils.GetParamsAsObjects();
            object ret = this.function.Invoke(this.invokingObject, lstParams.ToArray());

            if(ret == null)
                return null;

            return utils.ConvertObjectsToResult(new List<object>{ ret }); // This could no-doubt be optimized by making a non-list version
        }
    }
}