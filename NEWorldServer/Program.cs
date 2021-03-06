﻿// 
// NEWorld/NEWorldShell: Program.cs
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

using Core;
using Game;
using Game.Network;

namespace NEWorldServer
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            ApplicationControl.DoLaunch();
            Modules.Load("Main");
            var cli = new ServerCommandLine();
            var server = Services.Get<Server>("Game.Server");
            server.Enable(31111);
            EventBus.Broadcast(null, new GameLoadEvent());
            server.Run();
            cli.Start();
            EventBus.Broadcast(null, new GameUnloadEvent());
            ApplicationControl.DoShutdown();
        }
    }
}