// 
// Core: Protocols.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace Core.Network
{
    public static class Handshake
    {
        internal static async Task<KeyValuePair<string, uint>[]> Get(Session conn)
        {
            var session = Reply.AllocSession();
            using (var message = conn.CreateMessage(1))
            {
                message.Write(session.Key);
            }

            var result = await session.Value;
            return MessagePackSerializer.Deserialize<KeyValuePair<string, uint>[]>(result);
        }

        public class Server : FixedLengthProtocol
        {
            private readonly List<Protocol> protocols;

            public Server(List<Protocol> protocols) : base(4)
            {
                this.protocols = protocols;
            }

            public override void HandleRequest(Session.Receive request)
            {
                var session = request.ReadU32();
                var current = 0;
                var reply = new KeyValuePair<string, uint>[protocols.Count];
                foreach (var protocol in protocols)
                    reply[current++] = new KeyValuePair<string, uint>(protocol.Name(), protocol.Id);
                Reply.Send(request.Session, session, MessagePackSerializer.SerializeUnsafe(reply));
            }

            public override string Name()
            {
                return "FetchProtocols";
            }
        }

        public class Client : StubProtocol
        {
            public override string Name()
            {
                return "FetchProtocols";
            }
        }
    }

    public sealed class Reply : Protocol
    {
        private static int _idTop;
        private static readonly ConcurrentQueue<uint> SessionIds = new ConcurrentQueue<uint>();

        private static readonly ConcurrentDictionary<uint, TaskCompletionSource<byte[]>> Sessions =
            new ConcurrentDictionary<uint, TaskCompletionSource<byte[]>>();

        public override string Name()
        {
            return "Reply";
        }

        public override void HandleRequest(Session.Receive request)
        {
            var session = request.ReadU32();
            var length = request.ReadU32();
            var dataSegment = new byte[length];
            request.Read(dataSegment, 0, dataSegment.Length);
            SessionDispatch(session, dataSegment);
        }

        public static void Send(Session dialog, uint session, ArraySegment<byte> payload)
        {
            using (var message = dialog.CreateMessage(0))
            {
                message.Write(session);
                message.Write((uint) payload.Count);
                message.Write(payload);
            }
        }

        public static KeyValuePair<uint, Task<byte[]>> AllocSession()
        {
            if (!SessionIds.TryDequeue(out var newId))
                newId = (uint) (Interlocked.Increment(ref _idTop) - 1);

            var completionSource = new TaskCompletionSource<byte[]>();
            while (!Sessions.TryAdd(newId, completionSource)) ;
            return new KeyValuePair<uint, Task<byte[]>>(newId, completionSource.Task);
        }

        private static void SessionDispatch(uint sessionId, byte[] dataSegment)
        {
            TaskCompletionSource<byte[]> completion;
            while (!Sessions.TryRemove(sessionId, out completion)) ;
            completion.SetResult(dataSegment);
            SessionIds.Enqueue(sessionId);
        }

        private static int GetSessionId(byte[] head)
        {
            return (head[0] << 24) | (head[1] << 16) | (head[2] << 8) | head[3];
        }

        private static int GetSessionLength(byte[] head)
        {
            return (head[4] << 24) | (head[5] << 16) | (head[6] << 8) | head[7];
        }
    }
}