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
    /// <summary>
    /// In some instances, local and imported objects will share the same index space,
    /// even though they're stored in seperate containers. An IndexEntry will take
    /// a shared index space and point to their correct container index.
    /// </summary>
    public class IndexEntry
    {
        /// <summary>
        /// The source location where the actual object is stored.
        /// </summary>
        public enum FnIdxType
        { 
            /// <summary>
            /// The object exists in a local container.
            /// </summary>
            Local,

            /// <summary>
            /// The object exists in an import container.
            /// </summary>
            Import
        }

        /// <summary>
        /// The object source location.
        /// </summary>
        public FnIdxType type;

        /// <summary>
        /// The index in the object's source location.
        /// </summary>
        public int index;

        /// <summary>
        /// If the object is imported, what object does the module
        /// come from?
        /// </summary>
        public string module;

        /// <summary>
        /// If the object is imported, what is the field name in the
        /// module that it comes from?
        /// </summary>
        public string fieldname;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The object source location.</param>
        /// <param name="index">The index in the source location.</param>
        /// <param name="module">If an imported reference, the module the object came from.</param>
        /// <param name="fieldname">If an imported reference, the field name of the object.</param>
        protected IndexEntry(FnIdxType type, int index, string module, string fieldname)
        { 
            this.type = type;
            this.index = index;
            this.module = module;
            this.fieldname = fieldname;
        }

        /// <summary>
        /// Construction function for a local IndexEntry.
        /// </summary>
        /// <param name="index">The local container index.</param>
        /// <returns>The constructed local IndexEntry.</returns>
        public static IndexEntry CreateLocal(int index)
        {
            return 
                new IndexEntry(FnIdxType.Local, index, string.Empty, string.Empty);
        }

        /// <summary>
        /// Construction function for 
        /// </summary>
        /// <param name="index">The imported container index.</param>
        /// <param name="module">The module object came from.</param>
        /// <param name="fieldname">The field name of the object.</param>
        /// <returns>The constructed imported IndexEntry.</returns>
        public static IndexEntry CreateImport(int index, string module, string fieldname)
        {
            return 
                new IndexEntry(FnIdxType.Import, index, module, fieldname);
        }
    }
}
