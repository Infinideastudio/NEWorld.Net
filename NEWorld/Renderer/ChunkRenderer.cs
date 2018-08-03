// 
// NEWorld: ChunkRenderer.cs
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
using Core.Math;
using Core.Utilities;
using Game.Terrain;
using Game.World;
using OpenGL;

namespace NEWorld.Renderer
{
    public class VertexBuilder : IVertexBuilder
    {
        public VertexBuilder(int size) => Data = Marshal.AllocHGlobal(size * sizeof(float));

        ~VertexBuilder() => Marshal.FreeHGlobal(Data);

        public void AddPrimitive(int verts, params float[] data)
        {
            VertCount += verts;
            Marshal.Copy(data, 0, Data + Size * sizeof(float), data.Length);
            Size += data.Length;
        }

        public ConstDataBuffer Dump() => VertCount > 0 ? new ConstDataBuffer(Size * sizeof(float), Data) : null;

        public int Size;
        public int VertCount;
        public readonly IntPtr Data;
    }

    /**
     * \brief It stores all the render data (VA) used to render a chunk.
     *        But it does not involve OpenGL operations so it can be
     *        safely called from all threads.
     */
    public class ChunkRenderData
    {
        public ChunkRenderData()
        {
            VaOpacity = new VertexBuilder(262144 * (2 + 3 + 0 + 3));
            VaTranslucent = new VertexBuilder(262144 * (2 + 3 + 0 + 3));
        }

        /**
         * \brief Generate the render data, namely VA, from a chunk.
         *        Does not involve OpenGL functions.
         * \param chunk the chunk to be rendered.
         */
        public void Generate(Chunk chunk)
        {
            // TODO: merge face rendering
            var tmp = new Vec3<int>();
            for (tmp.X = 0; tmp.X < Chunk.Size; ++tmp.X)
            for (tmp.Y = 0; tmp.Y < Chunk.Size; ++tmp.Y)
            for (tmp.Z = 0; tmp.Z < Chunk.Size; ++tmp.Z)
            {
                var b = chunk[tmp];
                var target = Blocks.Index[b.Id].IsTranslucent ? VaTranslucent : VaOpacity;
                BlockRenderers.Render(target, b.Id, chunk, tmp);
            }
        }

        public VertexBuilder VaOpacity { get; } // {262144, VertexFormat(2, 3, 0, 3)};

        public VertexBuilder VaTranslucent { get; } //{262144, VertexFormat(2, 3, 0, 3)};
    }

    /**
     * \brief The renderer that can be used to render directly. It includes
     *        VBO that we need to render. It can be generated from a
     *        ChunkRenderData
     */
    public class ChunkRenderer : StrictDispose
    {
        public ChunkRenderer(ChunkRenderData data) => Update(data);

        /**
         * \brief Generate VBO from VA. Note that this function will call
         *        OpenGL functions and thus can be only called from the
         *        main thread.
         * \param data The render data that will be used to generate VBO
         */
        public void Update(ChunkRenderData data)
        {
            Release();
            _buffer = data.VaOpacity.Dump();
            _bufferTrans = data.VaTranslucent.Dump();
            _normCount = data.VaOpacity.VertCount;
            _transCount = data.VaTranslucent.VertCount;
        }

        protected override void Release()
        {
            _buffer?.Dispose();
            _bufferTrans?.Dispose();
        }

        // Draw call
        public void Render(Vec3<int> c, WorldRenderer rd)
        {
            if (_buffer != null)
            {
                Matrix.ModelTranslate(c * Chunk.Size);
                rd.FlushMatrix();
                rd.RenderBuffer(_buffer, _normCount);
                Matrix.ModelTranslate(-c * Chunk.Size);
            }
        }

        public void RenderTrans(Vec3<int> c, WorldRenderer rd)
        {
            if (_bufferTrans != null)
            {
                Matrix.ModelTranslate(c * Chunk.Size);
                rd.FlushMatrix();
                rd.RenderBuffer(_bufferTrans, _transCount);
                Matrix.ModelTranslate(-c * Chunk.Size);
            }
        }

        // Vertex buffer object
        private int _normCount, _transCount;
        private ConstDataBuffer _buffer, _bufferTrans;
    }
}