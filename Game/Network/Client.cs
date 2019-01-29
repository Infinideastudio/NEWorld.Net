// 
// Game: Client.cs
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
using System.Threading.Tasks;
using Core;

namespace Game.Network
{
    [DeclareService("Game.Client")]
    public class Client : IDisposable
    {
        public async Task Enable(string address, int port)
        {
            client = new Core.Network.Client(address, port);
            client.RegisterProtocol(GetChunk = new GetChunk.Client(client.GetConnection()));
            client.RegisterProtocol(GetAvailableWorldId = new GetAvailableWorldId.Client(client.GetConnection()));
            client.RegisterProtocol(GetWorldInfo = new GetWorldInfo.Client(client.GetConnection()));
            await client.NegotiateProtocols();
        }

        public static GetChunk.Client GetChunk;
        public static GetAvailableWorldId.Client GetAvailableWorldId;
        public static GetWorldInfo.Client GetWorldInfo;

        public void Stop()
        {
            client.Close();
        }

        public void Dispose()
        {
            client?.Close();
            client?.Dispose();
        }

        private Core.Network.Client client;
    }
}