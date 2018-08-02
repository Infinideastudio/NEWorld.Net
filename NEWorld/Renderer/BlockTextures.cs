using System;
using System.Collections.Generic;
using Core;
using Core.Math;
using Core.Utilities;
using Game.Terrain;
using OpenGL;
using SDL2;

namespace NEWorld.Renderer
{
    [DeclareService("BlockTextures")]
    public class BlockTextures : IBlockTextures, IDisposable
    {
        private unsafe class RawTexture
        {       
            public RawTexture(string filename) => Surface = (SDL.SDL_Surface*) SDL_image.IMG_Load(filename);

            ~RawTexture() => SDL.SDL_FreeSurface((IntPtr) Surface);

            public SDL.SDL_Surface* Surface { get; }
        }

        private BlockTextures()
        {
            SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
        }
        
        public void Dispose()
        {
            SDL_image.IMG_Quit();
        }
        
        private static int Capacity()
        {
            var w = CapacityRaw() / _pixelPerTexture;
            return w * w;
        }

        private static int CapacityRaw()
        {
            int cap = 2048;
            //glGetIntegerv(GL_MAX_TEXTURE_SIZE, &cap);
            return cap;
        }

        public static void SetWidthPerTex(int wid) => _pixelPerTexture = wid;

        public static int GetWidthPerTex() => _pixelPerTexture;

        public static unsafe Texture BuildAndFlush()
        {
            var count = RawTexs.Count;
            _texturePerLine = 1 << (int) Math.Ceiling(Math.Log(Math.Ceiling(Math.Sqrt(count))) / Math.Log(2));
            var wid = _texturePerLine * _pixelPerTexture;

            var mask = EndianCheck.BigEndian
                ? new uint[] {0xff000000, 0x00ff0000, 0x0000ff00, 0x000000ff}
                : new uint[] {0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000};

            var s = (SDL.SDL_Surface*) SDL.SDL_CreateRGBSurface(0, wid, wid, 32, mask[0], mask[1], mask[2], mask[3]);
            for (var i = 0; i < count; ++i)
            {
                var x = i % _texturePerLine;
                var y = i / _texturePerLine;
                SDL.SDL_Rect r;
                r.x = x * _pixelPerTexture;
                r.y = y * _pixelPerTexture;
                r.w = r.h = _pixelPerTexture;
                SDL.SDL_BlitScaled((IntPtr) RawTexs[i].Surface, IntPtr.Zero, (IntPtr) s, (IntPtr) (&r));
            }

            RawTexs.Clear();
            var levels = (int) (Math.Log(_pixelPerTexture) / Math.Log(2));
            var ret = new Texture(levels, PixelInternalFormats.Rgba8, new Vec2<int>(wid, wid))
            {
                MinifyingFilter = Texture.Filter.NearestMipmapNearest,
                MagnificationFilter = Texture.Filter.Nearest
            };
            Build2DMipmaps(ret, wid, wid, (int) (Math.Log(_pixelPerTexture) / Math.Log(2)), (byte*) s->pixels);
            return ret;
        }

        private static int Align(int x, int al)
        {
            return x % al == 0 ? x : (x / al + 1) * al;
        }

        private static unsafe void Build2DMipmaps(Texture tex, int w, int h, int level, byte* src)
        {
            var scale = 1;
            var cur = new byte[w * h * 4];
            for (var i = 0; i <= level; i++)
            {
                int curW = w / scale, curH = h / scale;
                for (var y = 0; y < curH; y++)
                for (var x = 0; x < curW; x++)
                for (var col = 0; col < 4; col++)
                {
                    var sum = 0;
                    for (var yy = 0; yy < scale; yy++)
                    for (var xx = 0; xx < scale; xx++)
                        sum += src[(y * scale + yy) * Align(w * 4, 4) + (x * scale + xx) * 4 + col];
                    cur[y * Align(curW * 4, 4) + x * 4 + col] = (byte) (sum / (scale * scale));
                }

                tex.Image(i, new Rect<int>(0, 0, curW, curH), PixelTypes.Rgba, PixelDataFormats.Byte, cur);
                scale *= 2;
            }
        }

        public uint Add(string path)
        {
            if (RawTexs.Count >= Capacity())
                throw new Exception("Too Many Textures");
            RawTexs.Add(new RawTexture(path));
            return (uint) (RawTexs.Count - 1);
        }

        public unsafe void GetTexturePos(float* pos, uint id)
        {
            var percentagePerTexture = 1.0f / _texturePerLine;
            var x = id % _texturePerLine;
            var y = id / _texturePerLine;
            pos[0] = percentagePerTexture * x;
            pos[1] = percentagePerTexture * y;
            pos[2] = percentagePerTexture * (x + 1);
            pos[3] = percentagePerTexture * (y + 1);
        }

        private static int _pixelPerTexture = 32, _texturePerLine = 8;
        private static readonly List<RawTexture> RawTexs = new List<RawTexture>();
    }
}