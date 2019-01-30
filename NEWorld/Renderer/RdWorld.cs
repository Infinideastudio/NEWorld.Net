// 
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

using System;
using Game;
using Game.World;
using System.Collections.Generic;
using System.Linq;
using Core.Utilities;
using Xenko.Core.Mathematics;

namespace NEWorld.Renderer
{
    public class RdWorld
    {
        public const int MaxChunkRenderCount = 4;

        private class RenderDetectorTask : IReadOnlyTask
        {
            public RenderDetectorTask(RdWorld rdWorldRenderer, uint currentWorldId, Player player)
            {
                this.rdWorldRenderer = rdWorldRenderer;
                this.currentWorldId = currentWorldId;
                this.player = player;
            }

            public void Task(ChunkService cs)
            {
                var counter = 0;
                // TODO: improve performance by adding multiple instances of this and set a step when itering the chunks.
                var position = player.Position;
                var positionInt = new Int3((int) position.X, (int) position.Y, (int) position.Z);
                var chunkpos = World.GetChunkPos(positionInt);
                var world = cs.Worlds.Get(currentWorldId);
                foreach (var c in world.Chunks)
                {
                    var chunk = c.Value;
                    var chunkPosition = chunk.Position;
                    // In render range, pending to render
                    if (chunk.IsUpdated && ChebyshevDistance(chunkpos, chunkPosition) <= rdWorldRenderer.RenderDist)
                    {
                        if (NeighbourChunkLoadCheck(world, chunkPosition))
                        {
                            // TODO: maybe build a VA pool can speed this up.
                            var crd = new ChunkRenderData();
                            crd.Generate(chunk);
                            cs.TaskDispatcher.Add(new VboGenerateTask(world, chunkPosition, crd,
                                rdWorldRenderer.chunkRenderers));
                            if (++counter == MaxChunkRenderCount) break;
                        }
                    }
                }
            }
            
            // TODO: Remove Type1 Clone
            private static int ChebyshevDistance(Int3 l, Int3 r) => Math.Max(Math.Max(Math.Abs(l.X - r.X), Math.Abs(l.Y - r.Y)), Math.Abs(l.Z - r.Z));

            private static readonly Int3[] Delta =
            {
                new Int3(1, 0, 0), new Int3(-1, 0, 0),
                new Int3(0, 1, 0), new Int3(0, -1, 0),
                new Int3(0, 0, 1), new Int3(0, 0, -1)
            };

            private static bool NeighbourChunkLoadCheck(World world, Int3 pos) =>
                Delta.All(p => world.IsChunkLoaded(pos + p));

            private readonly RdWorld rdWorldRenderer;
            private readonly uint currentWorldId;
            private readonly Player player;
        }

        private class VboGenerateTask : IRenderTask
        {
            public VboGenerateTask(World world, Int3 position, ChunkRenderData crd,
                Dictionary<Int3, RdChunk> chunkRenderers)
            {
                this.world = world;
                this.position = position;
                chunkRenderData = crd;
                this.chunkRenderers = chunkRenderers;
            }

            public void Task(ChunkService srv)
            {
                if (!world.Chunks.TryGetValue(position, out var chunk)) return;
                chunk.IsUpdated = false;
                if (chunkRenderers.TryGetValue(position, out var it))
                {
                    it.Update(chunkRenderData);
                }
                else
                {
                    var renderer = new RdChunk(chunkRenderData, new Vector3(position.X, position.Y, position.Z));
                    Context.OperatingScene.Entities.Add(renderer.Entity);
                    chunkRenderers.Add(position, renderer);
                }
            }

            private readonly World world;
            private readonly Int3 position;
            private readonly ChunkRenderData chunkRenderData;
            private readonly Dictionary<Int3, RdChunk> chunkRenderers;
        }

        public RdWorld(World world, Player player, int renderDistance)
        {
            this.world = world;
            RenderDist = renderDistance;
            chunkRenderers = new Dictionary<Int3, RdChunk>();
            Singleton<ChunkService>.Instance.TaskDispatcher.AddRegular(new RenderDetectorTask(this, world.Id, player));
        }
        
        private readonly World world;

        // Ranges
        public readonly int RenderDist;

        // Chunk Renderers
        private readonly Dictionary<Int3, RdChunk> chunkRenderers;
    }
}
