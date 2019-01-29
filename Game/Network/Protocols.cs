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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Network;
using Core.Utilities;
using Game.World;
using MsgPack.Serialization;
using Xenko.Core.Mathematics;

namespace Game.Network
{
    public static class GetChunk
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(Size)
            {
            }

            public override string Name() => "GetChunk";

            protected override void HandleRequestData(byte[] data, NetworkStream stream)
            {
                var request = From.UnpackSingleObject(data);
                var chunkData = Get((uint) request[0], new Int3(request[1], request[2], request[3]));
                Send(stream, Request(Id));
                Send(stream, data);
                Send(stream, chunkData);
            }

            private static readonly ThreadLocal<byte[]> LocalMemCache = new ThreadLocal<byte[]>();

            private static byte[] Get(uint worldId, Int3 position)
            {
                // TODO: empty chunk optimization
                var world = Singleton<ChunkService>.Instance.Worlds.Get(worldId);
                Chunk chunkPtr;
                try
                {
                    chunkPtr = world.GetChunk(ref position);
                }
                catch (KeyNotFoundException)
                {
                    var chunk = new Chunk(position, world);
                    chunkPtr = world.InsertChunkAndUpdate(position, chunk);
                }

                var chunkData = LocalMemCache.Value ?? (LocalMemCache.Value = new byte[32768 * 4]);
                for (var i = 0; i < 32768 * 4; ++i)
                {
                    var block = chunkPtr.Blocks[i >> 2];
                    chunkData[i++] = (byte) (block.Id >> 4);
                    chunkData[i++] = (byte) ((block.Id << 4) | block.Brightness);
                    chunkData[i++] = (byte) (block.Data >> 8);
                    chunkData[i] = (byte) block.Data;
                }

                return chunkData;
            }
        }

        public class Client : FixedLengthProtocol
        {
            public override string Name() => "GetChunk";

            public Client(ConnectionHost.Connection conn) : base(32768 * 4 + Size) => stream = conn.Stream;

            protected override void HandleRequestData(byte[] data, NetworkStream stream)
            {
                var req = From.UnpackSingleObject(data);
                var srv = Singleton<ChunkService>.Instance;
                var chk = new Chunk(new Int3(req[1], req[2], req[3]), srv.Worlds.Get((uint) req[0]));
                for (var i = Size; i < 32768 * 4 + Size; i += 4)
                {
                    ref var block = ref chk.Blocks[(i - Size) >> 2];
                    block.Id = (ushort) (data[i] << 4 | data[i + 1] >> 4);
                    block.Brightness = (byte) (data[i + 1] | 0xF);
                    block.Data = (uint) (data[i + 2] << 8 | data[i + 3]);
                }

                srv.TaskDispatcher.Add(new World.World.ResetChunkTask((uint) req[0], chk));
            }

            public void Call(uint worldId, Int3 position)
            {
                var data = new[] {(int) worldId, position.X, position.Y, position.Z};
                Send(stream, Request(Id, From.PackSingleObjectAsBytes(data)));
            }

            private readonly NetworkStream stream;
        }

        private static readonly MessagePackSerializer<int[]> From = MessagePackSerializer.Get<int[]>();
        private static readonly int Size = From.PackSingleObject(new int[4]).Length;
    }

    public static class GetAvailableWorldId
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(Size)
            {
            }

            protected override void HandleRequestData(byte[] data, NetworkStream stream)
            {
                var request = SerialSend.UnpackSingleObject(data);
                Send(stream, Reply(request, SerialReply.PackSingleObjectAsBytes(new uint[] {0})));
            }

            public override string Name() => "GetAvailableWorldId";
        }

        public class Client : StubProtocol
        {
            public Client(ConnectionHost.Connection conn) => stream = conn.Stream;

            public override string Name() => "GetAvailableWorldId";

            public async Task<uint[]> Call()
            {
                var session = ProtocolReply.AllocSession();
                Send(stream, Request(Id, SerialSend.PackSingleObjectAsBytes(session.Key)));
                var result = await session.Value;
                return SerialReply.UnpackSingleObject(result);
            }

            private readonly NetworkStream stream;
        }

        private static readonly MessagePackSerializer<int> SerialSend = MessagePackSerializer.Get<int>();

        private static readonly MessagePackSerializer<uint[]> SerialReply = MessagePackSerializer.Get<uint[]>();

        private static readonly int Size = SerialSend.PackSingleObject(0).Length;
    }

    public static class GetWorldInfo
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(Size)
            {
            }

            protected override void HandleRequestData(byte[] data, NetworkStream stream)
            {
                var ret = new Dictionary<string, string>();
                var request = SerialSend.UnpackSingleObject(data);
                var world = Singleton<ChunkService>.Instance.Worlds.Get((uint) request[1]);
                ret.Add("name", world.Name);
                Send(stream, Reply(request[0], SerialReply.PackSingleObjectAsBytes(ret)));
            }

            public override string Name() => "GetWorldInfo";
        }

        public class Client : StubProtocol
        {
            public Client(ConnectionHost.Connection conn) => stream = conn.Stream;

            public override string Name() => "GetWorldInfo";

            public async Task<Dictionary<string, string>> Call(uint wid)
            {
                var session = ProtocolReply.AllocSession();
                Send(stream, Request(Id, SerialSend.PackSingleObjectAsBytes(new[] {session.Key, (int) wid})));
                var result = await session.Value;
                return SerialReply.UnpackSingleObject(result);
            }

            private readonly NetworkStream stream;
        }

        private static readonly MessagePackSerializer<int[]> SerialSend = MessagePackSerializer.Get<int[]>();

        private static readonly MessagePackSerializer<Dictionary<string, string>> SerialReply =
            MessagePackSerializer.Get<Dictionary<string, string>>();

        private static readonly int Size = SerialSend.PackSingleObject(new int[2]).Length;
    }
}