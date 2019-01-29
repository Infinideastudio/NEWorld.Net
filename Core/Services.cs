// 
// Core: Services.cs
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

namespace Core
{
    public sealed class DeclareServiceAttribute : Attribute
    {
        public DeclareServiceAttribute(string name) => Name = name;

        public readonly string Name;
    }

    public sealed class ServiceDependencyAttribute : Attribute
    {
        public ServiceDependencyAttribute(params string[] dependencies) => Dependencies = dependencies;

        public readonly string[] Dependencies;
    }

    [Serializable]
    public class ServiceManagerException : Exception
    {
        public ServiceManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public static class Services
    {
        private class DisposeList
        {
            ~DisposeList()
            {
                foreach (var disposable in List)
                    disposable.Dispose();
            }

            public readonly List<IDisposable> List = new List<IDisposable>();
        }

        static Services() => ScanAssembly(Assembly.GetExecutingAssembly());

        public static void Inject<TP>() where TP : IDisposable => Inject(typeof(TP));

        public static TI Get<TI>(string name)
        {
            try
            {
                if (!typeof(TI).IsAssignableFrom(Providers[name]))
                    throw new Exception($"Service Does Not Provide Interface {typeof(TI)}");
                if (!Ready.TryGetValue(name, out var service))
                    service = CreateService(name);
                return (TI) service;
            }
            catch (Exception e)
            {
                throw new ServiceManagerException("Cannot Create Service Instance", e);
            }
        }

        public static bool TryGet<TI>(string name, out TI ins)
        {
            try
            {
                ins = Get<TI>(name);
                return true;
            }
            catch (ServiceManagerException)
            {
                ins = default(TI);
                return false;
            }
        }

        public static void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsDefined(typeof(DeclareServiceAttribute), false))
                    Inject(type);
            }
        }

        private static void Inject(Type tp)
        {
            var name = tp.GetCustomAttribute<DeclareServiceAttribute>().Name;
            var dependents = tp.IsDefined(typeof(ServiceDependencyAttribute), false)
                ? tp.GetCustomAttribute<ServiceDependencyAttribute>().Dependencies
                : new string[0];
            Providers.Add(name, tp);
            Dependencies.Add(name, dependents);
        }

        private static object CreateService(string name)
        {
            foreach (var dependent in Dependencies[name])
                CreateService(dependent);
            var provider = Providers[name];
            var instance = Activator.CreateInstance(provider);
            Dispose.List.Add((IDisposable) instance);
            Ready.Add(name, instance);
            return instance;
        }

        private static readonly DisposeList Dispose = new DisposeList();
        private static readonly Dictionary<string, object> Ready = new Dictionary<string, object>();
        private static readonly Dictionary<string, Type> Providers = new Dictionary<string, Type>();
        private static readonly Dictionary<string, string[]> Dependencies = new Dictionary<string, string[]>();
    }
}