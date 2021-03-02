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