// 
// NEWorld/NEWorldShell: Command.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;

namespace NEWorldServer
{
    public class CommandExecuteStat
    {
        public readonly string Info;

        public bool Success;

        public CommandExecuteStat(bool s, string i)
        {
            Success = s;
            Info = i;
        }
    }

    public class Command
    {
        public readonly List<string> Args;

        public readonly string Name;

        public Command(string rawString)
        {
            Args = rawString.Split(' ').ToList();
            Name = Args.Count != 0 ? Args[0] : "";
            if (Args.Count != 0) Args.RemoveAt(0);
        }
    }

    public class CommandInfo
    {
        public string Author;
        public string Help;

        public CommandInfo(string a, string h)
        {
            Author = a;
            Help = h;
        }
    }


    public class CommandManager
    {
        public delegate CommandExecuteStat CommandHandleFunction(Command exec);

        private readonly Dictionary<string, KeyValuePair<CommandInfo, CommandHandleFunction>> _commandMap;

        private readonly Thread _mainloop;

        private bool _threadRunning = true;

        private bool _waitingForInput;

        public CommandManager()
        {
            _commandMap = new Dictionary<string, KeyValuePair<CommandInfo, CommandHandleFunction>>();
            _mainloop = new Thread(InputLoop);
            _mainloop.Start();
        }

        ~CommandManager()
        {
            _threadRunning = false;
            if (!_waitingForInput)
                _mainloop.Join();
            else
                _mainloop.Abort();
        }

        public void InputLoop()
        {
            while (_threadRunning)
            {
                _waitingForInput = true;
                var input = Console.ReadLine();
                _waitingForInput = false;
                var result = HandleCommand(new Command(input));
                if (result.Info != "")
                    LogPort.Debug(result.Info);
            }
        }

        public Dictionary<string, KeyValuePair<CommandInfo, CommandHandleFunction>> GetCommandMap()
        {
            return _commandMap;
        }

        public void SetRunningStatus(bool s)
        {
            _threadRunning = s;
        }

        public void RegisterCommand(string name, CommandInfo info, CommandHandleFunction func)
        {
            _commandMap.Add(name, new KeyValuePair<CommandInfo, CommandHandleFunction>(info, func));
        }

        private CommandExecuteStat HandleCommand(Command cmd)
        {
            var lower = cmd.Name.ToLower();
            return _commandMap.TryGetValue(lower, out var result)
                ? result.Value(cmd)
                : new CommandExecuteStat(false, "Command not exists, type help for available commands.");
        }
    }
}