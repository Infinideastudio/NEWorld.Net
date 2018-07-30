// 
// GUI: chunkrenderer.h
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

using Core;
using Game;
using OpenGL;

namespace NEWorld.Renderer
{
    /**
     * \brief It stores all the render data (VA) used to render a chunk.
     *        But it does not involve OpenGL operations so it can be
     *        safely called from all threads.
     */
    public class ChunkRenderData
    {
        /**
         * \brief Generate the render data, namely VA, from a chunk.
         *        Does not involve OpenGL functions.
         * \param chunk the chunk to be rendered.
         */
        public void generate(Chunk chunk)
        {
            // TODO: merge face rendering
            var tmp = new Vec3<int>();
            for (tmp.X = 0; tmp.X < Chunk.Size; ++tmp.X)
            for (tmp.Y = 0; tmp.Y < Chunk.Size; ++tmp.Y)
            for (tmp.Z = 0; tmp.Z < Chunk.Size; ++tmp.Z)
            {
                var b = chunk[tmp];
                var target = Blocks.Index[b.Id].IsTranslucent ? VATranslucent : VAOpacity;
                BlockRendererManager.render(target, b.Id, chunk, tmp);
            }
        }
        
        public VertexBuilder VAOpacity { get; } // {262144, VertexFormat(2, 3, 0, 3)};

        public VertexBuilder VATranslucent { get; } //{262144, VertexFormat(2, 3, 0, 3)};
    };

    /**
     * \brief The renderer that can be used to render directly. It includes
     *        VBO that we need to render. It can be generated from a
     *        ChunkRenderData
     */
    public class ChunkRenderer
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
            mBuffer = data.VAOpacity.Dump();
            mBufferTrans = data.VATranslucent.Dump();
            _normCount = data.VAOpacity.VertCount;
            _transCount = data.VATranslucent.VertCount;
        }

        // Draw call
        public void render(Vec3<int> c)
        {
            if (mBuffer != null)
            {
                //Renderer::translate(Vec3f(c * Chunk.Size));
                //mBuffer.render();
                //Renderer::translate(Vec3f(-c * Chunk.Size));
            }
        }

        public void renderTrans(Vec3<int> c)
        {
            if (mBufferTrans != null)
            {
                //Renderer::translate(Vec3f(c * Chunk.Size));
                //mBufferTrans.render();
                //Renderer::translate(Vec3f(-c * Chunk.Size));
            }
        }

        // Vertex buffer object
        private int _normCount, _transCount;
        private ConstDataBuffer mBuffer, mBufferTrans;
    };
}