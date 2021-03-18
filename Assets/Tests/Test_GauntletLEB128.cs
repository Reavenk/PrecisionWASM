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
using NUnit.Framework;

namespace Tests
{
    public class Test_GauntletLEB128
    {
        // https://en.wikipedia.org/wiki/LEB128

        //      3c = > 60
        //      d804 => 600
        //      f02e => 6000
        //      e0d4 03 => 60000
        //      c0cf 24
        //      809b ee02

        public struct TestKeyUInt
        { 
            public uint val;
            public byte [] pattern;

            public TestKeyUInt(uint val, params byte [] pattern)
            { 
                this.val = val;
                this.pattern = pattern;
            }
        }

        public struct TestKeySInt
        {
            public int val;
            public byte[] pattern;

            public TestKeySInt(int val, params byte[] pattern)
            {
                this.val = val;
                this.pattern = pattern;
            }
        }

        public struct TestKeyUInt64
        {
            public ulong val;
            public byte[] pattern;

            public TestKeyUInt64(ulong val, params byte[] pattern)
            {
                this.val = val;
                this.pattern = pattern;
            }
        }

        public struct TestKeySInt64
        {
            public long val;
            public byte[] pattern;

            public TestKeySInt64(long val, params byte[] pattern)
            {
                this.val = val;
                this.pattern = pattern;
            }
        }

        static TestKeyUInt [] key32u = 
            new TestKeyUInt[]
            { 
                new TestKeyUInt(60,         0x3c),
                new TestKeyUInt(600,        0xd8, 0x04),
                new TestKeyUInt(6000,       0xf0, 0x2e),
                new TestKeyUInt(60000,      0xe0, 0xd4, 0x03),
                new TestKeyUInt(600000,     0xc0, 0xcf, 0x24),
                new TestKeyUInt(6000000,    0x80, 0x9b, 0xee, 0x02)
            };

        static TestKeySInt[] key32s =
            new TestKeySInt[]
            {
                new TestKeySInt(-60,        0x44),
                new TestKeySInt(-600,       0xa8, 0x7b),
                new TestKeySInt(-6000,      0x90, 0x51),
                new TestKeySInt(-60000,     0xa0, 0xab, 0x7c),
                new TestKeySInt(-600000,    0xc0, 0xb0, 0x5b),
                new TestKeySInt(-6000000,   0x80, 0xe5, 0x91, 0x7d)
            };

        // For now, we have a duplicate set of testing keys for 64bit,
        // but we'll go through the effort of duplicating them for 
        // correct typeness.

        static TestKeyUInt64[] key64u =
            new TestKeyUInt64[]
            {
                new TestKeyUInt64(60,         0x3c),
                new TestKeyUInt64(600,        0xd8, 0x04),
                new TestKeyUInt64(6000,       0xf0, 0x2e),
                new TestKeyUInt64(60000,      0xe0, 0xd4, 0x03),
                new TestKeyUInt64(600000,     0xc0, 0xcf, 0x24),
                new TestKeyUInt64(6000000,    0x80, 0x9b, 0xee, 0x02)
            };

        static TestKeySInt64[] key64s =
            new TestKeySInt64[]
            {
                new TestKeySInt64(-60,        0x44),
                new TestKeySInt64(-600,       0xa8, 0x7b),
                new TestKeySInt64(-6000,      0x90, 0x51),
                new TestKeySInt64(-60000,     0xa0, 0xab, 0x7c),
                new TestKeySInt64(-600000,    0xc0, 0xb0, 0x5b),
                new TestKeySInt64(-6000000,   0x80, 0xe5, 0x91, 0x7d)
            };

        public static bool CompareByteArrays(byte [] ra, byte [] rb)
        { 
            if(ra.Length != rb.Length)
                return false;

            for(int i = 0; i < ra.Length; ++i)
            { 
                if(ra[i] != rb[i])
                    return false;
            }

            return true;
        }

        [Test]
        unsafe public void Test_LEB32U()
        { 
            foreach(TestKeyUInt tku in key32u)
            { 
                fixed(byte * pb = tku.pattern)
                {
                    uint idx = 0;
                    uint res = PxPre.WASM.BinParse.LoadUnsignedLEB32(pb, ref idx);

                    if(res != tku.val)
                        throw new System.Exception($"Invalid LEB32U decoding of value {tku.val}.");
                }
            }

            foreach(TestKeyUInt tku in key32u)
            { 
                byte [] rb = PxPre.WASM.BinParse.EncodeUnsignedLEB(tku.val).ToArray();

                if(CompareByteArrays(rb, tku.pattern) == false)
                    throw new System.Exception($"Invalid LEB32U encoding of value {tku.val}.");
            }
        }

