// 
// Game: WorldTasks.cs
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
using Game.Network;
using Game.Utilities;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public partial class World
    {
        private const int MaxChunkLoadCount = 64, MaxChunkUnloadCount = 64;

        private static readonly Int3 MiddleOffset =
            new Int3(Chunk.Size / 2 - 1, Chunk.Size / 2 - 1, Chunk.Size / 2 - 1);

        public class ResetChunkTask : IReadWriteTask
        {
            /**
             * \brief Add a constructed chunk into world.
             * \param chunk the target chunk
             */
            public ResetChunkTask(Chunk chunk)
            {
                this.chunk = chunk;
            }

            public void Task(ChunkService srv)
            {
                chunk.World.ResetChunkAndUpdate(chunk.Position, chunk);
            }
            
            private readonly Chunk chunk;
        }

        private class UnloadChunkTask : IReadWriteTask
        {
            /**
            * \brief Given a chunk, it will try to unload it (decrease a ref)
            * \param world the target world
            * \param chunkPosition the position of the chunk
            */
            public UnloadChunkTask(World world, Int3 chunkPosition)
            {
                this.world = world;
                this.chunkPosition = chunkPosition;
            }

            public void Task(ChunkService srv)
            {
                //TODO: for multiplayer situation, it should decrease ref counter instead of deleting
                world.DeleteChunk(chunkPosition);
            }

            private readonly World world;
            private readonly Int3 chunkPosition;
        }

        private class BuildOrLoadChunkTask : IReadOnlyTask
        {
            /**
             * \brief Given a chunk, it will try to load it or build it
             * \param world the target world
             * \param chunkPosition the position of the chunk
             */
            public BuildOrLoadChunkTask(World world, Int3 chunkPosition)
            {
                this.world = world;
                this.chunkPosition = chunkPosition;
            }

            public void Task(ChunkService srv)
            {
                srv.TaskDispatcher.Add(new ResetChunkTask(new Chunk(chunkPosition, world)));
            }

            private readonly World world;
            private readonly Int3 chunkPosition;
        }

        private class LoadUnloadDetectorTask : IReadOnlyTask
        {
            private class AddToWorldTask : IReadWriteTask
            {
                /**
                 * \brief Add a constructed chunk into world.
                 * \param worldID the target world's id
                 * \param chunk the target chunk
                 */
                public AddToWorldTask(Chunk chunk)
                {
                    this.chunk = chunk;
                }
                
                public void Task(ChunkService srv)
                {
                    chunk.World.InsertChunkAndUpdate(chunk.Position, chunk);
                }
                
                private readonly Chunk chunk;
            }

            public LoadUnloadDetectorTask(World world, Player player)
            {
                this.player = player;
                this.world = world;
            }

            public void Task(ChunkService cs)
            {
                var loadList = new OrderedListIntLess<Int3>(MaxChunkLoadCount);
                var unloadList = new OrderedListIntGreater<Chunk>(MaxChunkUnloadCount);
                var playerPos = player.Position;
                var position = new Int3((int) playerPos.X, (int) playerPos.Y, (int) playerPos.Z);
                GenerateLoadUnloadList(world, position, 4, loadList, unloadList);

                foreach (var loadPos in loadList)
                {
                    // load a fake chunk
                    cs.TaskDispatcher.Add(new AddToWorldTask(new Chunk(loadPos.Value, world, false)));
                    cs.TaskDispatcher.Add(new BuildOrLoadChunkTask(world, loadPos.Value));
                    if (!cs.IsAuthority)
                    {
                        Client.GetChunk.Call(world.Id, loadPos.Value);
                    }
                }

                foreach (var unloadChunk in unloadList)
                {
                    // add a unload task.
                    cs.TaskDispatcher.Add(new UnloadChunkTask(world, unloadChunk.Value.Position));
                }
            }

            /**
             * \brief Find the nearest chunks in load range to load,
             *        fartherest chunks out of load range to unload.
             * \param world the world to load or unload chunks
             * \param centerPos the center position
             * \param loadRange chunk load range
             * \param loadList (Output) Chunk load list [position, distance]
             * \param unloadList (Output) Chunk unload list [pointer, distance]
             */
            private static void GenerateLoadUnloadList(World world, Int3 centerPos, int loadRange,
                OrderedListIntLess<Int3> loadList, OrderedListIntGreater<Chunk> unloadList)
            {
                // centerPos to chunk coords
                var centerCPos = GetChunkPos(centerPos);

                foreach (var chunk in world.Chunks)
                {
                    var curPos = chunk.Value.Position;
                    // Out of load range, pending to unload
                    if (ChebyshevDistance(centerCPos, curPos) > loadRange)
                        unloadList.Insert((curPos * Chunk.Size + MiddleOffset - centerPos).LengthSquared(), chunk.Value);
                }

                for (var x = centerCPos.X - loadRange; x <= centerCPos.X + loadRange; x++)
                {
                    for (var y = centerCPos.Y - loadRange; y <= centerCPos.Y + loadRange; y++)
                    {
                        for (var z = centerCPos.Z - loadRange; z <= centerCPos.Z + loadRange; z++)
                        {
                            var position = new Int3(x, y, z);
                            // In load range, pending to load
                            if (!world.IsChunkLoaded(position))
                                loadList.Insert((position * Chunk.Size + MiddleOffset - centerPos).LengthSquared(),
                                    position);
                        }
                    }
                }
            }

            // TODO: Remove Type1 Clone
            private static int ChebyshevDistance(Int3 l, Int3 r) => Math.Max(Math.Max(Math.Abs(l.X - r.X), Math.Abs(l.Y - r.Y)), Math.Abs(l.Z - r.Z));

            private readonly Player player;
            private readonly World world;
        }

        public void RegisterChunkTasks(ChunkService cs, Player player) =>
            cs.TaskDispatcher.AddRegular(new LoadUnloadDetectorTask(this, player));
    }
}