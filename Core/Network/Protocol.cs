// 
// Core: Protocol.cs
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
using System.Net.Sockets;

namespace Core.Network
{
    public abstract class Protocol
    {
        public int Id { get; set; }

        public abstract string Name();

        public abstract void HandleRequest(NetworkStream nstream);

        protected static byte[] Request(int protocol, ArraySegment<byte> message) => Concat(message, new[]
        {
            (byte) 'N', (byte) 'W', (byte) 'R', (byte) 'C',
            (byte) (protocol >> 24),
            (byte) (protocol >> 16 & 0xFF),
            (byte) (protocol >> 18 & 0xFF),
            (byte) (protocol & 0xFF)
        });

        protected static byte[] Reply(int requestSession, ArraySegment<byte> message) => Concat(message, new[]
        {
            (byte) 'N', (byte) 'W', (byte) 'R', (byte) 'C',
            (byte) 0, (byte) 0, (byte) 0, (byte) 0,
            (byte) (requestSession >> 24),
            (byte) (requestSession >> 16 & 0xFF),
            (byte) (requestSession >> 18 & 0xFF),
            (byte) (requestSession & 0xFF),
            (byte) (message.Count >> 24),
            (byte) (message.Count >> 16 & 0xFF),
            (byte) (message.Count >> 18 & 0xFF),
            (byte) (message.Count & 0xFF)
        });

        protected static void Send(NetworkStream stream, byte[] data) => stream.Write(data, 0, data.Length);

        private static byte[] Concat(ArraySegment<byte> message, byte[] head)
        {
            var final = new byte[head.Length + message.Count];
            head.CopyTo(final, 0);
            Array.Copy(message.Array ?? throw new InvalidOperationException(), message.Offset, final, head.Length,
                message.Count);
            return final;
        }
    }

    public abstract class StandardProtocol : Protocol
    {
        protected abstract void HandleRequestData(byte[] data, NetworkStream stream);

        protected abstract byte[] PullRequestData(NetworkStream nstream);

        public sealed override void HandleRequest(NetworkStream nstream) =>
            HandleRequestData(PullRequestData(nstream), nstream);
    }

    public abstract class FixedLengthProtocol : StandardProtocol
    {
        protected FixedLengthProtocol(int length) => _packetLength = length;

        protected override byte[] PullRequestData(NetworkStream nstream)
        {
            var ret = new byte[_packetLength];
            nstream.Read(ret, 0, _packetLength);
            return ret;
        }

        private readonly int _packetLength;
    }

    public abstract class StubProtocol : Protocol
    {
        public override void HandleRequest(NetworkStream nstream)
        {
        }
    }
}