// 
// GUI: blockrenderer.cpp
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Game;
using Core;
using Core.Math;
using OpenGL;
using SDL2;

namespace NEWorld.Renderer
{
    public unsafe struct BlockTexCoord
    {
        public uint Pos;
        public fixed float D[4];
    }

    public interface IBlockRenderer
    {
        void FlushTexture();
        void Render(VertexBuilder target, Chunk chunk, Vec3<int> pos);
    }

    public class VertexBuilder
    {
        public VertexBuilder(int size) => _data = Marshal.AllocHGlobal(size * sizeof(float));

        ~VertexBuilder() => Marshal.FreeHGlobal(_data);

        public void AddPrimitive(int verts, params float[] data)
        {
            VertCount += verts;
            Marshal.Copy(data, 0, _data + _size * sizeof(float), data.Length);
            _size += data.Length;
        }

        public ConstDataBuffer Dump() => VertCount > 0 ? new ConstDataBuffer(_size * sizeof(float), _data) : null;

        private int _size;
        public int VertCount;
        private readonly IntPtr _data;
    }

    public class DefaultBlockRenderer : IBlockRenderer
    {
        public DefaultBlockRenderer(uint[] data)
        {
            _tex = new BlockTexCoord[6];
            for (var i = 0; i < 6; ++i)
                _tex[i].Pos = data[i];
        }

        public unsafe void FlushTexture()
        {
            for (var i = 0; i < 6; ++i)
                fixed (float* tex = _tex[0].D)
                    BlockTextureBuilder.GetTexturePos(tex, _tex[i].Pos);
        }

        public unsafe void Render(VertexBuilder target, Chunk chunk, Vec3<int> pos)
        {
            var worldpos = chunk.Position * Chunk.Size + pos;
            var curr = chunk[pos];
            BlockData[] neighbors =
            {
                pos.X == Chunk.Size - 1
                    ? chunk.World.GetBlock(new Vec3<int>(worldpos.X + 1, worldpos.Y, worldpos.Z))
                    : chunk[new Vec3<int>(pos.X + 1, pos.Y, pos.Z)],
                pos.X == 0
                    ? chunk.World.GetBlock(new Vec3<int>(worldpos.X - 1, worldpos.Y, worldpos.Z))
                    : chunk[new Vec3<int>(pos.X - 1, pos.Y, pos.Z)],
                pos.Y == Chunk.Size - 1
                    ? chunk.World.GetBlock(new Vec3<int>(worldpos.X, worldpos.Y + 1, worldpos.Z))
                    : chunk[new Vec3<int>(pos.X, pos.Y + 1, pos.Z)],
                pos.Y == 0
                    ? chunk.World.GetBlock(new Vec3<int>(worldpos.X, worldpos.Y - 1, worldpos.Z))
                    : chunk[new Vec3<int>(pos.X, pos.Y - 1, pos.Z)],
                pos.Z == Chunk.Size - 1
                    ? chunk.World.GetBlock(new Vec3<int>(worldpos.X, worldpos.Y, worldpos.Z + 1))
                    : chunk[new Vec3<int>(pos.X, pos.Y, pos.Z + 1)],
                pos.Z == 0
                    ? chunk.World.GetBlock(new Vec3<int>(worldpos.X, worldpos.Y, worldpos.Z - 1))
                    : chunk[new Vec3<int>(pos.X, pos.Y, pos.Z - 1)]
            };

            // Right
            if (AdjacentTest(curr, neighbors[0]))
                fixed (float* tex = _tex[0].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 0.5f, 0.5f, 0.5f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                        tex[0], tex[3], 0.5f, 0.5f, 0.5f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                        tex[2], tex[3], 0.5f, 0.5f, 0.5f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[1], 0.5f, 0.5f, 0.5f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 0.0f
                    );

            // Left
            if (AdjacentTest(curr, neighbors[1]))
                fixed (float* tex = _tex[1].D)
                target.AddPrimitive(4,
                    tex[0], tex[1], 0.5f, 0.5f, 0.5f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 0.0f,
                    tex[0], tex[3], 0.5f, 0.5f, 0.5f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                    tex[2], tex[3], 0.5f, 0.5f, 0.5f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                    tex[2], tex[1], 0.5f, 0.5f, 0.5f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 1.0f
                );

            // Top
            if (AdjacentTest(curr, neighbors[2]))
                fixed (float* tex = _tex[2].D)
                target.AddPrimitive(4,
                    tex[0], tex[1], 1.0f, 1.0f, 1.0f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 0.0f,
                    tex[0], tex[3], 1.0f, 1.0f, 1.0f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                    tex[2], tex[3], 1.0f, 1.0f, 1.0f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                    tex[2], tex[1], 1.0f, 1.0f, 1.0f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 0.0f
                );

            // Bottom
            if (AdjacentTest(curr, neighbors[3]))
                fixed (float* tex = _tex[3].D)
                target.AddPrimitive(4,
                    tex[0], tex[1], 1.0f, 1.0f, 1.0f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                    tex[0], tex[3], 1.0f, 1.0f, 1.0f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                    tex[2], tex[3], 1.0f, 1.0f, 1.0f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                    tex[2], tex[1], 1.0f, 1.0f, 1.0f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 1.0f
                );

            // Front
            if (AdjacentTest(curr, neighbors[4]))
                fixed (float* tex = _tex[4].D)
                target.AddPrimitive(4,
                    tex[0], tex[1], 0.7f, 0.7f, 0.7f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                    tex[0], tex[3], 0.7f, 0.7f, 0.7f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                    tex[2], tex[3], 0.7f, 0.7f, 0.7f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                    tex[2], tex[1], 0.7f, 0.7f, 0.7f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f
                );

            // Back
            if (AdjacentTest(curr, neighbors[5]))
                fixed (float* tex = _tex[5].D)
                target.AddPrimitive(4,
                    tex[0], tex[1], 0.7f, 0.7f, 0.7f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 0.0f,
                    tex[0], tex[3], 0.7f, 0.7f, 0.7f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                    tex[2], tex[3], 0.7f, 0.7f, 0.7f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                    tex[2], tex[1], 0.7f, 0.7f, 0.7f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 0.0f
                );
        }

