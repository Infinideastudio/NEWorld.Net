// 
// OpenGL: RenderBuffer.cs
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

using Core.Math;
using Core.Utilities;

namespace OpenGL
{
    public static partial class Gl
    {
        public const uint RenderBuffer = 0x8D41;

        internal unsafe delegate void CreateRenderbuffersProc(int n, uint* renderbuffers);

        internal unsafe delegate void DeleteRenderbuffersProc(int n, uint* renderbuffers);

        internal delegate void NamedRenderbufferStorageProc(uint renderbuffer, uint format, int width, int height);

        internal static CreateRenderbuffersProc CreateRenderbuffers;
        internal static DeleteRenderbuffersProc DeleteRenderbuffers;
        internal static NamedRenderbufferStorageProc NamedRenderbufferStorage;

        static partial void InitRenderBuffer()
        {
            CreateRenderbuffers = Get<CreateRenderbuffersProc>("glCreateRenderbuffers");
            DeleteRenderbuffers = Get<DeleteRenderbuffersProc>("glDeleteRenderbuffers");
            NamedRenderbufferStorage = Get<NamedRenderbufferStorageProc>("glNamedRenderbufferStorage");
        }
    }

    public class RenderBuffer : StrictDispose
    {
        public unsafe RenderBuffer()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.CreateRenderbuffers(1, addr);
            }
        }

        protected override unsafe void Release()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.DeleteRenderbuffers(1, addr);
            }
        }

        public void SetStorage(PixelInternalFormats fmt, Vec2<int> size) =>
            Gl.NamedRenderbufferStorage(_hdc, (uint) fmt, size.X, size.Y);

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}