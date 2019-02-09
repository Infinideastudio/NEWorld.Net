// 
// NEWorld/Core: Server.cs
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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core.Network
{
    public class Server : TcpListener
    {
        private readonly List<Protocol> protocols;

        public Server(int port) : base(IPAddress.Any, port)
        {
            protocols = new List<Protocol>();
            RegisterProtocol(new Reply());
            RegisterProtocol(new Handshake.Server(protocols));
        }

        public async Task RunAsync()
        {
            Boot();
            await ListenConnections();
            ShutDown();
        }

        public void RegisterProtocol(Protocol newProtocol)
        {
            protocols.Add(newProtocol);
        }

        public int CountConnections()
        {
            return ConnectionHost.CountConnections();
        }

        private void Boot()
        {
            AssignProtocolIdentifiers();
            Start();
        }

        public void ShutDown()
        {
            Stop();
        }

        private async Task ListenConnections()
        {
            while (Active)
                try
                {
                    ConnectionHost.Add(await AcceptTcpClientAsync(), protocols);
                }
                catch
                {
                    // ignored
                }
        }

        private void AssignProtocolIdentifiers()
        {
            var current = 0u;
            foreach (var protocol in protocols)
                protocol.Id = current++;
        }
    }
}