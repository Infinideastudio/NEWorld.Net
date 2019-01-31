// NEWorld: GameScene.cs
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

using Game.Terrain;
using Game.World;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Rendering;

namespace NEWorld.Renderer
{
    /**
     * \brief It stores all the render data (VA) used to render a chunk.
     *        But it does not involve OpenGL operations so it can be
     *        safely called from all threads.
     */
    public class ChunkRenderData
    {
        public Model Model { get; private set; }

        /**
         * \brief Generate the render data, namely VA, from a chunk.
         *        Does not involve OpenGL functions.
         * \param chunk the chunk to be rendered.
         */
        public void Generate(Chunk chunk)
        {
            using (VertexBuilder vaOpacity = new VertexBuilder(262144 * (2 + 3)), vaTranslucent = new VertexBuilder(262144 * (2 + 3))) {
                var tmp = new Int3();
                for (tmp.X = 0; tmp.X < Chunk.Size; ++tmp.X)
                for (tmp.Y = 0; tmp.Y < Chunk.Size; ++tmp.Y)
                for (tmp.Z = 0; tmp.Z < Chunk.Size; ++tmp.Z)
                {
                    var b = chunk[tmp];
                    var target = Blocks.Index[b.Id].IsTranslucent ? vaTranslucent : vaOpacity;
                    BlockRenderers.Render(target, b.Id, chunk, tmp);
                }

                var mesh0 = vaOpacity.Dump();
                var mesh1 = vaTranslucent.Dump();
                Model = mesh0 != null && mesh1 != null ? new Model {new MaterialInstance(Context.Material)} : null;
                if (mesh0 != null) Model?.Add(mesh0);
                if (mesh1 != null) Model?.Add(mesh1);
                //if (Model != null) Model.Materials[0].IsShadowCaster = true;
            }
        }
    }

    /**
     * \brief The renderer that can be used to render directly. It includes
     *        VBO that we need to render. It can be generated from a
     *        ChunkRenderData
     */
    public class RdChunk
    {
        public RdChunk(ChunkRenderData data, Vector3 chunkPosition)
        {
            Entity = new Entity();
            Update(data);
            Entity.GetOrCreate<TransformComponent>().Position = chunkPosition * Chunk.Size;
        }

        public Entity Entity { get; }

        /**
         * \brief Generate VBO from VA. Note that this function will call
         *        OpenGL functions and thus can be only called from the
         *        main thread.
         * \param data The render data that will be used to generate VBO
         */
        public void Update(ChunkRenderData data)
        {
            var model = data.Model;
            if (model != null)
                Entity.GetOrCreate<ModelComponent>().Model = data.Model;
            else
                Entity.Remove<ModelComponent>();
        }
    }
}