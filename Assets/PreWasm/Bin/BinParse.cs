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
    public static class BinParse
    {
        // Some references of parsing WASMs (and WATs)
        // https://developer.mozilla.org/en-US/docs/WebAssembly/Understanding_the_text_format
        // https://webassembly.github.io/spec/core/_download/WebAssembly.pdf

        public const int WASM_BINARY_MAGIC = 0x6d736100;
        public const int WASM_BINARY_VERSION = 0x00000001;

        /// <summary>
        /// Implement LEB128 decoding for unsigned 32bit unsigned integers.
        /// https://en.wikipedia.org/wiki/LEB128
        /// </summary>
        /// <param name="pb">The source of bytes to parse.</param>
        /// <param name="idx">The byte iterator.</param>
        /// <returns>The unsigned int, starting at pb[idx] that was LEB128 decoded
        /// for 32 bit unsigned integers.</returns>
        unsafe static public uint LoadUnsignedLEB32(byte* pb, ref uint idx)
        {
            uint ret = 0;

            const uint mask = (1 << 7) - 1;
            const uint flag = (1 << 7);

            int shift = 0;
            for (int i = 0; i < 4; ++i)
            {
                uint u = pb[idx];
                ++idx;

                uint mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                if ((u & flag) == 0)
                    break;
            }
            return ret;

        }

        /// <summary>
        /// Implement LEB128 decoding for unsigned 32bit signed integers.
        /// https://en.wikipedia.org/wiki/LEB128
        /// </summary>
        /// <param name="pb">The source of bytes to parse.</param>
        /// <param name="idx">The byte iterator.</param>
        /// <returns>The unsigned int, starting at pb[idx] that was LEB128 decoded
        /// for 32 bit signed integers.</returns>
        unsafe static public int LoadSignedLEB32(byte* pb, ref uint idx)
        {
            // https://en.wikipedia.org/wiki/LEB128

            int ret = 0;

            const int mask = (1 << 7) - 1;
            const int flag = (1 << 7);

            int shift = 0;
            while(true)
            {
                int u = pb[idx];
                ++idx;

                int mag = u & mask;

                ret |= mag << shift;

                if(shift >= 32)
                    break;

                shift += 7;

                if ((u & flag) == 0)
                    break;
            }

            if ((shift < 32) && (ret & (1 << (shift-1))) != 0)
            {
                // Sign extend for negative
                ret |= (~0 << shift);
            }
            return ret;
        }

        /// <summary>
        /// Implement LEB128 decoding for unsigned 64 bit unsigned integers.
        /// https://en.wikipedia.org/wiki/LEB128
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        unsafe static public ulong LoadUnsignedLEB64(byte* pb, ref uint idx)
        {
            ulong ret = 0;

            const ulong mask = (1 << 7) - 1;
            const ulong flag = (1 << 7);

            int shift = 0;
            for (int i = 0; i < 8; ++i)
            {
                ulong u = pb[idx];
                ++idx;

                ulong mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                if ((u & flag) == 0)
                    break;
            }
            return ret;

        }

        /// <summary>
        /// Implement LEB128 decoding for unsigned 64 bit signed integers.
        /// https://en.wikipedia.org/wiki/LEB128
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        unsafe static public long LoadSignedLEB64(byte* pb, ref uint idx)
        {
            // https://en.wikipedia.org/wiki/LEB128

            long ret = 0;

            const long mask = (1 << 7) - 1;
            const long flag = (1 << 7);

            int shift = 0;
            while(true)
            {
                long u = pb[idx];
                ++idx;

                long mag = u & mask;

                ret |= mag << shift;

                if(shift >= 64)
                    break;

                shift += 7;

                if ((u & flag) == 0)
                    break;
            }

            if ((shift < 64) && (ret & (1<< (shift-1))) != 0)
            {
                // Sign extend for negative
                ret |= ~((long)0) << shift;
            }
            return ret;
        }
    }
}