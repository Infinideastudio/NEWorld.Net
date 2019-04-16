// 
// NEWorld/Game: Protocols.cs
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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Network;
using Game.World;
using Xenko.Core.Mathematics;

namespace Game.Network
{
    public static class GetStaticChunkIds
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(4)
            {
            }

            public override string Name()
            {
                return "GetStaticChunkId";
            }

            public override void HandleRequest(Session.Receive request)
            {
                var session = request.ReadUInt32();
                var ids = StaticChunkPool.Id;
                Reply.Send(request.Session, session, ids);
            }
        }

        public class Client : StubProtocol
        {
            public override string Name()
            {
                return "GetStaticChunkId";
            }

            public async Task Call()
            {
                var session = Reply.AllocSession();
                using (var message = Network.Client.CreateMessage(Id))
                {
                    message.Write(session.Key);
                }

                StaticChunkPool.Id = (await session.Value).Get<Dictionary<string, uint>>();
            }
        }
    }

    public static class GetChunk
    {
        [ThreadStatic] private static byte[] _localMemCache;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] GetCache()
        {
            return  _localMemCache ?? (_localMemCache = new byte[32768 * 4]);
        }

        public class Server : Protocol
        {
            public override string Name()
            {
                return "GetChunk";
            }

            public override void HandleRequest(Session.Receive stream)
            {
                var request = stream.Read<int[]>();
                var chunk = GetChunk((uint) request[0], new Int3(request[1], request[2], request[3]));
                using (var message = stream.Session.CreateMessage(Id))
                {
                    message.WriteObject(request);
                    var cow = chunk.CopyOnWrite;
                    message.Write(cow);
                    if (cow == uint.MaxValue)
                    {
                        var chunkData = Get(chunk);
                        message.Write(chunkData, 0, chunkData.Length);
                    }
                }
            }

            private static byte[] Get(Chunk chunk)
            {
                var chunkData = GetCache();
                chunk.SerializeTo(chunkData);
                return chunkData;
            }

            private static Chunk GetChunk(uint worldId, Int3 position)
            {
                var world = ChunkService.Worlds.Get(worldId);
                Chunk chunkPtr;
                try
                {
                    chunkPtr = world.GetChunk(position);
                }
                catch (KeyNotFoundException)
                {
                    var chunk = new Chunk(position, world);
                    // TODO: Implement a WorldTask Instead
                    chunkPtr = world.InsertChunkAndUpdate(chunk);
                }

                return chunkPtr;
            }
        }

        public class Client : Protocol
        {
            public override string Name()
            {
                return "GetChunk";
            }

            public override void HandleRequest(Session.Receive request)
            {
                var req = request.Read<int[]>();
                var chunkPos = new Int3(req[1], req[2], req[3]);
                var world = ChunkService.Worlds.Get((uint) req[0]);
                ChunkService.TaskDispatcher.Add(
                    new World.World.ResetChunkTask(RequestExtractChunkContent(request, chunkPos, world))
                );
            }

            private static Chunk RequestExtractChunkContent(Session.Receive request, Int3 chunkPos, World.World world)
            {
                var cow = request.ReadUInt32();
                if (cow != uint.MaxValue) return new Chunk(chunkPos, world, cow);
                var data = GetCache();
                request.Read(data, 0, data.Length);
                return DeserializeChunk(chunkPos, world, data);
            }

            private static Chunk DeserializeChunk(Int3 chunkPos, World.World world, byte[] data)
            {
                var chk = new Chunk(chunkPos, world, Chunk.InitOption.AllocateUnique);
                chk.DeserializeFrom(data);
                return chk;
            }

            public void Call(uint worldId, Int3 position)
            {
                using (var message = Network.Client.CreateMessage(Id))
                {
                    message.WriteObject(new[] {(int) worldId, position.X, position.Y, position.Z});
                }
            }
        }
    }

    public static class GetAvailableWorldId
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(4)
            {
            }

            public override void HandleRequest(Session.Receive request)
            {
                var session = request.ReadUInt32();
                Reply.Send(request.Session, session, new uint[] {0});
            }

            public override string Name()
            {
                return "GetAvailableWorldId";
            }
        }

        public class Client : StubProtocol
        {
            public override string Name()
            {
                return "GetAvailableWorldId";
            }

            public async Task<uint[]> Call()
            {
                var session = Reply.AllocSession();
                using (var message = Network.Client.CreateMessage(Id))
                {
                    message.Write(session.Key);
                }

                return (await session.Value).Get<uint[]>();
            }
        }
    }

    public static class GetWorldInfo
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(8)
            {
            }

            public override void HandleRequest(Session.Receive stream)
            {
                var request = stream.ReadUInt32();
                var world = ChunkService.Worlds.Get(stream.ReadUInt32());
                var ret = new Dictionary<string, string> {{"name", world.Name}};
                Reply.Send(stream.Session, request, ret);
            }

            public override string Name()
            {
                return "GetWorldInfo";
            }
        }

        public class Client : StubProtocol
        {
            public override string Name()
            {
                return "GetWorldInfo";
            }

            public async Task<Dictionary<string, string>> Call(uint wid)
            {
                var session = Reply.AllocSession();
                using (var message = Network.Client.CreateMessage(Id))
                {
                    message.Write(session.Key);
                    message.Write(wid);
                }

                return (await session.Value).Get<Dictionary<string, string>>();
            }
        }
    }
}