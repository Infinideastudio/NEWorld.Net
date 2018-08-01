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

using System.Collections.Generic;
using Core;
using Core.Math;

namespace Game
{
    public partial class World
    {
        private const int MaxChunkLoadCount = 64, MaxChunkUnloadCount = 64;

        static Vec3<int> _middleOffset = new Vec3<int>(Chunk.Size / 2 - 1, Chunk.Size / 2 - 1, Chunk.Size / 2 - 1);

        /**
        * \brief Find the nearest chunks in load range to load,
        *        fartherest chunks out of load range to unload.
        * \param world the world to load or unload chunks
        * \param centerPos the center position
        * \param loadRange chunk load range
        * \param loadList (Output) Chunk load list [position, distance]
        * \param unloadList (Output) Chunk unload list [pointer, distance]
        */
        static void GenerateLoadUnloadList(World world, Vec3<int> centerPos, int loadRange,
            OrderedListIntLess<Vec3<int>> loadList, OrderedListIntGreater<Chunk> unloadList)
        {
            // centerPos to chunk coords
            var centerCPos = GetChunkPos(centerPos);

            foreach (var chunk in world.Chunks)
            {
                var curPos = chunk.Value.Position;
                // Out of load range, pending to unload
                if (centerCPos.ChebyshevDistance(curPos) > loadRange)
                    unloadList.Insert((curPos * Chunk.Size + _middleOffset - centerPos).LengthSqr(), chunk.Value);
            }

            for (var x = centerCPos.X - loadRange; x <= centerCPos.X + loadRange; x++)
            {
                for (var y = centerCPos.Y - loadRange; y <= centerCPos.Y + loadRange; y++)
                {
                    for (var z = centerCPos.Z - loadRange; z <= centerCPos.Z + loadRange; z++)
                    {
                        var position = new Vec3<int>(x, y, z);
                        // In load range, pending to load
                        if (!world.IsChunkLoaded(position))
                            loadList.Insert((position * Chunk.Size + _middleOffset - centerPos).LengthSqr(), position);
                    }
                }
            }
        }

        public class AddToWorldTask : IReadWriteTask
        {
            /**
             * \brief Add a constructed chunk into world.
             * \param worldID the target world's id
             * \param chunk the target chunk
             */
            public AddToWorldTask(uint worldId, Chunk chunk)
            {
                _worldId = worldId;
                _chunk = chunk;
            }

            public IReadWriteTask Clone() => (IReadWriteTask) MemberwiseClone();

            public void Task(ChunkService srv)
            {
                var world = srv.Worlds.Get(_worldId);
                world.InsertChunkAndUpdate(_chunk.Position, _chunk);
            }

            private uint _worldId;
            private Chunk _chunk;
        }

        private class UnloadChunkTask : IReadWriteTask
        {
            /**
            * \brief Given a chunk, it will try to unload it (decrease a ref)
            * \param world the target world
            * \param chunkPosition the position of the chunk
            */
            public UnloadChunkTask(World world, Vec3<int> chunkPosition)
            {
                _world = world;
                _chunkPosition = chunkPosition;
            }

            public IReadWriteTask Clone() => (IReadWriteTask) MemberwiseClone();

            public void Task(ChunkService srv)
            {
                //TODO: for multiplayer situation, it should decrease ref counter instead of deleting
                _world.DeleteChunk(_chunkPosition);
            }

            private World _world;
            private Vec3<int> _chunkPosition;
        }

        private class BuildOrLoadChunkTask : IReadOnlyTask
        {
            /**
             * \brief Given a chunk, it will try to load it or build it
             * \param world the target world
             * \param chunkPosition the position of the chunk
             */
            public BuildOrLoadChunkTask(World world, Vec3<int> chunkPosition)
            {
                _world = world;
                _chunkPosition = chunkPosition;
            }

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            public void Task(ChunkService srv)
            {
                Chunk chunk;
                // TODO: should try to load from local first
                chunk = new Chunk(_chunkPosition, _world);
                // Add addToWorldTask
                srv.TaskDispatcher.Add(new AddToWorldTask(_world.Id, chunk));
            }

            private World _world;
            private Vec3<int> _chunkPosition;
        }

        private class RpcGetChunkTask : IReadOnlyTask
        {
            /**
            * \brief Given a chunk, it will try to use RPC to load it
            * \param world the target world
            * \param chunkPosition the position of the chunk
            * \note It will be used when the chunk service is not the authority.
            *       Usually when it's the client of a multiplayer session.
            */
            public RpcGetChunkTask(World world, Vec3<int> chunkPosition)
            {
                _world = world;
                _chunkPosition = chunkPosition;
            }

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            public void Task(ChunkService cs)
            {
                /*var data = context.rpc.getClient()
                    .call("getChunk", mWorld.Id, mChunkPosition)
                    .as<std::vector < uint32_t >> ();
                var chunk = new Chunk(mChunkPosition, mWorld, data));
                // Add addToWorldTask
                cs.TaskDispatcher.AddReadWriteTask(new AddToWorldTask(mWorld.Id, chunk));*/
            }

            private World _world;
            private Vec3<int> _chunkPosition;
        }

        private class LoadUnloadDetectorTask : IReadOnlyTask
        {
            public LoadUnloadDetectorTask(World world, Player player)
            {
                _player = player;
                _world = world;
            }

            public void Task(ChunkService cs)
            {
                var list = new List<int>();
                list.GetEnumerator();
                var loadList = new OrderedListIntLess<Vec3<int>>(MaxChunkLoadCount);
                var unloadList = new OrderedListIntGreater<Chunk>(MaxChunkUnloadCount);
                var playerPos = _player.Position;
                var position = new Vec3<int>((int) playerPos.X, (int) playerPos.Y, (int) playerPos.Z);
                GenerateLoadUnloadList(_world, position,
                    4, //getJsonValue<uint>(getSettings()["server"]["load_distance"], 4),
                    loadList, unloadList);

                foreach (var loadPos in loadList)
                {
                    if (cs.IsAuthority)
                    {
                        cs.TaskDispatcher.Add(new BuildOrLoadChunkTask(_world, loadPos.Value));
                    }
                    else
                    {
                        cs.TaskDispatcher.Add(new RpcGetChunkTask(_world, loadPos.Value));
                    }
                }

                foreach (var unloadChunk in unloadList)
                {
                    // add a unload task.
                    cs.TaskDispatcher.Add(new UnloadChunkTask(_world, unloadChunk.Value.Position));
                }
            }

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            private Player _player;
            private World _world;
        }

        public void RegisterChunkTasks(ChunkService cs, Player player) =>
            cs.TaskDispatcher.AddRegular(new LoadUnloadDetectorTask(this, player));
    }
}