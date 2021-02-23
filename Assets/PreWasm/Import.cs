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
            if(paramIdx >= functionType.paramTypes.Count )
                throw new System.Exception(); // TODO: Provide error

            FunctionType.DataOrgInfo doi = functionType.paramTypes[paramIdx];

            if(throwIfNotStrongType == true && doi.type != Bin.TypeID.Int32)
                throw new System.Exception(); // TODO: provided error

            int memIdx = stackEnterIdx + (int)doi.offset;

            switch (doi.type)
            { 
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, memIdx);

                case Bin.TypeID.Float32:
                    return (int)System.BitConverter.ToSingle(executionContext.stack, memIdx);

                case Bin.TypeID.Int64:
                    return (int)System.BitConverter.ToInt64(executionContext.stack, memIdx);

                case Bin.TypeID.Float64:
                    return (int)System.BitConverter.ToDouble(executionContext.stack, memIdx);

            }


            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public float GetFloat32(int paramIdx, bool throwIfNotStrongType = false)
        {
            if (paramIdx >= functionType.paramTypes.Count)
                throw new System.Exception(); // TODO: Provide error

            FunctionType.DataOrgInfo doi = functionType.paramTypes[paramIdx];

            if (throwIfNotStrongType == true && doi.type != Bin.TypeID.Float32)
                throw new System.Exception(); // TODO: provided error

            int memIdx = stackEnterIdx + (int)doi.offset;

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, memIdx);

                case Bin.TypeID.Float32:
                    return System.BitConverter.ToSingle(executionContext.stack, memIdx);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, memIdx);

                case Bin.TypeID.Float64:
                    return (float)System.BitConverter.ToDouble(executionContext.stack, memIdx);

            }

            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public long GetInt64(int paramIdx, bool throwIfNotStrongType = false)
        {
            if (paramIdx >= functionType.paramTypes.Count)
                throw new System.Exception(); // TODO: Provide error

            FunctionType.DataOrgInfo doi = functionType.paramTypes[paramIdx];

            if (throwIfNotStrongType == true && doi.type != Bin.TypeID.Int64)
                throw new System.Exception(); // TODO: provided error

            int memIdx = stackEnterIdx + (int)doi.offset;

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, memIdx);

                case Bin.TypeID.Float32:
                    return (long)System.BitConverter.ToSingle(executionContext.stack, memIdx);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, memIdx);

                case Bin.TypeID.Float64:
                    return (long)System.BitConverter.ToDouble(executionContext.stack, memIdx);

            }

            throw new System.Exception("Attempt to access imported function parameter of unknown type.");
        }

        public double GetDouble64(int paramIdx, bool throwIfNotStrongType = false)
        {
            if (paramIdx >= functionType.paramTypes.Count)
                throw new System.Exception(); // TODO: Provide error

            FunctionType.DataOrgInfo doi = functionType.paramTypes[paramIdx];

            if (throwIfNotStrongType == true && doi.type != Bin.TypeID.Float64)
                throw new System.Exception(); // TODO: provided error

            int memIdx = stackEnterIdx + (int)doi.offset;

            switch (doi.type)
            {
                case Bin.TypeID.Int32:
                    return System.BitConverter.ToInt32(executionContext.stack, memIdx);

                case Bin.TypeID.Float32:
                    return (long)System.BitConverter.ToSingle(executionContext.stack, memIdx);

                case Bin.TypeID.Int64:
                    return System.BitConverter.ToInt64(executionContext.stack, memIdx);

                case Bin.TypeID.Float64:
                    return (long)System.BitConverter.ToDouble(executionContext.stack, memIdx);

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
    }

    public abstract class ImportFunction
    {
        public abstract byte [] InvokeImpl(ImportFunctionUtil utils);
    }
}
