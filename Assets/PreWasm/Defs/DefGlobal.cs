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
    public struct DefGlobal
    {
        public readonly int index;

        public readonly Bin.TypeID type;
        public readonly int elements;
        public readonly Global.Mutability mut;
        public byte [] defaultValue;

        private DefGlobal(int index, Bin.TypeID type, int elements, bool mutable, byte [] defaultValue)
        { 
            this.index = index;
            this.defaultValue = defaultValue;

            this.type = type;
            this.elements = elements;
            this.mut = mutable ? Global.Mutability.Variable : Global.Mutability.Const;
        }

        public DefGlobal(int index, Bin.TypeID type, bool mutable)
            : this(index, type, 1, mutable, null)
        { }

        public DefGlobal(int index, int defaultVal, bool mutable)
            : this(index, Bin.TypeID.Int32, 1, mutable, System.BitConverter.GetBytes(defaultVal))
        {}

        public DefGlobal(int index, float defaultVal,  bool mutable)
            : this(index, Bin.TypeID.Float32, 1, mutable, System.BitConverter.GetBytes(defaultVal))
        { }

        public DefGlobal(int index, long defaultVal, bool mutable)
            : this(index, Bin.TypeID.Int64, 1, mutable, System.BitConverter.GetBytes(defaultVal))
        { }

        public DefGlobal(int index, double defaultVal, bool mutable)
            : this(index, Bin.TypeID.Float64, 1, mutable, System.BitConverter.GetBytes(defaultVal))
        { }

        public Global CreateDefault()
        { 
            bool mutable = this.mut == Global.Mutability.Variable;

            if (this.type == Bin.TypeID.Int32)
            { 
                int value = 0;
                if(this.defaultValue != null && this.defaultValue.Length >= 4)
                    value = System.BitConverter.ToInt32(this.defaultValue, 0);

                return new GlobalInt(value, mutable);
            }
            else if(this.type == Bin.TypeID.Float32)
            { 
                float value = 0.0f;
                if (this.defaultValue != null && this.defaultValue.Length >= 4)
                    value = System.BitConverter.ToSingle(this.defaultValue, 0);

                return new GlobalFloat(value, mutable);
            }
            else if(this.type == Bin.TypeID.Int64)
            { 
                long value = 0;
                if (this.defaultValue != null && this.defaultValue.Length >= 8)
                    value = System.BitConverter.ToInt64(this.defaultValue, 0);

                return new GlobalInt64(value, mutable);
            }
            else if (this.type == Bin.TypeID.Float64)
            {
                double value = 0.0;
                if (this.defaultValue != null && this.defaultValue.Length >= 8)
                    value = System.BitConverter.ToDouble(this.defaultValue, 0);

                return new GlobalFloat64(value, mutable);
            }

            throw new System.Exception("Attempting to instanciate global with invalid global type.");
        }
    }
}