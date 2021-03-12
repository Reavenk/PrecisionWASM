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

namespace PxPre.WASM
{
    /// <summary>
    /// A base class that can be derived to hold implementations of 
    /// imported functions (aka., host functions).
    /// 
    /// See ImportFunction_Lam and ImportFunction_Refl for examples
    /// of implementations.
    /// </summary>
    public abstract class ImportFunction
    {
        /// <summary>
        /// A cache of the function type. See SetFunctionType() for 
        /// more details.
        /// </summary>
        public FunctionType functionType {get; private set; } = null;

        /// <summary>
        /// The seal for setting functionType.
        /// </summary>
        private bool setFnTy = false;

        /// <summary>
        /// The function for 
        /// </summary>
        /// <param name="utils">Liason class to grab function parameters, 
        /// as well as other utilities.</param>
        /// <returns>The return value of the function. This must match the
        /// number of bytes the return value takes up in WASM.</returns>
        public abstract byte[] InvokeImpl(ImportFunctionUtil utils);

        /// <summary>
        /// Set the cached function type.
        /// This can only be set once, andd is expected to be set
        /// within ImportModule.SetFunction().
        /// </summary>
        /// <param name="fnty">The function type.</param>
        public void SetFunctionType(FunctionType fnty)
        {
            // This can only be set once.
            if (this.setFnTy == true)
                return;

            this.functionType = fnty;
            setFnTy = true;
        }


    }
}