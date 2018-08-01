// OpenGL: Basics.cs
// Graphics.Net: General Application Framework API and GUI For .Net
// Copyright (C) 2015-2018 NEWorld Team
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text;

namespace OpenGL
{
    public static partial class Gl
    {
        public const uint Quads = 0x0007;
        public const uint Byte = 0x1400;
        public const uint UnsignedByte = 0x1401;
        public const uint Short = 0x1402;
        public const uint UnsignedShort = 0x1403;
        public const uint Int = 0x1404;
        public const uint UnsignedInt = 0x1405;
        public const uint Float = 0x1406;
        public const uint Double = 0x140A;
        public const uint Lines = 0x0001;
        public const uint LineLoop = 0x0002;
        public const uint LineStrip = 0x0003;
        public const uint Triangles = 0x0004;
        public const uint TriangleStrip = 0x0005;
        public const uint TriangleFan = 0x0006;
        public const uint ColorBufferBit = 0x4000;
        public const uint DepthBufferBit = 0x0100;
        public const uint StencilBufferBit = 0x0400;
        public const uint Blend = 0x0BE2;
        public const uint SrcAlpha = 0x0302;
        public const uint OneMinusSrcAlpha = 0x0303;
        public const uint DstAlpha = 0x0304;
        public const uint OneMinusDstAlpha = 0x0305;
        public const uint FuncAdd = 0x8006;
        public const uint CullFace = 0x0B44;
        public const uint DepthTest = 0x0B71;
        public const uint ScissorTest = 0x0C11;
        public const uint FrontLeft = 0x0400;
        public const uint FrontRight = 0x0401;
        public const uint BackLeft = 0x0402;
        public const uint BackRight = 0x0403;
        public const uint Front = 0x0404;
        public const uint Back = 0x0405;
        public const uint Left = 0x0406;
        public const uint Right = 0x0407;
        public const uint FrontAndBack = 0x0408;
        public const uint Less = 0x0201;
        public const uint Equal = 0x0202;
        public const uint Lequal = 0x0203;
        public const uint Greater = 0x0204;
        public const uint Notequal = 0x0205;
        public const uint Gequal = 0x0206;
        public const uint Always = 0x0207;

        public static byte[] Utf8ToNative(string s) => s == null ? null : Encoding.UTF8.GetBytes(s + "\0");

        public static unsafe string Utf8ToManaged(IntPtr s)
        {
            var sBase = (byte*) s;
            if (sBase == null)
                return null;
            var numPtr = sBase;
            while (*numPtr != 0)
                ++numPtr;
            var num = (int) (numPtr - sBase);
            char* chars1 = stackalloc char[num];
            var chars2 = Encoding.UTF8.GetChars(sBase, num, chars1, num);
            return new string(chars1, 0, chars2);
        }
    }
}