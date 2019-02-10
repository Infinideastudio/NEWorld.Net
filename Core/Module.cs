// 
// NEWorld/Core: Module.cs
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
using System.Reflection;

namespace Core
{
    public interface IModule
    {
        void CoInitialize();
        void CoFinalize();
        void OnMemoryWarning();
    }

    public sealed class DeclareModuleAttribute : Attribute
    {
    }

    [DeclareAssemblyReflectiveScanner]
    [DeclareGlobalBusEventHandlerClass]
    public sealed class Modules : IAssemblyReflectiveScanner
    {
        private static readonly Dictionary<string, IModule> Loaded = new Dictionary<string, IModule>();

        private static string _basePath = AppContext.BaseDirectory;
        
        public static void SetBasePath(string path)
        {
            _basePath = path;
        }

        public static void Load(string moduleFile)
        {
            Assembly.Load(moduleFile);
        }
        
        [DeclareBusEventHandler]
        public static void UnloadAll(object sender, ApplicationControl.Shutdown type)
        {
            lock (Loaded)
            {
                foreach (var module in Loaded)
                    module.Value.CoFinalize();
                Loaded.Clear();
            }
        }

        public void ProcessType(Type type)
        {
            if (type.IsDefined(typeof(DeclareModuleAttribute), false) && typeof(IModule).IsAssignableFrom(type))
                try
                {
                    var module = (IModule) Activator.CreateInstance(type);
                    module.CoInitialize();
                    lock (Loaded)
                    {
                        Loaded.Add(type.FullName ?? "", module);
                    }
                    LogPort.Debug($"Loaded Module : {type}");
                }
                catch (Exception e)
                {
                    LogPort.Debug($"Module {type} Load Failure : {e}");
                }
        }
    }
}