// 
// ClientTest: Program.cs
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
using Core;
using Core.Network;
using MsgPack.Serialization;

namespace ClientTest
{
    public static class Test
    {
        public class ClientStub : StubProtocol
        {
            public string Get(ConnectionHost.Connection conn)
            {
                var session = Singleton<ProtocolReply>.Instance.AllocReplySession();
                Send(conn.Stream, Request(Id, SerialSend.PackSingleObjectAsBytes(session.Key)));
                return SerialReply.UnpackSingleObject(session.Value.Result);
            }

            public override string Name() => "Test";
        }

        private static readonly MessagePackSerializer<int> SerialSend = MessagePackSerializer.Get<int>();
        private static readonly MessagePackSerializer<string> SerialReply = MessagePackSerializer.Get<string>();
    }

    internal class Program
    {
        private static void OneClient()
        {
            var client = new Client("localhost", 6369);
            var testProtocol = new Test.ClientStub();
            client.RegisterProtocol(testProtocol);
            client.NegotiateProtocols();
            long count = 0;
            DateTime time = DateTime.Now;
            while (true)
            {
                testProtocol.Get(client.GetConnection());
                if (++count % 10000 == 0)
                {
                    System.Console.WriteLine($"{(double) count / (DateTime.Now - time).Seconds} Calls Per Second");
                }
            }

            client.Close();
        }

        public static void Main(string[] args)
        {
            OneClient();
            System.Console.ReadKey();
        }
    }
}