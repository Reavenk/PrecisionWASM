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

using System.Collections.Generic;

namespace PxPre.WASM
{
    public class FunctionType
    {

        /// <summary>
        /// A record of data organized for function locals (i.e., parameters
        /// and local variables).
        /// </summary>
        public struct DataOrgInfo
        {
            /// <summary>
            /// Currently unused. Intended to be the symbolic name in the WAT.
            /// </summary>
            public string refName;

            /// <summary>
            /// The variable type.
            /// </summary>
            public Bin.TypeID type;

            /// <summary>
            /// Cached record of if type is a float type.
            /// </summary>
            public bool isFloat;

            /// <summary>
            /// Cached record of the byte count of type.
            /// </summary>
            public uint size;

            /// <summary>
            /// The offset in bytes from the start. This is needed for figuring out
            /// alignment, but is actually the opposite of what we need.
            /// </summary>
            public uint alignmentCtr;
        }

        /// <summary>
        /// Function type ID. This points to an index in the parent Module's
        /// types list.
        /// </summary>
        public uint typeid;

        /// <summary>
        /// A listing of the types for the function's return values.
        /// </summary>
        public List<DataOrgInfo> paramTypes = new List<DataOrgInfo>();

        /// <summary>
        /// A listing of the types for the function's return values.
        /// </summary>
        public List<DataOrgInfo> resultTypes = new List<DataOrgInfo>();

        /// <summary>
        /// The byte alignment for return values.
        /// </summary>
        private List<uint> resultByteOffsets = new List<uint>();

        /// <summary>
        /// The byte alignment for parameters. Note that this should not be 
        /// confused with local variable alignment, which includes variables
        /// on the function stack.
        /// </summary>
        private List<uint> paramByteOffsets = new List<uint>();
        
        /// <summary>
        /// The number of bytes needed for the parameter variables.
        /// </summary>
        public uint totalParamSize = 0;

        /// <summary>
        /// The number of bytes needed for the return value.
        /// </summary>
        public uint totalResultSize = 0;

        public void InitializeOrganization()
        {
            this.totalParamSize = 0;
            this.totalResultSize = 0;

            // Pass 1. Get inverted offsets and get the total stack size
            for(int i = 0; i < this.paramTypes.Count; ++i)
            { 
                DataOrgInfo doi = this.paramTypes[i];
                FillInOrg(ref doi, ref this.totalParamSize);
                this.paramTypes[i] = doi;
            }

            for (int i = 0; i < this.resultTypes.Count; ++i)
            {
                DataOrgInfo doi = this.resultTypes[i];
                FillInOrg(ref doi, ref this.totalResultSize);
                this.resultTypes[i] = doi;
            }

            foreach(DataOrgInfo doi in this.paramTypes)
                this.paramByteOffsets.Add(this.totalParamSize - doi.alignmentCtr - doi.size);

            foreach(DataOrgInfo doi in this.resultTypes)
                this.resultByteOffsets.Add(this.totalResultSize - doi.alignmentCtr - doi.size);

        }

        public static void FillInOrg(ref DataOrgInfo doi, ref uint totalSize)
        {
            doi.alignmentCtr = totalSize;

            switch (doi.type)
            {
                case Bin.TypeID.Float32:
                    doi.isFloat = true;
                    doi.size = 4;
                    break;

                case Bin.TypeID.Float64:
                    doi.isFloat = true;
                    doi.size = 8;
                    break;

                case Bin.TypeID.Int32:
                    doi.isFloat = false;
                    doi.size = 4;
                    break;

                case Bin.TypeID.Int64:
                    doi.isFloat = false;
                    doi.size = 8;
                    break;
            }

            totalSize += doi.size;
        }

        public uint GetResultStackOffset(int resultIdx)
        { 
            return this.resultByteOffsets[resultIdx];
        }

        public uint GetParamStackOffset(int paramIdx)
        {
            return this.paramByteOffsets[paramIdx];
        }
    }
}