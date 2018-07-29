// OpenGL: Texture.cs
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

using Core;

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
            int height, uint format, uint type, byte[] pixels);

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

        public void Image(int level, Rect<int> area, PixelTypes format, PixelDataFormats type, byte[] data) =>
            Gl.TextureSubImage2D(_hdc, level, area.Min.X, area.Max.Y, area.Delta.X, area.Delta.Y, (uint) format,
                (uint) type, data);

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}