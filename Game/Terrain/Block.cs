// 
// NEWorld/Game: Block.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2019 NEWorld Team
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
    public struct BlockTexCoord
    {
        public uint Id;
        public Int2 Tex;
    }

    public class BlockRenderContext
    {
        public readonly Chunk Current;
        public readonly Chunk[] Neighbors;

        public BlockRenderContext(Chunk current, Chunk[] neighbors)
        {
            Current = current;
            Neighbors = neighbors;
        }
    }

    public interface IBlockRenderer
    {
        void FlushTexture(IBlockTextures textures);
        void Render(IVertexBuilder target, BlockRenderContext context, Int3 tmp);
    }

    public interface IVertexBuilder
    {
        void Rect(Int3 position, Int2 tex, uint face, int rotation, uint shade);
    }

    public interface IBlockTextures
    {
        uint Add(string assetUri);
        void GetTexturePos(ref BlockTexCoord pos);
    }

    public class DefaultBlockRenderer : IBlockRenderer
    {
        private readonly BlockTexCoord[] tex;

        public DefaultBlockRenderer(uint[] data)
        {
            tex = new BlockTexCoord[6];
            for (var i = 0; i < 6; ++i)
                tex[i].Id = data[i];
        }

        public void FlushTexture(IBlockTextures textures)
        {
            for (var i = 0; i < 6; ++i)
                textures.GetTexturePos(ref tex[i]);
        }

        public void Render(IVertexBuilder target, BlockRenderContext context, Int3 pos)
        {
            var current = context.Current[pos];
            BlockData[] neighbors =
            {
                pos.X == Chunk.RowLast
                    ? context.Neighbors[0][0, pos.Y, pos.Z]
                    : context.Current[pos.X + 1, pos.Y, pos.Z],
                pos.X == 0
                    ? context.Neighbors[1][Chunk.RowLast, pos.Y, pos.Z]
                    : context.Current[pos.X - 1, pos.Y, pos.Z],
                pos.Y == Chunk.RowLast
                    ? context.Neighbors[2][pos.X, 0, pos.Z]
                    : context.Current[pos.X, pos.Y + 1, pos.Z],
                pos.Y == 0
                    ? context.Neighbors[3][pos.X, Chunk.RowLast, pos.Z]
                    : context.Current[pos.X, pos.Y - 1, pos.Z],
                pos.Z == Chunk.RowLast
                    ? context.Neighbors[4][pos.X, pos.Y, 0]
                    : context.Current[pos.X, pos.Y, pos.Z + 1],
                pos.Z == 0
                    ? context.Neighbors[5][pos.X, pos.Y, Chunk.RowLast]
                    : context.Current[pos.X, pos.Y, pos.Z - 1]
            };
            // Right Left Top Bottom Front Back
            for (uint i = 0; i < 6; ++i)
                if (AdjacentTest(current, neighbors[i]))
                    target.Rect(pos, tex[i].Tex, i, 0, 15);
        }

        private static bool AdjacentTest(BlockData a, BlockData b)
        {
            return a.Id != 0 && !Blocks.Index[b.Id].IsOpaque && a.Id != b.Id;
        }
    }

    public static class BlockRenderers
    {
        private static readonly List<IBlockRenderer> Renderers = new List<IBlockRenderer>();

        public static void Render(IVertexBuilder target, int id, BlockRenderContext context, Int3 pos)
        {
            if (Renderers.Count > 0 && Renderers[id] != null)
                Renderers[id].Render(target, context, pos);
        }

        public static void Add(int pos, IBlockRenderer blockRenderer)
        {
            while (pos >= Renderers.Count)
                Renderers.Add(null);
            Renderers[pos] = blockRenderer;
        }

        public static void FlushTextures(IBlockTextures textures)
        {
            foreach (var x in Renderers) x?.FlushTexture(textures);
        }
    }
}