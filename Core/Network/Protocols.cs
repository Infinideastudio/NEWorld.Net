// 
// NEWorld/Core: Protocols.cs
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

using System.Collections.Generic;
using System.Threading.Tasks;

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

            return (await session.Value).Get<KeyValuePair<string, uint>[]>();
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
                var session = request.ReadUInt32();
                var current = 0;
                var reply = new KeyValuePair<string, uint>[protocols.Count];
                foreach (var protocol in protocols)
                    reply[current++] = new KeyValuePair<string, uint>(protocol.Name(), protocol.Id);
                Reply.Send(request.Session, session, reply);
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
}