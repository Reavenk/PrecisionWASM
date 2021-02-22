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
        public struct DataOrgInfo
        {
            public string refName;
            public Bin.TypeID type;
            public bool isFloat;
            public uint size;
            public uint offset;
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
        

        public uint totalParamSize = 0;
        public uint totalResultSize = 0;

        public void InitializeOrganization()
        {
            this.totalParamSize = 0;
            this.totalResultSize = 0;

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
        }

        public static void FillInOrg(ref DataOrgInfo doi, ref uint totalSize)
        {
            doi.offset = totalSize;

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
    }
}