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
    public class Module
    {
        // TODO: Check if can be removed.
        public Dictionary<string, PxPre.Datum.Val> exposed = 
            new Dictionary<string, PxPre.Datum.Val>();

        public enum Section
        { 
            CustomSec   = 0,
            TypeSec     = 1,
            ImportSec   = 2,
            FunctionSec = 3,
            TableSec    = 4,
            MemorySec   = 5,
            GlobalSec   = 6,
            ExportSec   = 7,
            StartSec    = 8,
            ElementSec  = 9,
            CodeSec     = 10,
            DataSec     = 11
        }

        public enum TypeID
        { 
            Function    = 0x60, 
            Int32       = 0x7F,
            Int64       = 0x7E,
            Float32     = 0x7D,
            Float64     = 0x7C
        }

         public const uint UnloadedStartIndex = unchecked((uint)~0);

        public List<FunctionType> types = new List<FunctionType>();
        public List<Export> exports = new List<Export>();
        public List<Function> functions = new List<Function>();
        public List<Memory> memories = new List<Memory>();

        public GlobalDirectory globals = new GlobalDirectory();

        public uint startFnIndex = UnloadedStartIndex;

        unsafe public static Module LoadBinary(byte [] rb)
        { 
            fixed(byte * pb = rb)
            { 
                uint idx = 0;
                return LoadBinary(pb, ref idx);
            }
        }

        unsafe public static Module LoadBinary(byte * pb, ref uint idx)
        {
            // https://www.reddit.com/r/WebAssembly/comments/9vq019/is_anyone_learning_webassembly_in_binary_im_stuck/
            // https://webassembly.github.io/wabt/demo/wat2wasm/

            if (*((int*)(&pb[idx])) != WASM.WASM_BINARY_MAGIC)
                return null;

            idx += 4;

            if(*(int*)&pb[idx] != WASM.WASM_BINARY_VERSION)
                return null;

            idx += 4;

            Module ret = new Module();

            while(true)
            {
                Section sectionCode = (Section)pb[idx];
                ++idx;

                uint sectionSize = LoadUnsignedLEB32(pb, ref idx);

                if(sectionCode == Section.CustomSec)
                { 
                    uint end = idx + sectionSize;

                    while(true)
                    { 
                        return ret;

                        // TODO: Considered end and unprocessed for now.
                        //
                        // uint nameLen = LoadUnsignedLEB32(pb, ref idx);
                        // string customSecName = LoadString(pb, nameLen, ref idx);
                        // 
                        // uint subType = LoadUnsignedLEB32(pb, ref idx);
                        // uint subSize = LoadUnsignedLEB32(pb, ref idx);
                    }
                }
                else if(sectionCode == Section.TypeSec)
                { 
                    uint numTypes = LoadUnsignedLEB32(pb, ref idx);
                    for(uint i = 0; i < numTypes; ++i)
                    {
                        TypeID type = (TypeID)LoadUnsignedLEB32(pb, ref idx);
                        if(type == TypeID.Function)
                        {
                            FunctionType fty = new FunctionType();
                            fty.typeid = (uint)type;
                            ret.types.Add(fty);

                            uint numParams = LoadUnsignedLEB32(pb, ref idx);
                            for(uint j = 0; j < numParams; ++j)
                            {
                                FunctionType.DataOrgInfo paramInfo = new FunctionType.DataOrgInfo();
                                paramInfo.type = (TypeID)LoadUnsignedLEB32(pb, ref idx);
                                fty.paramTypes.Add(paramInfo);
                            }

                            uint numResults = LoadUnsignedLEB32(pb, ref idx);
                            for(uint j = 0; j < numResults; ++j)
                            {
                                FunctionType.DataOrgInfo resultInfo = new FunctionType.DataOrgInfo();
                                resultInfo.type = (TypeID)LoadUnsignedLEB32(pb, ref idx);
                                fty.resultTypes.Add(resultInfo);
                            }

                            //uint fixup = LoadUnsignedLEB32(pb, ref idx);
                            //idx += fixup;

                            fty.InitializeOrganization();
                        }
                        else
                        { 
                        }
                    }
                }
                else if(sectionCode == Section.ImportSec)
                { 
                }
                else if(sectionCode == Section.FunctionSec)
                {
                    uint numFunctions = LoadUnsignedLEB32(pb, ref idx);
                    for (uint i = 0; i < numFunctions; ++i)
                    {
                        Function function = new Function();
                        uint fnType = LoadUnsignedLEB32(pb, ref idx);
                        function.typeidx = fnType;
                        function.fnType = ret.types[(int)fnType];

                        ret.functions.Add(function);
                    }
                }
                else if(sectionCode == Section.TableSec)
                { 
                }
                else if(sectionCode == Section.MemorySec)
                {
                    // Prepare the declaration of memory regions.
                    //
                    // Note that this is only prepping for the data payloads, actual
                    // parsing of that data happens in the Data section.
                    uint numMems = LoadUnsignedLEB32(pb, ref idx); // Right now this is assumed to be 1
                    
                    for (uint i = 0; i < numMems; ++i)
                    {
                        Memory newMem = new Memory();

                        newMem.flags = pb[idx];
                        ++idx;

                        // This should be the initial size, but I'm assuming it's also the minimum size.
                        newMem.minSize = LoadUnsignedLEB32(pb, ref idx); 
                        newMem.memory = new byte [newMem.minSize * Memory.PageSize];
                        fixed(byte * pmem = newMem.memory)
                        { 
                            newMem.pmemory = pmem;
                        }

                        // Zero out the memory (the spec says so)
                        for(int j = 0; j < newMem.memory.Length; ++j)
                            newMem.memory[j] = 0;

                        newMem.maxSize = LoadUnsignedLEB32(pb, ref idx);


                        ret.memories.Add(newMem);
                    }
                }
                else if(sectionCode == Section.GlobalSec)
                { 
                }
                else if(sectionCode == Section.ExportSec)
                { 
                    uint numExports = LoadUnsignedLEB32(pb, ref idx);
                    for(uint i = 0; i < numExports; ++i)
                    { 
                        uint strLen = LoadUnsignedLEB32(pb, ref idx);
                        string name = LoadString(pb, strLen, ref idx);
                        uint kind = LoadUnsignedLEB32(pb, ref idx);
                        uint index = LoadUnsignedLEB32(pb, ref idx);

                        Export export   = new Export();
                        export.name     = name;
                        export.kind     = kind;
                        export.index    = index;
                        ret.exports.Add(export);
                    }
                }
                else if(sectionCode == Section.StartSec)
                {
                    ret.startFnIndex = LoadUnsignedLEB32(pb, ref idx);
                }
                else if(sectionCode == Section.ElementSec) 
                { 
                }
                else if(sectionCode == Section.CodeSec)
                { 
                    uint numFunctions = LoadUnsignedLEB32(pb, ref idx);
                    for(uint i = 0; i < numFunctions; ++i)
                    {
                        Function function = ret.functions[(int)i];

                        uint bodySize = LoadUnsignedLEB32(pb, ref idx);
                        uint end = idx + bodySize;

                        uint localsCount = LoadUnsignedLEB32(pb, ref idx); 
                        for(int j = 0; j < localsCount; ++j)
                        { 
                            // The number of consecutive occurences of this type
                            uint localTyCt  = LoadUnsignedLEB32(pb, ref idx);
                            // The type to place on the stack. The quantity of how many
                            // is specified in localTyCt.
                            uint type = LoadUnsignedLEB32(pb, ref idx);

                            for(int k = 0; k < localTyCt; ++k)
                            {
                                FunctionType.DataOrgInfo doi = new FunctionType.DataOrgInfo();
                                doi.type = (Module.TypeID)type;
                                function.localTypes.Add(doi);
                            }
                        }
                        function.InitializeOrganization();

                        uint size = end - idx;
                        function.expression = new byte[size];

                        System.Runtime.InteropServices.Marshal.Copy(
                            (System.IntPtr)(int*)(&pb[idx]), 
                            function.expression, 
                            (int)0, 
                            (int)size);


                        ret.functions.Add(function);

                        idx = end;

                        //uint fixup = LoadUnsignedLEB32(pb, ref idx);
                        //idx += fixup;
                    }

                    for (uint i = 0; i < numFunctions; ++i)
                        ret.functions[(int)i].ExpandExpressionToBeUsable(ret, (int)i);
                }
                else if(sectionCode == Section.DataSec)
                {
                    uint numData = LoadUnsignedLEB32(pb, ref idx);

                    // This check might not be correct - especially if other things are putting data
                    // in this section instead of just the memory.
                    if(numData != ret.memories.Count)
                        throw new System.Exception();   // TODO: Error msg

                    for(uint i = 0; i < numData; ++i)
                    { 
                        // TODO: Figure out header
                        byte segHeaderFlags = pb[idx];
                        ++idx;

                        // TODO: What are these types for?
                        List<byte> types = new List<byte>();
                        while(pb[idx] != 0x0b)
                        { 
                            types.Add(pb[idx]);
                            ++idx;
                        }
                        ++idx;

                        Memory mem = ret.memories[(int)i];

                        uint dataSz = LoadUnsignedLEB32(pb, ref idx);
                        if(mem.memory.Length < dataSz)
                            throw new System.Exception();   // TODO: Error msg

                        // Copy into runtime memory block.
                        //
                        // We're going to do the copy manually, but if there's a C#
                        // low-level copy function that also does this, that would be 
                        // prefered.
                        for(uint j = 0; j < dataSz; ++j)
                            mem.memory[(int)j] = pb[idx + j];

                        idx += dataSz;
                    }
                }
                else
                { 
                }
            }
            ++idx;

            // TODO: Decide when to run the start function (if specified)
            // TODO: Check if start function is nullary (no parameters, no return values)

            return ret;
        }

        unsafe static public uint LoadUnsignedLEB32(byte * pb, ref uint idx)
        { 
            uint ret = 0;

            const uint mask = (1 << 8) - 1;
            const uint flag = (1 << 8);

            int shift = 0;
            for(int i = 0; i < 4; ++i)
            { 
                uint u = pb[idx];
                uint mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                ++idx;

                if((u & flag) == 0)
                    break;
            }
            return ret;

        }

        unsafe static public int LoadSignedLEB32(byte * pb, ref uint idx)
        {
            // https://en.wikipedia.org/wiki/LEB128

            int ret = -1;

            const int mask = (1 << 8) - 1;
            const int flag = (1 << 8);

            int shift = 0;
            for(int i = 0; i < 4; ++i)
            {
                int u = pb[idx];
                int mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                ++idx;

                if ((u & flag) == 0)
                    break;
            }

            if((shift < 32) && (ret & shift) != 0)
            { 
                // Sign extend for negative
                ret |= (~0 << shift);
            }
            return ret;
        }

        unsafe static public ulong LoadUnsignedLEB64(byte* pb, ref uint idx)
        {
            uint ret = 0;

            const uint mask = (1 << 8) - 1;
            const uint flag = (1 << 8);

            int shift = 0;
            for (int i = 0; i < 8; ++i)
            {
                uint u = pb[idx];
                uint mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                ++idx;

                if ((u & flag) == 0)
                    break;
            }
            return ret;

        }

        unsafe static public long LoadSignedLEB64(byte* pb, ref uint idx)
        {
            // https://en.wikipedia.org/wiki/LEB128

            int ret = -1;

            const int mask = (1 << 8) - 1;
            const int flag = (1 << 8);

            int shift = 0;
            for (int i = 0; i < 8; ++i)
            {
                int u = pb[idx];
                int mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                ++idx;

                if ((u & flag) == 0)
                    break;
            }

            if ((shift < 64) && (ret & shift) != 0)
            {
                // Sign extend for negative
                ret |= (~0 << shift);
            }
            return ret;
        }

        unsafe static string LoadString(byte * pb, uint len, ref uint idx)
        { 
            byte [] rb = new byte[len];
            for(int i = 0; i < len; ++i)
                rb[i] = pb[idx + i];

            idx += len;

            return System.Text.Encoding.UTF8.GetString(rb);
        }

        public int GetExportedFunctionID(string fnName)
        {
            foreach(Export e in this.exports)
            { 
                if(e.name == fnName)
                    return (int)e.index;
            }
            return -1;
        }

        public int GetExportedFunctionID(string fnName, out FunctionType fnty)
        {
            foreach (Export e in this.exports)
            {
                if (e.name == fnName)
                {
                    fnty = this.types[(int)this.functions[(int)e.index].typeidx];
                    return (int)e.index;
                }
            }
            fnty = null;
            return -1;
        }

        //public static Session LoadString(string str)
        //{ }
    }
}