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

namespace PxPre.WASM.Vali
{
    /// <summary>
    /// Stack Operand
    /// 
    /// The various values used to track and validate execution, matching
    /// the unique values in the WASM spec - for the reference algorithm in
    /// the appendix.
    /// 
    /// See type opd_stack in https://webassembly.github.io/spec/core/appendix/algorithm.html
    /// for more details.
    /// </summary>
    public enum StackOpd
    {
        /// <summary>
        /// The type is a 32 bit integer.
        /// </summary>
        i32,

        /// <summary>
        /// The type is a 64 bit integer.
        /// </summary>
        i64,

        /// <summary>
        /// The type is a 32 bit (single precision) float.
        /// </summary>
        f32,

        /// <summary>
        /// The type is a 64 bit (double precision) float.
        /// </summary>
        f64,

        /// <summary>
        /// The type is unknown.
        /// </summary>
        Unknown
    }
}