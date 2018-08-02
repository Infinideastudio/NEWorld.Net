// 
// Game: Server.cs
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

using System.Threading.Tasks;
using Core.Utilities;

namespace Game.Network
{
    public class Server
    {
        public Server(int port)
        {
            _server = new Core.Network.Server(port);
            _server.RegisterProtocol(new GetChunk.Server());
            _server.RegisterProtocol(new GetAvailableWorldId.Server());
            _server.RegisterProtocol(new GetWorldInfo.Server());
            Singleton<ChunkService>.Instance.Worlds.Add("test world");
        }
        
        public void Run()
        {
            _wait = _server.RunAsync();
        }

        public void Stop()
        {
            _server.StopServer();
            _wait.Wait();
        }

        private Task _wait;
        private readonly Core.Network.Server _server;
    }
}