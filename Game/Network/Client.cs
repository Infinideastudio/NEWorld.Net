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
using Core.Network;

namespace Game.Network
{
    [DeclareService("Game.Client")]
    public class Client : IDisposable
    {
        public static GetChunk.Client GetChunk;
        public static GetAvailableWorldId.Client GetAvailableWorldId;
        public static GetWorldInfo.Client GetWorldInfo;

        private static Core.Network.Client _client;

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task Enable(string address, int port)
        {
            _client = new Core.Network.Client(address, port);
            _client.RegisterProtocol(GetChunk = new GetChunk.Client());
            _client.RegisterProtocol(GetAvailableWorldId = new GetAvailableWorldId.Client());
            _client.RegisterProtocol(GetWorldInfo = new GetWorldInfo.Client());
            await _client.HandShake();
        }

        public static Session.Send CreateMessage(uint protocol)
        {
            return _client.CreateMessage(protocol);
        }

        public void Stop()
        {
            _client.Close();
        }
    }
}