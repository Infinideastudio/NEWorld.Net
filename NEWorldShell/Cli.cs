// 
// NEWorldShell: Cli.cs
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
using System.Runtime;
using Core;
using Core.Utilities;
using Game;
using Game.Network;

namespace NEWorldShell
{
    internal class ServerCommandLine
    {
        public ServerCommandLine()
        {
            _commands = new CommandManager();
            InitBuiltinCommands();
        }

        public void Start()
        {
            _commands.InputLoop();
        }

        private void InitBuiltinCommands()
        {
            _commands.RegisterCommand("help", new CommandInfo("internal", "Help"),
                cmd =>
                {
                    var helpString = "\nAvailable commands:\n";
                    foreach (var command in _commands.GetCommandMap())
                    {
                        helpString += command.Key + " - " + command.Value.Key.Author
                                      + " : " + command.Value.Key.Help + "\n";
                    }

                    return new CommandExecuteStat(true, helpString);
                });

            _commands.RegisterCommand("server.stop", new CommandInfo("internal", "Stop the server."),
                cmd =>
                {
                    Services.Get<Server>("Game.Server").Stop();
                    LogPort.Debug("Server RPC stopped.");
                    _commands.SetRunningStatus(false);
                    return new CommandExecuteStat(true, "");
                });

            _commands.RegisterCommand("server.ups", new CommandInfo("internal", "Show the ups."),
                cmd =>
                {
                    // TODO: Add UPS counter for server
                    return new CommandExecuteStat(true, "[Server UPS counter not finished yet!]");
                });

            _commands.RegisterCommand("server.connections", new CommandInfo("internal", "Count Connections."),
                cmd =>
                {
                    LogPort.Debug($"{Services.Get<Server>("Game.Server").CountConnections()}");
                    return new CommandExecuteStat(true, "");
                });

            _commands.RegisterCommand("chunks.count",
                new CommandInfo("internal", "Show how many chunks are loaded"),
                cmd =>
                {
                    var ret = "Chunks loaded: ";
                    long sum = 0;
                    var worlds = Singleton<ChunkService>.Instance.Worlds;
                    foreach (var world in worlds)
                    {
                        ret += $"\n{world.Id} {world.Name} :\t{world.GetChunkCount()}";
                        sum += world.GetChunkCount();
                    }

                    return new CommandExecuteStat(true,
                        ret + $"\nTotal: {worlds.Count} worlds loaded, {sum} chunks loaded");
                });

            _commands.RegisterCommand("system.gc", new CommandInfo("internal", "Collect Garbage"),
                cmd =>
                {
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                    GC.WaitForFullGCComplete();
                    LogPort.Debug("GC Completed");
                    return new CommandExecuteStat(true, "");
                });
        }

        private readonly CommandManager _commands;
    }
}