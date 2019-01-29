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

using System;
using System.Threading.Tasks;
using Core;
using Core.Utilities;

namespace Game.Network
{
    [DeclareService("Game.Server")]
    public class Server : IDisposable
    {
        public void Enable(int port)
        {
            server = new Core.Network.Server(port);
            server.RegisterProtocol(new GetChunk.Server());
            server.RegisterProtocol(new GetAvailableWorldId.Server());
            server.RegisterProtocol(new GetWorldInfo.Server());
            Singleton<ChunkService>.Instance.Worlds.Add("test world");
        }

        public void Run()
        {
            wait = server.RunAsync();
        }

        public int CountConnections() => server.CountConnections();

        public void Stop()
        {
            server.StopServer();
            wait.Wait();
        }

        private Task wait;
        private Core.Network.Server server;

        public void Dispose()
        {
            Stop();
            wait?.Dispose();
        }
    }
}