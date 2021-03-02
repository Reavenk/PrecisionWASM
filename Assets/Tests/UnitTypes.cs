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
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

namespace Tests
{
    public struct FloatIntPair
    {
        public float left;
        public int right;

        public FloatIntPair(float left, int right)
        {
            this.left = left;
            this.right = right;
        }
    }

    public struct IntPair
    {
        public int left;
        public int right;

        public IntPair(int left, int right)
        {
            this.left = left;
            this.right = right;
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
}