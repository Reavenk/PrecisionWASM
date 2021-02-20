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
        /// <summary>
        /// Implement LEB128 decoding for unsigned 32bit unsigned integers.
        /// https://en.wikipedia.org/wiki/LEB128
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        unsafe static public uint LoadUnsignedLEB32(byte* pb, ref uint idx)
        {
            uint ret = 0;

            const uint mask = (1 << 8) - 1;
            const uint flag = (1 << 8);

            int shift = 0;
            for (int i = 0; i < 4; ++i)
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

        /// <summary>
        /// Implement LEB128 decoding for unsigned 32bit signed integers.
        /// https://en.wikipedia.org/wiki/LEB128
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        unsafe static public int LoadSignedLEB32(byte* pb, ref uint idx)
        {
            // https://en.wikipedia.org/wiki/LEB128

            int ret = -1;

            const int mask = (1 << 8) - 1;
            const int flag = (1 << 8);

            int shift = 0;
            for (int i = 0; i < 4; ++i)
            {
                int u = pb[idx];
                int mag = u & mask;

                ret |= mag << shift;
                shift += 7;

                ++idx;

                if ((u & flag) == 0)
                    break;
            }

            if ((shift < 32) && (ret & shift) != 0)
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
    }
}