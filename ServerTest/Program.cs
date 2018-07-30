// 
// ServerTest: Program.cs
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

using System.Net.Sockets;
using Core.Network;
using MsgPack.Serialization;

namespace ServerTest
{
    public static class Test
    {
        public class Server : FixedLengthProtocol
        {
            public Server() : base(Size)
            {
            }

            protected override void HandleRequestData(byte[] data, NetworkStream stream)
            {
                var request = SerialSend.UnpackSingleObject(data);
                Send(stream, Reply(request, SerialReply.PackSingleObjectAsBytes("Hello World for Network Test!")));
            }

            public override string Name() => "Test";
        }

        private static readonly MessagePackSerializer<int> SerialSend = MessagePackSerializer.Get<int>();
        private static readonly MessagePackSerializer<string> SerialReply = MessagePackSerializer.Get<string>();
        private static readonly int Size = SerialSend.PackSingleObject(0).Length;
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            var server = new Server(6369);
            server.RegisterProtocol(new Test.Server());
            server.Run();
        }
    }
}