        static bool AdjacentTest(BlockData a, BlockData b) => a.Id != 0 && !Blocks.Index[b.Id].IsOpaque && a.Id != b.Id;

        private readonly BlockTexCoord[] _tex;
    }

    public unsafe class RawTexture
    {
        public RawTexture(string filename) => Surface = (SDL.SDL_Surface*) SDL_image.IMG_Load(filename);

        public RawTexture(RawTexture other) => Surface =
            (SDL.SDL_Surface*) SDL.SDL_ConvertSurfaceFormat((IntPtr) other.Surface, SDL.SDL_PIXELFORMAT_ABGR8888, 0);

        ~RawTexture() => SDL.SDL_FreeSurface((IntPtr) Surface);

        public SDL.SDL_Surface* Surface { get; }
    };

    public static class BlockTextureBuilder
    {
        public static int Capacity()
        {
            var w = CapacityRaw() / _pixelPerTexture;
            return w * w;
        }

        public static int CapacityRaw()
        {
            int cap = 2048;
            //glGetIntegerv(GL_MAX_TEXTURE_SIZE, &cap);
            return cap;
        }

        public static void SetWidthPerTex(int wid) => _pixelPerTexture = wid;

        public static int GetWidthPerTex() => _pixelPerTexture;

        public static int AddTexture(RawTexture rawTexture)
        {
            RawTexs.Add(rawTexture);
            return RawTexs.Count - 1;
        }

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

        public static int AddTexture(string path) => AddTexture(new RawTexture(path));

        public static int GetTexturePerLine() => _texturePerLine;

        public static unsafe void GetTexturePos(float* pos, uint id)
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

    public static class BlockRendererManager
    {
        public static void Render(VertexBuilder target, int id, Chunk chunk, Vec3<int> pos)
        {
            if (BlockRenderers.Count > 0 && BlockRenderers[id] != null)
                BlockRenderers[id].Render(target, chunk, pos);
        }

        public static void SetBlockRenderer(int pos, IBlockRenderer blockRenderer)
        {
            while (pos >= BlockRenderers.Count)
                BlockRenderers.Add(null);
            BlockRenderers[pos] = blockRenderer;
        }

        public static void FlushTextures()
        {
            foreach (var x in BlockRenderers)
            {
                x?.FlushTexture();
            }
        }

        private static readonly List<IBlockRenderer> BlockRenderers = new List<IBlockRenderer>();
    }
}