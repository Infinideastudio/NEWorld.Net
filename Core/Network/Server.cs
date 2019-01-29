// 
// Core: Server.cs
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

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core.Utilities;

namespace Core.Network
{
    public class Server : TcpListener
    {
        public Server(int port) : base(IPAddress.Any, port)
        {
            RegisterProtocol(Singleton<ProtocolReply>.Instance);
            RegisterProtocol(new ProtocolFetchProtocol.Server(connHost.Protocols));
        }

        public void Run()
        {
            lock (connHost.Lock)
            {
                Boot();
                ListenConnections().Wait();
                ShutDown();
            }
        }

        public async Task RunAsync()
        {
            Boot();
            await ListenConnections();
            ShutDown();
        }

        public void RegisterProtocol(Protocol newProtocol) => connHost.RegisterProtocol(newProtocol);

        public void StopServer() => exit = true;

        public int CountConnections() => connHost.CountConnections();

        private void Boot()
        {
            exit = false;
            AssignProtocolIdentifiers();
            Start();
        }

        private void ShutDown() => Stop();

        private async Task ListenConnections()
        {
            while (!exit)
            {
                try
                {
                    connHost.AddConnection(await AcceptTcpClientAsync());
                }
                catch
                {
                    // ignored
                }

                connHost.SweepInvalidConnectionsIfNecessary();
            }

            connHost.CloseAll();
        }

        private void AssignProtocolIdentifiers()
        {
            var current = 0;
            foreach (var protocol in connHost.Protocols)
                protocol.Id = current++;
        }

        private bool exit;
        private readonly ConnectionHost connHost = new ConnectionHost();
    }
}