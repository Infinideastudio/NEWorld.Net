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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities;
using MsgPack.Serialization;

namespace Core.Network
{
    public static class ProtocolFetchProtocol
    {
        public class Server : FixedLengthProtocol
        {
            public Server(List<Protocol> protocols) : base(Size) => _protocols = protocols;

            protected override void HandleRequestData(byte[] data, NetworkStream stream)
            {
                var request = SerialSend.UnpackSingleObject(data);
                var current = 0;
                var reply = new KeyValuePair<string, int>[_protocols.Count];
                foreach (var prot in _protocols)
                    reply[current++] = new KeyValuePair<string, int>(prot.Name(), prot.Id);
                Send(stream, Reply(request, SerialReply.PackSingleObjectAsBytes(reply)));
            }

            public override string Name() => "FetchProtocols";

            private readonly List<Protocol> _protocols;
        }

        public class Client : StubProtocol
        {
            public override string Name() => "FetchProtocols";

            public static KeyValuePair<string, int>[] Get(ConnectionHost.Connection conn)
            {
                var session = ProtocolReply.AllocSession();
                Send(conn.Stream, Request(1, SerialSend.PackSingleObjectAsBytes(session.Key)));
                return SerialReply.UnpackSingleObject(session.Value.Result);
            }
        }

        private static readonly MessagePackSerializer<int> SerialSend = MessagePackSerializer.Get<int>();

        private static readonly MessagePackSerializer<KeyValuePair<string, int>[]> SerialReply =
            MessagePackSerializer.Get<KeyValuePair<string, int>[]>();

        private static readonly int Size = SerialSend.PackSingleObject(0).Length;
    }

    public sealed class ProtocolReply : Protocol
    {
        private ProtocolReply()
        {
        }

        public override string Name() => "Reply";

        public override void HandleRequest(NetworkStream nstream)
        {
            var extraHead = new byte[8];
            nstream.Read(extraHead, 0, extraHead.Length);
            var dataSegment = new byte[GetSessionLength(extraHead)];
            nstream.Read(dataSegment, 0, dataSegment.Length);
            SessionDispatch(GetSessionId(extraHead), dataSegment);
        }

        public static KeyValuePair<int, Task<byte[]>> AllocSession() =>
            Singleton<ProtocolReply>.Instance.AllocSessionInternal();

        private KeyValuePair<int, Task<byte[]>> AllocSessionInternal()
        {
            if (!_sessionIds.TryDequeue(out var newId))
                newId = Interlocked.Increment(ref _idTop) - 1;

            var completionSource = new TaskCompletionSource<byte[]>();
            while (!_sessions.TryAdd(newId, completionSource)) ;
            return new KeyValuePair<int, Task<byte[]>>(newId, completionSource.Task);
        }

        private void SessionDispatch(int sessionId, byte[] dataSegment)
        {
            TaskCompletionSource<byte[]> completion;
            while (!_sessions.TryRemove(sessionId, out completion)) ;
            completion.SetResult(dataSegment);
            _sessionIds.Enqueue(sessionId);
        }

        private int _idTop;
        private readonly ConcurrentQueue<int> _sessionIds = new ConcurrentQueue<int>();

        private readonly ConcurrentDictionary<int, TaskCompletionSource<byte[]>> _sessions =
            new ConcurrentDictionary<int, TaskCompletionSource<byte[]>>();

        private static int GetSessionId(byte[] head) => head[0] << 24 | head[1] << 16 | head[2] << 8 | head[3];

        private static int GetSessionLength(byte[] head) => head[4] << 24 | head[5] << 16 | head[6] << 8 | head[7];
    }
}