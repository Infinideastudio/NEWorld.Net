// 
// NEWorld/Game: Client.cs
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

using System;
using System.Threading.Tasks;
using Akarin;
using Akarin.Network;

namespace Game.Network
{
    [DeclareService("Game.Client")]
    public class Client : IDisposable
    {
        public static GetChunk.Invoke GetChunk = new GetChunk.Invoke();
        public static GetAvailableWorldId.Invoke GetAvailableWorldId = new GetAvailableWorldId.Invoke();
        public static GetWorldInfo.Invoke GetWorldInfo = new GetWorldInfo.Invoke();
        public static GetStaticChunkIds.Invoke GetStaticChunkIds = new GetStaticChunkIds.Invoke();

        private static Akarin.Network.Client _client;

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task Enable(string address, int port)
        {
            _client = await Akarin.Network.Client.CreateClient(new ClientCreateInfo
            {
                Address = address, Port = port,
                HandshakeGroup = new Handshake(),
                ProtocolGroups = new []{"NEWorld.Core"}
            });
            _client.BindStaticInvoke(GetChunk);
            _client.BindStaticInvoke(GetAvailableWorldId);
            _client.BindStaticInvoke(GetWorldInfo);
            _client.BindStaticInvoke(GetStaticChunkIds);
        }

        public void Stop()
        {
            _client.Close();
        }
    }
}