        [Test]
        unsafe public void Test_LEB32S()
        {
            // POSITIVE ENCODING AND DECODING 
            foreach (TestKeyUInt tku in key32u)
            {
                fixed (byte* pb = tku.pattern)
                {
                    uint idx = 0;
                    int res = PxPre.WASM.BinParse.LoadSignedLEB32(pb, ref idx);

                    if (res != tku.val)
                        throw new System.Exception($"Invalid LEB32S decoding of value {tku.val}.");
                }
            }

            foreach (TestKeyUInt tku in key32u)
            {
                byte[] rb = PxPre.WASM.BinParse.EncodeSignedLEB(tku.val).ToArray();

                if (CompareByteArrays(rb, tku.pattern) == false)
                    throw new System.Exception($"Invalid LEB32S encoding of value {tku.val}.");
            }

            // NEGATIVE ENCODING AND DECODING 

            foreach (TestKeySInt tku in key32s)
            {
                fixed (byte* pb = tku.pattern)
                {
                    uint idx = 0;
                    int res = PxPre.WASM.BinParse.LoadSignedLEB32(pb, ref idx);

                    if (res != tku.val)
                        throw new System.Exception($"Invalid LEB32S decoding of value {tku.val}.");
                }
            }

            foreach (TestKeySInt tku in key32s)
            {
                byte[] rb = PxPre.WASM.BinParse.EncodeSignedLEB(tku.val).ToArray();

                if (CompareByteArrays(rb, tku.pattern) == false)
                    throw new System.Exception($"Invalid LEB32S encoding of value {tku.val}.");
            }
        }

        [Test]
        unsafe public void Test_LEB64U()
        {
            foreach (TestKeyUInt64 tku in key64u)
            {
                fixed (byte* pb = tku.pattern)
                {
                    uint idx = 0;
                    ulong res = PxPre.WASM.BinParse.LoadUnsignedLEB64(pb, ref idx);

                    if (res != tku.val)
                        throw new System.Exception($"Invalid LEB64U decoding of value {tku.val}.");
                }
            }

            foreach (TestKeyUInt64 tku in key64u)
            {
                byte[] rb = PxPre.WASM.BinParse.EncodeUnsignedLEB(tku.val).ToArray();

                if (CompareByteArrays(rb, tku.pattern) == false)
                    throw new System.Exception($"Invalid LEB64U encoding of value {tku.val}.");
            }
        }

        [Test]
        unsafe public void Test_LEB64S()
        {
            // POSITIVE ENCODING AND DECODING 
            foreach (TestKeyUInt64 tku in key64u)
            {
                fixed (byte* pb = tku.pattern)
                {
                    uint idx = 0;
                    long res = PxPre.WASM.BinParse.LoadSignedLEB64(pb, ref idx);

                    if (res != (long)tku.val)
                        throw new System.Exception($"Invalid LEB64S decoding of value {tku.val}.");
                }
            }

            foreach (TestKeyUInt64 tku in key64u)
            {
                byte[] rb = PxPre.WASM.BinParse.EncodeSignedLEB((long)tku.val).ToArray();

                if (CompareByteArrays(rb, tku.pattern) == false)
                    throw new System.Exception($"Invalid LEB64S encoding of value {tku.val}.");
            }

            // NEGATIVE ENCODING AND DECODING 

            foreach (TestKeySInt64 tku in key64s)
            {
                fixed (byte* pb = tku.pattern)
                {
                    uint idx = 0;
                    long res = PxPre.WASM.BinParse.LoadSignedLEB64(pb, ref idx);

                    if (res != tku.val)
                        throw new System.Exception($"Invalid LEB64S decoding of value {tku.val}.");
                }
            }

            foreach (TestKeySInt64 tku in key64s)
            {
                byte[] rb = PxPre.WASM.BinParse.EncodeSignedLEB(tku.val).ToArray();

                if (CompareByteArrays(rb, tku.pattern) == false)
                    throw new System.Exception($"Invalid LEB64S encoding of value {tku.val}.");
            }
        }
    }
}