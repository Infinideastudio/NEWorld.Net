// OpenGL: Init.cs
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
using System.Runtime.InteropServices;

namespace OpenGL
{
    static partial class Gl
    {
        public delegate IntPtr GetProcAddressProc(string proc);

        private static GetProcAddressProc _getProcAddress;

        private static TDelegateType Get<TDelegateType>(string name) =>
            Marshal.GetDelegateForFunctionPointer<TDelegateType>(_getProcAddress(name));

        static partial void InitVertexArray();
        static partial void InitShader();
        static partial void InitRenderBuffer();
        static partial void InitDataBuffer();
        static partial void InitOthers();
        static partial void InitTexture();
        static partial void InitFrameBuffer();

        public static void Init(GetProcAddressProc getProcAddressProc)
        {
            _getProcAddress = getProcAddressProc;
            InitVertexArray();
            InitShader();
            InitRenderBuffer();
            InitDataBuffer();
            InitTexture();
            InitFrameBuffer();
            InitOthers();
        }

    }
}