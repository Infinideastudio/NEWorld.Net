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

using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core.Utilities;

namespace Core.Network
{
    public class Client : TcpClient
    {
        public Client(string address, int port) : base(address, port)
        {
            RegisterProtocol(Singleton<ProtocolReply>.Instance);
            RegisterProtocol(new ProtocolFetchProtocol.Client());
            connHost.AddConnection(this);
        }

        public void RegisterProtocol(Protocol newProtocol) => connHost.RegisterProtocol(newProtocol);

        public async Task NegotiateProtocols()
        {
            var skvm = new Dictionary<string, Protocol>();
            foreach (var protocol in connHost.Protocols)
                skvm.Add(protocol.Name(), protocol);
            var reply = await ProtocolFetchProtocol.Client.Get(GetConnection());
            foreach (var entry in reply)
                skvm[entry.Key].Id = entry.Value;
            connHost.Protocols.Sort(ProtocolSorter);
        }

        public ConnectionHost.Connection GetConnection() => connHost.GetConnection(0);

        public new void Close() => connHost.CloseAll();

        private static int ProtocolSorter(Protocol x, Protocol y) => Comparer<int>.Default.Compare(x.Id, y.Id);

        private readonly ConnectionHost connHost = new ConnectionHost();
    }
}