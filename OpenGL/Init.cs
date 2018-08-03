// 
// OpenGL: Init.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2018 NEWorld Team
// 
// NEWorld is free software: you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// NEWorld is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General 
// Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with NEWorld.  If not, see <http://www.gnu.org/licenses/>.
// 

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