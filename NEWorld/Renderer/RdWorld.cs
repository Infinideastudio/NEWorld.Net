// 
// NEWorld/NEWorld: RdWorld.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.World;
using Xenko.Core.Mathematics;

namespace NEWorld.Renderer
{
    public class RdWorld
    {
        public const int MaxChunkRenderCount = 64;

        // Chunk Renderers
        private readonly Dictionary<Int3, RdChunk> chunkRenderers;

        // Ranges
        public readonly int RenderDist;

        private readonly World world;

        public RdWorld(World world, Player player, int renderDistance)
        {
            this.world = world;
            RenderDist = renderDistance;
            chunkRenderers = new Dictionary<Int3, RdChunk>();
            ChunkService.TaskDispatcher.AddRegular(new RenderDetectorTask(this, world.Id, player));
        }

        // TODO: Implement this with Chunk Updation Hook Instead
        private class RenderDetectorTask : IRegularReadOnlyTask
        {
            private static readonly Int3[] Delta =
            {
                new Int3(1, 0, 0), new Int3(-1, 0, 0),
                new Int3(0, 1, 0), new Int3(0, -1, 0),
                new Int3(0, 0, 1), new Int3(0, 0, -1)
            };

            private readonly uint currentWorldId;
            private readonly Player player;

            private readonly RdWorld rdWorldRenderer;

            public RenderDetectorTask(RdWorld rdWorldRenderer, uint currentWorldId, Player player)
            {
                this.rdWorldRenderer = rdWorldRenderer;
                this.currentWorldId = currentWorldId;
                this.player = player;
            }

            public void Task(int instance, int instances)
            {
                if (instance == 0)
                {
                    var counter = 0;
                    // TODO: improve performance by adding multiple instances of this and set a step when itering the chunks.
                    var position = player.Position;
                    var center = World.GetChunkPos(new Int3((int) position.X, (int) position.Y, (int) position.Z));
                    var world = ChunkService.Worlds.Get(currentWorldId);
                    foreach (var c in world.Chunks)
                    {
                        var chunk = c.Value;
                        // In render range, pending to render
                        if (chunk.IsUpdated && ChebyshevDistance(center, chunk.Position) <= rdWorldRenderer.RenderDist)
                            if (NeighbourChunkLoadCheck(world, chunk.Position))
                            {
                                GenerateVbo(chunk, rdWorldRenderer.chunkRenderers);
                                if (++counter == MaxChunkRenderCount) break;
                            }
                    }
                }
            }

            private static async void GenerateVbo(Chunk target, Dictionary<Int3, RdChunk> pool)
            {
                await ChunkService.TaskDispatcher.NextReadOnlyChance();
                var model = new ChunkVboGen().Generate(target).Model;
                await ChunkService.TaskDispatcher.NextRenderChance();
                var position = target.Position;
                // TODO: Check the Chunk Directly
                if (!target.World.Chunks.ContainsKey(position)) return;
                target.IsUpdated = false; // TODO: Remove when chunk update driver is complete
                GetOrAddRdChunk(pool, position).Update(model);
            }

            private static RdChunk GetOrAddRdChunk(Dictionary<Int3, RdChunk> pool, Int3 position)
            {
                if (pool.TryGetValue(position, out var it)) return it;
                var renderer = new RdChunk(new Vector3(position.X, position.Y, position.Z));
                Context.OperatingScene.Entities.Add(renderer.Entity);
                pool.Add(position, renderer);
                return renderer;
            }

            // TODO: Remove Type1 Clone
            private static int ChebyshevDistance(Int3 l, Int3 r)
            {
                return Math.Max(Math.Max(Math.Abs(l.X - r.X), Math.Abs(l.Y - r.Y)), Math.Abs(l.Z - r.Z));
            }

            private static bool NeighbourChunkLoadCheck(World world, Int3 pos)
            {
                return Delta.All(p => world.IsChunkLoaded(pos + p));
            }
        }

        
    }
}