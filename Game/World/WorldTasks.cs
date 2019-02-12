// 
// NEWorld/Game: WorldTasks.cs
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
using System.Threading;
using Game.Utilities;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public partial class World
    {
        private const int MaxChunkLoadCount = 64, MaxChunkUnloadCount = 64;

        private static readonly Int3 MiddleOffset =
            new Int3(Chunk.RowSize / 2 - 1, Chunk.RowSize / 2 - 1, Chunk.RowSize / 2 - 1);

        public void RegisterChunkTasks(Player player)
        {
            ChunkService.TaskDispatcher.AddRegular(new LoadUnloadDetectorTask(this, player));
        }

        public class ResetChunkTask : IReadWriteTask
        {
            private readonly Chunk chunk;

            /**
             * \brief AddReadOnlyTask a constructed chunk into world.
             * \param chunk the target chunk
             */
            public ResetChunkTask(Chunk chunk)
            {
                this.chunk = chunk;
            }

            public void Task()
            {
                chunk.World.ResetChunkAndUpdate(chunk);
            }
        }

        private class LoadTask : IReadWriteTask
        {
            private Chunk chunk;
            private bool entryAdded;

            internal LoadTask(World world, Int3 chunkPosition)
            {
                // Adding Sentry
                chunk = new Chunk(chunkPosition, world, Chunk.InitOption.None);
                if (!ChunkService.IsAuthority) Network.Client.GetChunk.Call(world.Id, chunkPosition);
                LocalLoad(world, chunkPosition);
            }

            public void Task()
            {
                var operated = Interlocked.Exchange(ref chunk, null);
                if (entryAdded)
                    operated.World.ResetChunkAndUpdate(operated);
                else
                {
                    entryAdded = true;
                    operated.World.InsertChunkAndUpdate(operated);
                }
            }

            private void Reset(Chunk chk)
            {
                if (Interlocked.Exchange(ref chunk, chk) == null)
                    ChunkService.TaskDispatcher.Add(this);
            }

            private async void LocalLoad(World world, Int3 position)
            {
                await ChunkService.TaskDispatcher.NextReadOnlyChance();
                Chunk chk;
                if (world.ChunkExistsInDisk(chunk))
                {
                    chk = new Chunk(position, world, Chunk.InitOption.None);
                    world.LoadChunkFromDisk(ref chk);
                }
                else
                {
                    chk = new Chunk(position, world);
                }
                Reset(chk);
            }
        }

        private class UnloadChunkTask : IReadWriteTask
        {
            private readonly Chunk chunk;

            /**
            * \brief Given a chunk, it will try to unload it (decrease a ref)
            * \param world the target world
            * \param chunkPosition the position of the chunk
            */
            public UnloadChunkTask(Chunk chk)
            {
                chunk = chk;
            }

            public void Task()
            {
                //TODO: for multiplayer situation, it should decrease ref counter instead of deleting
                chunk.World.SaveChunkToDisk(chunk);
                chunk.World.DeleteChunk(chunk.Position);
                chunk.Dispose();
            }
        }

        private class LoadUnloadDetectorTask : IRegularReadOnlyTask
        {
            private readonly Player player;
            private readonly World world;

            private class Instance
            {
                private readonly Int3 centerPos;
                private readonly World world;
                private readonly int instance, instances;
                private readonly OrderedListIntLess<Int3> loadList = new OrderedListIntLess<Int3>(MaxChunkLoadCount);
                private readonly OrderedListIntGreater<Chunk> unloadList = new OrderedListIntGreater<Chunk>(MaxChunkUnloadCount);

                internal Instance(World wrd, Player player, int ins, int inst)
                {
                    world = wrd;
                    instance = ins;
                    instances = inst;
                    var playerPos = player.Position;
                    centerPos = new Int3((int) playerPos.X, (int) playerPos.Y, (int) playerPos.Z);
                }

                internal void Run()
                {
                    GenerateLoadUnloadList(4);

                    foreach (var loadPos in loadList)
                        ChunkService.TaskDispatcher.Add(new LoadTask(world, loadPos.Value));

                    foreach (var unloadChunk in unloadList)
                        // add a unload task.
                        ChunkService.TaskDispatcher.Add(new UnloadChunkTask(unloadChunk.Value));
                }

                private void GenerateLoadUnloadList(int loadRange)
                {
                    var centerCPos = GetChunkPos(centerPos);

                    // TODO: Instance this
                    if (instance == 0 )
                    {
                        foreach (var chunk in world.Chunks)
                        {
                            var curPos = chunk.Value.Position;
                            // Out of load range, pending to unload
                            if (ChebyshevDistance(centerCPos, curPos) > loadRange)
                                unloadList.Insert((curPos * Chunk.RowSize + MiddleOffset - centerPos).LengthSquared(),
                                    chunk.Value);
                        }
                    }

                    var edge1 = loadRange * 2 + 1;
                    var edge2 = edge1 * edge1;
                    var edge3 = edge2 * edge1;
                    var corner = new Int3(centerCPos.X - loadRange, centerCPos.Y - loadRange, centerCPos.Z - loadRange);
                    for (var i = instance; i < edge3; i += instances)
                    {
                        var position = corner + new Int3(i / edge2, (i % edge2) / edge1, i % edge1);
                        // In load range, pending to load
                        if (!world.IsChunkLoaded(position))
                            loadList.Insert((position * Chunk.RowSize + MiddleOffset - centerPos).LengthSquared(),
                                position);
                    }
                }
            }

            public LoadUnloadDetectorTask(World world, Player player)
            {
                this.player = player;
                this.world = world;
            }

            public void Task(int instance, int instances)
            {
                new Instance(world, player, instance, instances).Run();
            }
                
            // TODO: Remove Type1 Clone
            private static int ChebyshevDistance(Int3 l, Int3 r)
            {
                return Math.Max(Math.Max(Math.Abs(l.X - r.X), Math.Abs(l.Y - r.Y)), Math.Abs(l.Z - r.Z));
            }
        }
    }
}