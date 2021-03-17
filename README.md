# Precision WASM

A WebAssembly virtual machine written in C#.

The project is still a work in progress and is not meant for general production.

## Introduction

I needed a program representation for another personal project, so I decided to 
tackling making a WASM interpreter.

The interpreter is designed to be explicitly low level, which required unsafe code -
and for other projects, it's designed to compile in Unity for PC, Android and WebGL,
meaning it can only use the version of C# limited by what Unity supports.

The interpreter works by taking the bytecode, and translating it to a similar bytecode that
leverages information resolved in the validation process, and that's more suitable for 
execution.

## Environment

The library is not dependent on Unity, but it is in a Unity project for development
and unit testing. 

It's also helpful to keep the development in a Unity project so it can be built to make
sure it's supported by the target platforms.

The testing also uses a variant library called Datum that isn't required, but
makes data management easier with a utility library (WASMDatum).

## License

MIT License

Copyright (c) 2021 Pixel Precision, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.