﻿// MIT License
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

namespace Tests
{
    public struct FloatIntPair
    {
        public float f;
        public int n;

        public FloatIntPair(float f, int n)
        {
            this.f = f;
            this.n = n;
        }
    }

    public struct IntPair
    {
        public int a;
        public int b;

        public IntPair(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct UIntPair
    {
        public uint a;
        public uint b;

        public UIntPair(uint a, uint b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct FloatPair
    {
        public float a;
        public float b;

        public FloatPair(float a, float b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct Int64Pair
    {
        public long a;
        public long b;

        public Int64Pair(long a, long b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct UInt64Pair
    {
        public ulong a;
        public ulong b;

        public UInt64Pair(ulong a, ulong b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct Float64Pair
    {
        public double a;
        public double b;

        public Float64Pair(double a, double b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct IntTripplet
    {
        public int a;
        public int b;
        public int c;

        public IntTripplet(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public struct FloatTrippplet
    { 
        public float a;
        public float b;
        public float c;

        public FloatTrippplet(float a, float b, float c)
        { 
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public struct DoubleTripplet
    { 
        public double a;
        public double b;
        public double c;

        public DoubleTripplet(double a, double b, double c)
        { 
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public struct LongTripplet
    {
        public long a;
        public long b;
        public long c;

        public LongTripplet(long a, long b, long c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public struct MixedPair<tya, tyb>
    { 
        public tya a;
        public tyb b;

        public MixedPair(tya a, tyb b)
        { 
            this.a = a;
            this.b = b;
        }
    }
}