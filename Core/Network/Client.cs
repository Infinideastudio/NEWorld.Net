// 
// Core: Client.cs
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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Core.Network
{
    public sealed class Client : IDisposable
    {
        private readonly ConnectionHost.Connection connection;
        private readonly List<Protocol> protocols;

        public Client(string address, int port)
        {
            var client = new TcpClient(address, port);
            protocols = new List<Protocol>();
            RegisterProtocol(new Reply());
            RegisterProtocol(new Handshake.Client());
            connection = ConnectionHost.Add(client, protocols);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~Client()
        {
            ReleaseUnmanagedResources();
        }

        public void RegisterProtocol(Protocol newProtocol)
        {
            protocols.Add(newProtocol);
        }

        public async Task HandShake()
        {
            var skvm = new Dictionary<string, Protocol>();
            foreach (var protocol in protocols)
                skvm.Add(protocol.Name(), protocol);
            var reply = await Handshake.Get(GetConnection().Session);
            foreach (var entry in reply)
                skvm[entry.Key].Id = entry.Value;
            protocols.Sort(ProtocolSorter);
        }

        public Session.Send CreateMessage(uint protocol)
        {
            return GetConnection().Session.CreateMessage(protocol);
        }

        public void Close()
        {
            connection.Close();
        }

        public ConnectionHost.Connection GetConnection()
        {
            return connection;
        }

        private static int ProtocolSorter(Protocol x, Protocol y)
        {
            return Comparer<uint>.Default.Compare(x.Id, y.Id);
        }

        private void ReleaseUnmanagedResources()
        {
            Close();
        }
    }
}