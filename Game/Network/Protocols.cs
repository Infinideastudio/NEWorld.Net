// 
// Game: Protocols.cs
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
using System.Threading;
using System.Threading.Tasks;
using Core.Network;
using Game.World;
using MessagePack;
using Xenko.Core.Mathematics;

namespace Game.Network
{
    public static class GetStaticChunkIds
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(4){}

            public override string Name()
            {
                return "GetStaticChunkId";
            }

            public override void HandleRequest(Session.Receive request)
            {
                var session = request.ReadU32();
                var ids = StaticChunkPool.Id;
                Reply.Send(request.Session, session, MessagePackSerializer.SerializeUnsafe(ids));
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

                var result = await session.Value;
                StaticChunkPool.Id = MessagePackSerializer.Deserialize<Dictionary<string, uint>>(result);
            }
        }
    }

    public static class GetChunk
    {
        private static readonly ThreadLocal<byte[]> LocalMemCache = new ThreadLocal<byte[]>();
        private static readonly int Size = MessagePackSerializer.SerializeUnsafe(new int[4]).Count;

        public class Server : FixedLengthProtocol
        {
            public Server() : base(Size)
            {
            }

            public override string Name()
            {
                return "GetChunk";
            }

            public override void HandleRequest(Session.Receive stream)
            {
                var request = MessagePackSerializer.Deserialize<int[]>(stream.Raw);
                var chunk = GetChunk((uint) request[0], new Int3(request[1], request[2], request[3]));
                using (var message = stream.Session.CreateMessage(Id))
                {
                    message.Write(stream.Raw, 0, Size);
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
                var chunkData = LocalMemCache.Value ?? (LocalMemCache.Value = new byte[32768 * 4]);
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
                var buffer = new byte[Size];
                request.Read(buffer, 0, Size);
                var req = MessagePackSerializer.Deserialize<int[]>(buffer);
                var cow = request.ReadU32();
                Chunk chk;
                var chunkPos = new Int3(req[1], req[2], req[3]);
                var world = ChunkService.Worlds.Get((uint) req[0]);
                if (cow == uint.MaxValue)
                {
                    var data = LocalMemCache.Value ?? (LocalMemCache.Value = new byte[32768 * 4]);
                    request.Read(data, 0, data.Length);
                    chk = DeserializeChunk(chunkPos, world, data);
                }
                else
                {
                    chk = new Chunk(chunkPos, world, cow);
                }

                ChunkService.TaskDispatcher.Add(new World.World.ResetChunkTask(chk));
            }

            private static Chunk DeserializeChunk(Int3 chunkPos, World.World world, byte[] data)
            {
                var chk = new Chunk(chunkPos, world, Chunk.InitOption.AllocateUnique);
                chk.DeserializeFrom(data);
                return chk;
            }

            public void Call(uint worldId, Int3 position)
            {
                var data = new[] {(int) worldId, position.X, position.Y, position.Z};
                using (var message = Network.Client.CreateMessage(Id))
                {
                    message.Write(MessagePackSerializer.SerializeUnsafe(data));
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
                var session = request.ReadU32();
                Reply.Send(request.Session, session, MessagePackSerializer.SerializeUnsafe(new uint[] {0}));
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

                var result = await session.Value;
                return MessagePackSerializer.Deserialize<uint[]>(result);
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
                var request = stream.ReadU32();
                var world = ChunkService.Worlds.Get(stream.ReadU32());
                var ret = new Dictionary<string, string> {{"name", world.Name}};
                Reply.Send(stream.Session, request, MessagePackSerializer.SerializeUnsafe(ret));
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

                var result = await session.Value;
                return MessagePackSerializer.Deserialize<Dictionary<string, string>>(result);
            }
        }
    }
}