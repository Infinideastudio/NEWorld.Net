// 
// OpenGL: FrameBuffer.cs
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

using Core.Utilities;

namespace OpenGL
{
    public static partial class Gl
    {
        public const uint FrameBuffer = 0x8D40;
        public const uint ColorAttachment = 0x8CE0;
        public const uint StencilAttachment = 0x8D20;

        internal unsafe delegate void CreateFramebuffersProc(int n, uint* framebuffers);

        internal unsafe delegate void DeleteFramebuffersProc(int n, uint* framebuffers);

        internal delegate void BindFramebufferProc(uint target, uint texture);

        internal delegate void NamedFramebufferTextureProc(uint framebuffer, uint attachment, uint texture, int level);

        internal delegate void NamedFramebufferRenderbufferProc(uint framebuffer, uint attachment,
            uint renderbuffertarget, uint renderbuffer);

        internal delegate void ClearNamedFramebufferfvProc(uint fBuffer, uint buffer, int drawbuffer, float[] value);

        internal static CreateFramebuffersProc CreateFramebuffers;
        internal static DeleteFramebuffersProc DeleteFramebuffers;
        internal static BindFramebufferProc BindFramebuffer;
        internal static NamedFramebufferTextureProc NamedFramebufferTexture;
        internal static NamedFramebufferRenderbufferProc NamedFramebufferRenderbuffer;
        internal static ClearNamedFramebufferfvProc ClearNamedFramebufferfv;

        static partial void InitFrameBuffer()
        {
            CreateFramebuffers = Get<CreateFramebuffersProc>("glCreateFramebuffers");
            DeleteFramebuffers = Get<DeleteFramebuffersProc>("glDeleteFramebuffers");
            BindFramebuffer = Get<BindFramebufferProc>("glBindFramebuffer");
            NamedFramebufferTexture = Get<NamedFramebufferTextureProc>("glNamedFramebufferTexture");
            NamedFramebufferRenderbuffer = Get<NamedFramebufferRenderbufferProc>("glNamedFramebufferRenderbuffer");
            ClearNamedFramebufferfv = Get<ClearNamedFramebufferfvProc>("glClearNamedFramebufferfv");
        }
    }

    public class FrameBuffer : StrictDispose
    {
        public unsafe FrameBuffer()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.CreateFramebuffers(1, addr);
            }
        }

        protected override unsafe void Release()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.DeleteFramebuffers(1, addr);
            }
        }

        public static void UseDefault() => Gl.BindFramebuffer(Gl.FrameBuffer, 0);

        public void Use() => Gl.BindFramebuffer(Gl.FrameBuffer, _hdc);

        public void Texture(uint attachment, Texture texture, int level) =>
            Gl.NamedFramebufferTexture(_hdc, attachment, texture?.Raw() ?? 0, level);

        public void RenderBuffer(uint attachment, RenderBuffer buffer) =>
            Gl.NamedFramebufferRenderbuffer(_hdc, attachment, Gl.RenderBuffer, buffer?.Raw() ?? 0);

        public void ClearColor(int index, float[] color) => Gl.ClearNamedFramebufferfv(_hdc, 0, index, color);

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}