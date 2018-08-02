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

namespace Game.Network
{
    public class Client
    {
        public Client(string address, int port)
        {
            _client = new Core.Network.Client(address, port);
            _client.RegisterProtocol(GetChunk = new GetChunk.Client(_client.GetConnection()));
            _client.RegisterProtocol(GetAvailableWorldId = new GetAvailableWorldId.Client(_client.GetConnection()));
            _client.RegisterProtocol(GetWorldInfo = new GetWorldInfo.Client(_client.GetConnection()));
            _client.NegotiateProtocols();
        }

        public readonly GetChunk.Client GetChunk;
        public readonly GetAvailableWorldId.Client GetAvailableWorldId;
        public readonly GetWorldInfo.Client GetWorldInfo;
        public static Client ThisClient { get; private set; }

        public static void EnableClient(string address, int port) => ThisClient = new Client(address, port);

        public void Stop()
        {
            _client.Close();
        }

        private readonly Core.Network.Client _client;
    }
}