// 
// Game: Block.cs
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

using System.Collections.Generic;
using Game.World;
using Xenko.Core.Mathematics;

namespace Game.Terrain
{
    public unsafe struct BlockTexCoord
    {
        public uint Pos;
        public fixed float D[4];
    }

    public interface IBlockRenderer
    {
        void FlushTexture(IBlockTextures textures);
        void Render(IVertexBuilder target, Chunk chunk, Int3 pos);
    }

    public interface IVertexBuilder
    {
        void AddPrimitive(int verts, params float[] data);
    }

    public interface IBlockTextures
    {
        uint Add(string path);
        unsafe void GetTexturePos(float* pos, uint id);
    }

    public class DefaultBlockRenderer : IBlockRenderer
    {
        public DefaultBlockRenderer(uint[] data)
        {
            tex = new BlockTexCoord[6];
            for (var i = 0; i < 6; ++i)
                tex[i].Pos = data[i];
        }

        public unsafe void FlushTexture(IBlockTextures textures)
        {
            for (var i = 0; i < 6; ++i)
                fixed (float* tex = this.tex[0].D)
                    textures.GetTexturePos(tex, this.tex[i].Pos);
        }

        public unsafe void Render(IVertexBuilder target, Chunk chunk, Int3 pos)
        {
            var worldpos = chunk.Position * Chunk.Size + pos;
            var curr = chunk[pos];
            BlockData[] neighbors =
            {
                pos.X == Chunk.Size - 1
                    ? chunk.World.GetBlock(new Int3(worldpos.X + 1, worldpos.Y, worldpos.Z))
                    : chunk[new Int3(pos.X + 1, pos.Y, pos.Z)],
                pos.X == 0
                    ? chunk.World.GetBlock(new Int3(worldpos.X - 1, worldpos.Y, worldpos.Z))
                    : chunk[new Int3(pos.X - 1, pos.Y, pos.Z)],
                pos.Y == Chunk.Size - 1
                    ? chunk.World.GetBlock(new Int3(worldpos.X, worldpos.Y + 1, worldpos.Z))
                    : chunk[new Int3(pos.X, pos.Y + 1, pos.Z)],
                pos.Y == 0
                    ? chunk.World.GetBlock(new Int3(worldpos.X, worldpos.Y - 1, worldpos.Z))
                    : chunk[new Int3(pos.X, pos.Y - 1, pos.Z)],
                pos.Z == Chunk.Size - 1
                    ? chunk.World.GetBlock(new Int3(worldpos.X, worldpos.Y, worldpos.Z + 1))
                    : chunk[new Int3(pos.X, pos.Y, pos.Z + 1)],
                pos.Z == 0
                    ? chunk.World.GetBlock(new Int3(worldpos.X, worldpos.Y, worldpos.Z - 1))
                    : chunk[new Int3(pos.X, pos.Y, pos.Z - 1)]
            };

            // Right
            if (AdjacentTest(curr, neighbors[0]))
                fixed (float* tex = this.tex[0].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 0.5f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                        tex[0], tex[3], 0.5f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                        tex[2], tex[3], 0.5f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[1], 0.5f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 0.0f
                    );

            // Left
            if (AdjacentTest(curr, neighbors[1]))
                fixed (float* tex = this.tex[1].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 0.5f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 0.0f,
                        tex[0], tex[3], 0.5f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[3], 0.5f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                        tex[2], tex[1], 0.5f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 1.0f
                    );

            // Top
            if (AdjacentTest(curr, neighbors[2]))
                fixed (float* tex = this.tex[2].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 1.0f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 0.0f,
                        tex[0], tex[3], 1.0f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                        tex[2], tex[3], 1.0f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                        tex[2], tex[1], 1.0f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 0.0f
                    );

            // Bottom
            if (AdjacentTest(curr, neighbors[3]))
                fixed (float* tex = this.tex[3].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 1.0f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                        tex[0], tex[3], 1.0f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[3], 1.0f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[1], 1.0f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 1.0f
                    );

            // Front
            if (AdjacentTest(curr, neighbors[4]))
                fixed (float* tex = this.tex[4].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 0.7f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 1.0f,
                        tex[0], tex[3], 0.7f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                        tex[2], tex[3], 0.7f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 1.0f,
                        tex[2], tex[1], 0.7f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f
                    );

            // Back
            if (AdjacentTest(curr, neighbors[5]))
                fixed (float* tex = this.tex[5].D)
                    target.AddPrimitive(4,
                        tex[0], tex[1], 0.7f, pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 0.0f,
                        tex[0], tex[3], 0.7f, pos.X + 1.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[3], 0.7f, pos.X + 0.0f, pos.Y + 0.0f, pos.Z + 0.0f,
                        tex[2], tex[1], 0.7f, pos.X + 0.0f, pos.Y + 1.0f, pos.Z + 0.0f
                    );
        }

        static bool AdjacentTest(BlockData a, BlockData b) => a.Id != 0 && !Blocks.Index[b.Id].IsOpaque && a.Id != b.Id;

        private readonly BlockTexCoord[] tex;
    }

    public static class BlockRenderers
    {
        public static void Render(IVertexBuilder target, int id, Chunk chunk, Int3 pos)
        {
            if (Renderers.Count > 0 && Renderers[id] != null)
                Renderers[id].Render(target, chunk, pos);
        }

        public static void Add(int pos, IBlockRenderer blockRenderer)
        {
            while (pos >= Renderers.Count)
                Renderers.Add(null);
            Renderers[pos] = blockRenderer;
        }

        public static void FlushTextures(IBlockTextures textures)
        {
            foreach (var x in Renderers)
            {
                x?.FlushTexture(textures);
            }
        }

        private static readonly List<IBlockRenderer> Renderers = new List<IBlockRenderer>();
    }
}