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
using Core;

namespace Game.Network
{
    [DeclareService("Game.Client")]
    public class Client : IDisposable
    {
        public void Enable(string address, int port)
        {
            _client = new Core.Network.Client(address, port);
            _client.RegisterProtocol(GetChunk = new GetChunk.Client(_client.GetConnection()));
            _client.RegisterProtocol(GetAvailableWorldId = new GetAvailableWorldId.Client(_client.GetConnection()));
            _client.RegisterProtocol(GetWorldInfo = new GetWorldInfo.Client(_client.GetConnection()));
            _client.NegotiateProtocols();
        }

        public static GetChunk.Client GetChunk;
        public static GetAvailableWorldId.Client GetAvailableWorldId;
        public static GetWorldInfo.Client GetWorldInfo;

        public void Stop()
        {
            _client.Close();
        }

        public void Dispose()
        {
            _client?.Close();
            _client?.Dispose();
        }

        private Core.Network.Client _client;
    }
}