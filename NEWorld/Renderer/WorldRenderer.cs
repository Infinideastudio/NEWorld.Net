// 
// GUI: worldrenderer.h
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
using System.Data.Common;
using System.Linq;
using Core;
using Game;

namespace NEWorld.Renderer
{
    /**
     * \brief Manage the VBO of a world. It includes ChunkRenderer.
     */
    public class WorldRenderer
    {
        public const int MaxChunkRenderCount = 4;

        private class RenderDetectorTask : IReadOnlyTask
        {
            public RenderDetectorTask(WorldRenderer worldRenderer, uint currentWorldId, Player player)
            {
                mWorldRenderer = worldRenderer;
                mCurrentWorldId = currentWorldId;
                mPlayer = player;
            }

            public void Task(ChunkService cs)
            {
                var counter = 0;
                // TODO: improve performance by adding multiple instances of this and set a step when itering the chunks.
                // Render build list
                //PODOrderedList<int, Chunk*, MaxChunkRenderCount> chunkRenderList;
                var position = mPlayer.Position;
                var positionInt = new Vec3<int>((int)position.X, (int)position.Y, (int)position.Z);
                var chunkpos = World.GetChunkPos(positionInt);
                var world = cs.Worlds.Get(mCurrentWorldId);
                foreach (var c in world.Chunks)
                {
                    var chunk = c.Value;
                    var chunkPosition = chunk.Position;
                    // In render range, pending to render
                    if (chunk.IsUpdated && chunkpos.ChebyshevDistance(chunkPosition) <= mWorldRenderer.mRenderDist)
                    {
                        if (NeighbourChunkLoadCheck(world, chunkPosition))
                        {
                            // TODO: maybe build a VA pool can speed this up.
                            var crd = new ChunkRenderData();
                            crd.generate(chunk);
                            cs.TaskDispatcher.AddRenderTask(new VBOGenerateTask(world, chunkPosition, crd,
                                mWorldRenderer.mChunkRenderers));
                            if (counter++ == 3) break;
                        }
                    }
                    else
                    {
                        // TODO: Unload unneeded VBO.
                        //       We can't do it here since it's not thread safe
                        /*
                        auto iter = mChunkRenderers.find(chunkPosition);
                        if (iter != mChunkRenderers.end())
                        mChunkRenderers.erase(iter);
                        */
                    }
                }
            }

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            private static readonly Vec3<int>[] Delta =
            {
                new Vec3<int>(1, 0, 0), new Vec3<int>(-1, 0, 0),
                new Vec3<int>(0, 1, 0), new Vec3<int>(0, -1, 0),
                new Vec3<int>(0, 0, 1), new Vec3<int>(0, 0, -1)
            };

            private static bool NeighbourChunkLoadCheck(World world, Vec3<int> pos) =>
                Delta.All(p => world.IsChunkLoaded(pos + p));

            private WorldRenderer mWorldRenderer;
            private uint mCurrentWorldId;
            private Player mPlayer;
        }

        private class VBOGenerateTask : IRenderTask
        {
            public VBOGenerateTask(World world, Vec3<int> position, ChunkRenderData crd,
                Dictionary<Vec3<int>, ChunkRenderer> chunkRenderers)
            {
                mWorld = world;
                mPosition = position;
                mChunkRenderData = crd;
                mChunkRenderers = chunkRenderers;
            }

            public void Task(ChunkService srv)
            {
                if (mWorld.Chunks.TryGetValue(mPosition, out var chunk))
                {
                    chunk.IsUpdated = false;
                    if (mChunkRenderers.TryGetValue(mPosition, out var it))
                    {
                        it.Update(mChunkRenderData);
                    }
                    else
                    {
                        mChunkRenderers.Add(mPosition, new ChunkRenderer(mChunkRenderData));
                    }
                }
            }

            public IRenderTask Clone() => (IRenderTask) MemberwiseClone();

            private World mWorld;
            private Vec3<int> mPosition;
            private ChunkRenderData mChunkRenderData;
            private Dictionary<Vec3<int>, ChunkRenderer> mChunkRenderers;
        }


        public WorldRenderer(World world, int renderDistance)
        {
            mWorld = world;
            mRenderDist = renderDistance;
        }

        // Render all chunks
        public int render(Vec3<int> position)
        {
            var chunkPending = new List<KeyValuePair<Vec3<int>, ChunkRenderer>>();

            var chunkpos = World.GetChunkPos(position);
            foreach (var c in mChunkRenderers)
            {
                if (chunkpos.ChebyshevDistance(c.Key) <= mRenderDist)
                {
                    c.Value.render(c.Key);
                    chunkPending.Add(c);
                }
            }

            //glEnable(GL_BLEND);

            //glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            foreach (var c in chunkPending)
            {
                c.Value.renderTrans(c.Key);
            }

            //glDisable(GL_BLEND);
            return chunkPending.Count;
        }

        public void registerTask(ChunkService chunkService, Player player) => 
            chunkService.TaskDispatcher.AddRegularReadOnlyTask(new RenderDetectorTask(this, mWorld.Id, player));

        private World mWorld;

        // Ranges
        public int mRenderDist;

        // Chunk Renderers
        public Dictionary<Vec3<int>, ChunkRenderer> mChunkRenderers;
    }
}