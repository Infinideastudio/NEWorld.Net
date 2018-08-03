// 
// OpenGL: Texture.cs
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
using Core;
using Core.Math;
using Core.Utilities;

namespace OpenGL
{
    public static partial class Gl
    {
        internal const uint Texture2D = 0x0DE1;

        internal unsafe delegate void CreateTexturesProc(uint target, int n, uint* textures);

        internal unsafe delegate void DeleteTexturesProc(int n, uint* textures);

        internal delegate void TextureParameteriProc(uint texture, uint pname, int param);

        internal delegate void TextureParameterfProc(uint texture, uint pname, float param);

        internal delegate void BindTextureUnitProc(uint unit, uint texture);

        internal delegate void TextureStorage2DProc(uint texture, int levels, uint format, int width, int height);

        internal delegate void TextureSubImage2DProc(uint texture, int level, int xoffset, int yoffset, int width,
            int height, uint format, uint type, IntPtr pixels);

        internal static CreateTexturesProc CreateTextures;
        internal static DeleteTexturesProc DeleteTextures;
        internal static TextureParameteriProc TextureParameteri;
        internal static TextureParameterfProc TextureParameterf;
        internal static BindTextureUnitProc BindTextureUnit;
        internal static TextureStorage2DProc TextureStorage2D;
        internal static TextureSubImage2DProc TextureSubImage2D;

        static partial void InitTexture()
        {
            CreateTextures = Get<CreateTexturesProc>("glCreateTextures");
            DeleteTextures = Get<DeleteTexturesProc>("glDeleteTextures");
            TextureParameteri = Get<TextureParameteriProc>("glTextureParameteri");
            TextureParameterf = Get<TextureParameterfProc>("glTextureParameterf");
            BindTextureUnit = Get<BindTextureUnitProc>("glBindTextureUnit");
            TextureStorage2D = Get<TextureStorage2DProc>("glTextureStorage2D");
            TextureSubImage2D = Get<TextureSubImage2DProc>("glTextureSubImage2D");
        }
    }

    public class Texture : StrictDispose
    {
        public unsafe Texture(int levels, PixelInternalFormats internalFormat, Vec2<int> size)
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.CreateTextures(Gl.Texture2D, 1, addr);
            }

            Gl.TextureStorage2D(_hdc, levels, (uint) internalFormat, size.X, size.Y);
        }

        protected override unsafe void Release()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.DeleteTextures(1, addr);
            }
        }

        public enum Filter
        {
            Nearest = 0x2600,
            Linear = 0x2601,
            NearestMipmapNearest = 0x2700,
            LinearMipmapNearest = 0x2701,
            NearestMipmapLinear = 0x2702,
            LinearMipmapLinear = 0x2703
        }

        public enum Warp
        {
            ClampToEdge = 0x812F,
            ClampToBorder = 0x812D,
            MirroredRepeat = 0x8370,
            Repeat = 0x2901,
            MirrorClampToEdge = 0x8743
        }

        private void SetParameter(uint name, int param) => Gl.TextureParameteri(_hdc, name, param);

        private void SetParameter(uint name, float param) => Gl.TextureParameterf(_hdc, name, param);

        public Filter MinifyingFilter
        {
            set => SetParameter(0x2801, (int) value);
        }

        public Filter MagnificationFilter
        {
            set => SetParameter(0x2800, (int) value);
        }

        public Warp WarpS
        {
            set => SetParameter(0x2802, (int) value);
        }

        public Warp WarpT
        {
            set => SetParameter(0x2803, (int) value);
        }

        public void Use(uint slot) => Gl.BindTextureUnit(slot, _hdc);

        public static void UseRaw(uint slot, uint handle) => Gl.BindTextureUnit(slot, handle);

        public unsafe void Image(int level, Rect<int> area, PixelTypes format, PixelDataFormats type, byte[] data)
        {
            fixed (byte* ptr = &data[0])
            {
                Gl.TextureSubImage2D(_hdc, level, area.Min.X, area.Min.Y, area.Delta.X, area.Delta.Y, (uint) format,
                    (uint) type, (IntPtr) ptr);
            }
        }

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}