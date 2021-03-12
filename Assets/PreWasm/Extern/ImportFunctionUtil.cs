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
    public struct ImportFunctionUtil
    { 
        public readonly FunctionType functionType;
        private readonly ExecutionContext executionContext;
        public int stackEnterIdx;

        public ImportFunctionUtil(FunctionType functionType, ExecutionContext executionCtx, int stackEnterIdx)
        { 
            this.functionType = functionType;
            this.executionContext = executionCtx;
            this.stackEnterIdx = stackEnterIdx;
        }

        public int GetInt32(int paramIdx, bool throwIfNotStrongType = false)
        { 
            if(paramIdx >= this.functionType.paramTypes.Count )
                throw new System.Exception("Attempting to access int parameter out of bounds.");

            FunctionType.DataOrgInfo doi = this.functionType.paramTypes[paramIdx];

            if(throwIfNotStrongType == true && doi.type != Bin.TypeID.Int32)
                throw new System.Exception("Attempting to access int parameter of different type.");

            int paramOnStack = this.stackEnterIdx + (int)this.functionType.GetParamStackOffset(paramIdx);

            switch (doi.type)
            { 
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float32:
                    return (int)System.BitConverter.ToSingle(executionContext.stack, paramOnStack);

                case Bin.TypeID.Int64:
                    return (int)System.BitConverter.ToInt64(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float64:
                    return (int)System.BitConverter.ToDouble(executionContext.stack, paramOnStack);

            }


            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public float GetFloat32(int paramIdx, bool throwIfNotStrongType = false)
        {
            if (paramIdx >= this.functionType.paramTypes.Count)
                throw new System.Exception("Attempting to access int parameter out of bounds.");

            FunctionType.DataOrgInfo doi = this.functionType.paramTypes[paramIdx];

            if (throwIfNotStrongType == true && doi.type != Bin.TypeID.Float32)
                throw new System.Exception("Attempting to access float parameter of different type.");

            int paramOnStack = this.stackEnterIdx + (int)this.functionType.GetParamStackOffset(paramIdx);

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float32:
                    return System.BitConverter.ToSingle(executionContext.stack, paramOnStack);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float64:
                    return (float)System.BitConverter.ToDouble(executionContext.stack, paramOnStack);

            }

            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public long GetInt64(int paramIdx, bool throwIfNotStrongType = false)
        {
            if (paramIdx >= functionType.paramTypes.Count)
                throw new System.Exception("Attempting to access int parameter out of bounds.");

            FunctionType.DataOrgInfo doi = functionType.paramTypes[paramIdx];

            if (throwIfNotStrongType == true && doi.type != Bin.TypeID.Int64)
                throw new System.Exception("Attempting to access int64 parameter of different type.");

            int paramOnStack = this.stackEnterIdx + (int)this.functionType.GetParamStackOffset(paramIdx);

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float32:
                    return (long)System.BitConverter.ToSingle(executionContext.stack, paramOnStack);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float64:
                    return (long)System.BitConverter.ToDouble(executionContext.stack, paramOnStack);
            }

            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public double GetDouble64(int paramIdx, bool throwIfNotStrongType = false)
        {
            if (paramIdx >= this.functionType.paramTypes.Count)
                throw new System.Exception("Attempting to access float64 parameter out of bounds.");

            FunctionType.DataOrgInfo doi = this.functionType.paramTypes[paramIdx];

            if (throwIfNotStrongType == true && doi.type != Bin.TypeID.Float64)
                throw new System.Exception("Attempting to access float64 parameter of different type.");

            int paramOnStack = this.stackEnterIdx + (int)this.functionType.GetParamStackOffset(paramIdx);

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float32:
                    return System.BitConverter.ToSingle(executionContext.stack, paramOnStack);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float64:
                    return System.BitConverter.ToDouble(executionContext.stack, paramOnStack);

            }

            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public object GetObject(int paramIdx)
        {
            if (paramIdx >= this.functionType.paramTypes.Count)
                throw new System.Exception("Attempting to access parameter out of bounds.");

            FunctionType.DataOrgInfo doi = this.functionType.paramTypes[paramIdx];

            int paramOnStack = this.stackEnterIdx + (int)this.functionType.GetParamStackOffset(paramIdx);

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float32:
                    return System.BitConverter.ToSingle(executionContext.stack, paramOnStack);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, paramOnStack);

                case Bin.TypeID.Float64:
                    return System.BitConverter.ToDouble(executionContext.stack, paramOnStack);
            }

            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public int ParamCount()
        { 
            return this.functionType.paramTypes.Count;
        }

        public IReadOnlyList<FunctionType.DataOrgInfo> ReturnValueTypes()
        { 
             return this.functionType.resultTypes;
        }

        public List<object> GetParamsAsObjects()
        {
            List<object> ret = new List<object>();

            for (int i = 0; i < this.functionType.paramTypes.Count; ++i)
            {
                FunctionType.DataOrgInfo doi = this.functionType.paramTypes[i];
                int paramOnStack = this.stackEnterIdx + (int)this.functionType.GetParamStackOffset(i);

                switch(doi.type)
                {
                    case Bin.TypeID.Int32:
                        ret.Add(System.BitConverter.ToInt32(executionContext.stack, paramOnStack));
                        break;

                    case Bin.TypeID.Float32:
                        ret.Add(System.BitConverter.ToSingle(executionContext.stack, paramOnStack));
                        break;

                    case Bin.TypeID.Int64:
                        ret.Add(System.BitConverter.ToInt64(executionContext.stack, paramOnStack));
                        break;

                    case Bin.TypeID.Float64:
                        ret.Add(System.BitConverter.ToDouble(executionContext.stack, paramOnStack));
                        break;

                    default:
                        throw new System.Exception("Attempt to access imported function parameter of unknown type.");
                }
            }
            return ret;
        }

        public byte [] ConvertObjectsToResult(List<object> lstObjs)
        { 
            int retCt = this.functionType.resultTypes.Count;
            if(retCt == 0 && lstObjs == null || lstObjs.Count == 0)
                return new byte[0];

            if(retCt != lstObjs.Count)
                throw new System.Exception("Result count mismatch while converting objects to results buffer.");

            byte [] ret = new byte[this.functionType.totalResultSize];

            for(int i = 0; i < retCt; ++i)
            {
                FunctionType.DataOrgInfo doi = this.functionType.resultTypes[i];
                int resultPos = (int)this.functionType.GetResultStackOffset(i);

                byte[] rb;
                switch (doi.type)
                { 
                case Bin.TypeID.Int32:
                    rb = System.BitConverter.GetBytes((int)lstObjs[i]);
                    break;

                case Bin.TypeID.Float32:
                    rb = System.BitConverter.GetBytes((float)lstObjs[i]);
                    break;

                case Bin.TypeID.Int64:
                    rb = System.BitConverter.GetBytes((long)lstObjs[i]);
                    break;

                case Bin.TypeID.Float64:
                    rb = System.BitConverter.GetBytes((double)lstObjs[i]);
                    break;

                default:
                    throw new System.Exception();
                }

                // We're making somewhat's quirky rules where the ImportFunctionUtil isn't 
                // categorized as the core low-level part, so we're going to keep the code
                // safe. And AFAICT, there isn't a safe byte conversion that writes directly
                // in a byte array - so we write to an array and then transfer it, which has
                // quite a few places where overhead is incurred.
                for(int j = 0; j < rb.Length; ++j)
                    ret[resultPos + j] = rb[j];
            }

            return ret;
        }
    }
}
