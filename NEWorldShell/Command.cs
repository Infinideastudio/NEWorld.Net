// 
// NEWorldShell: Command.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NEWorldShell
{
    public class CommandExecuteStat
    {
        public CommandExecuteStat(bool s, string i)
        {
            Success = s;
            Info = i;
        }

        public bool Success;
        public readonly string Info;
    }

    public class Command
    {
        public Command(string rawString)
        {
            Args = rawString.Split(' ').ToList();
            Name = Args.Count != 0 ? Args[0] : "";
            if (Args.Count != 0) Args.RemoveAt(0);
        }

        public readonly string Name;
        public readonly List<string> Args;
    }

    public class CommandInfo
    {
        public CommandInfo(string a, string h)
        {
            Author = a;
            Help = h;
        }

        public string Author;
        public string Help;
    }


    public class CommandManager
    {
        public delegate CommandExecuteStat CommandHandleFunction(Command exec);

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
            {
                _mainloop.Join();
            }
            else
            {
                _mainloop.Abort();
            }
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
                    Core.LogPort.Debug(result.Info);
            }
        }

        public Dictionary<string, KeyValuePair<CommandInfo, CommandHandleFunction>> GetCommandMap() => _commandMap;

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

        private readonly Thread _mainloop;

        private bool _threadRunning = true;

        private bool _waitingForInput;

        private readonly Dictionary<string, KeyValuePair<CommandInfo, CommandHandleFunction>> _commandMap;
    }
}