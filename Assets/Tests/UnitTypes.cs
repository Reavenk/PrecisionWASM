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
}