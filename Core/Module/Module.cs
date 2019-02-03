﻿// 
// Core: Module.cs
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
using System.Reflection;
using Core.Utilities;

namespace Core.Module
{
    public interface IModule
    {
        void CoInitialize();
        void WorkspaceInitialize();
        void WorkspaceFinalize();
        void CoFinalize();
        void OnMemoryWarning();
    }

    public class DeclareModuleAttribute : Attribute
    {
    }

    public class Modules
    {
        private readonly Dictionary<string, KeyValuePair<IModule, Assembly>> modules;

        private string basePath = Path.Modules();

        private Modules()
        {
            modules = new Dictionary<string, KeyValuePair<IModule, Assembly>>();
        }

        public IModule this[string name] => modules[name].Key;

        public static Modules Instance => Singleton<Modules>.Instance;

        public void SetBasePath(string path)
        {
            basePath = path;
        }

        public void Load(string moduleFile)
        {
            var assembly = Assembly.Load(moduleFile);
            var possibleTypes = assembly.GetExportedTypes();
            foreach (var type in possibleTypes)
                if (type.IsDefined(typeof(DeclareModuleAttribute), false) && typeof(IModule).IsAssignableFrom(type))
                    try
                    {
                        var module = (IModule) Activator.CreateInstance(type);
                        module.CoInitialize();
                        modules.Add(type.FullName ?? "", new KeyValuePair<IModule, Assembly>(module, assembly));
                        LogPort.Debug($"Loaded Module : {type}");
                    }
                    catch (Exception e)
                    {
                        LogPort.Debug($"Module {type} Load Failure : {e}");
                    }
        }

        public void WorkspaceInitialize()
        {
            foreach (var module in modules)
                module.Value.Key.WorkspaceInitialize();
        }

        public void WorkspaceFinalize()
        {
            foreach (var module in modules)
                module.Value.Key.WorkspaceFinalize();
        }

        public void UnloadAll()
        {
            foreach (var module in modules)
                module.Value.Key.CoFinalize();
            modules.Clear();
        }
    }
}