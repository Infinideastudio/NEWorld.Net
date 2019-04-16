// 
// NEWorld/Core: ProtocolReply.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace Core.Network
{
    public sealed class Reply : Protocol
    {
        private static int _idTop;
        private static readonly ConcurrentQueue<uint> SessionIds = new ConcurrentQueue<uint>();

        private static readonly ConcurrentDictionary<uint, TaskCompletionSource<Payload>> Sessions =
            new ConcurrentDictionary<uint, TaskCompletionSource<Payload>>();

        public override string Name()
        {
            return "Reply";
        }

        public override void HandleRequest(Session.Receive request)
        {
            var session = request.ReadUInt32();
            var length = request.ReadUInt32();
            var dataSegment = new byte[length];
            request.Read(dataSegment, 0, dataSegment.Length);
            SessionDispatch(session, dataSegment);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Send<T>(Session dialog, uint session, T payload)
        {
            Send(dialog, session, MessagePackSerializer.SerializeUnsafe(payload));
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

        public static KeyValuePair<uint, Task<Payload>> AllocSession()
        {
            if (!SessionIds.TryDequeue(out var newId))
                newId = (uint) (Interlocked.Increment(ref _idTop) - 1);

            var completionSource = new TaskCompletionSource<Payload>();
            while (!Sessions.TryAdd(newId, completionSource))
            {
            }

            return new KeyValuePair<uint, Task<Payload>>(newId, completionSource.Task);
        }

        private static void SessionDispatch(uint sessionId, byte[] dataSegment)
        {
            TaskCompletionSource<Payload> completion;
            while (!Sessions.TryRemove(sessionId, out completion))
            {
            }

            completion.SetResult(new Payload {Raw = dataSegment});
            SessionIds.Enqueue(sessionId);
        }

        public struct Payload
        {
            public byte[] Raw;

            public T Get<T>()
            {
                return MessagePackSerializer.Deserialize<T>(Raw);
            }
        }
    }
}