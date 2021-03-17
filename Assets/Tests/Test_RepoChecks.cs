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
    /// <summary>
    /// Repo management utilities and checks.
    /// </summary>
    public class Test_RepoChecks
    {
        [Test]
        public void Test_NoUnityReferences()
        { 
            // Tests to make sure nothing in PreWASM's core source code
            // contains explicit references to Unity.

            string rootDirectory = "Assets/PrecisionWASM_Core";

            Queue<System.IO.DirectoryInfo> toScan = new Queue<System.IO.DirectoryInfo>();

            System.IO.DirectoryInfo diRoot = new System.IO.DirectoryInfo(rootDirectory);
            if(diRoot.Exists == false)
                throw new System.Exception("Could not locate existing PreWasm directory.");

            toScan.Enqueue(diRoot);

            while(toScan.Count > 0)
            { 
                System.IO.DirectoryInfo di = toScan.Dequeue();

                foreach(System.IO.DirectoryInfo childDir in di.GetDirectories())
                    toScan.Enqueue(childDir);

                foreach(System.IO.FileInfo fi in di.GetFiles("*.cs"))
                { 
                    string fileText = System.IO.File.ReadAllText(fi.FullName);
                    if(fileText.Contains("UnityEngine") == true || fileText.Contains("UnityEditor") == true)
                        throw new System.Exception($"Detected reference to Unity namespace in {fi.FullName}");
                }
            }
        }
    }
}
