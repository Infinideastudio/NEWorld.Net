// 
// NEWorld/Core: AssemblyReflectiveScanner.cs
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

// IMPORTANT NOTICE: The Design and Implementation of this Functionality Assumes that there is
// and will only be ONE AppDomain Throughout the Entire Instance of The Program.
// This decision is made for the lack of functionality of the .Net Core and .Net Standard on 
// isolating and safety issues. All internal interfaces and implementations ARE SUBJECTED TO CHANGE
// in the future so DO NOT rely on them for any reason outside this assembly

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core
{
    public sealed class DeclareNeWorldAssemblyAttribute : Attribute
    {
        internal readonly AssemblyScanPolicy Policy;

        public DeclareNeWorldAssemblyAttribute(AssemblyScanPolicy policy = AssemblyScanPolicy.Default)
        {
            Policy = policy;
        }
    }

    public enum AssemblyScanPolicy
    {
        PublicOnly,
        All,
        Default = PublicOnly
    }

    public sealed class DeclareAssemblyReflectiveScannerAttribute : Attribute
    {
    }

    public interface IAssemblyReflectiveScanner
    {
        void ProcessType(Type type);
    }

    internal static class AssemblyReflectiveScanner
    {
        // Only for conflict resolve for multi-thread load
        private static HashSet<AssemblyName> _processed = new HashSet<AssemblyName>();
        private static readonly object ProcessLock = new object();
        private static readonly List<IAssemblyReflectiveScanner> Scanners = new List<IAssemblyReflectiveScanner>();
        private static readonly List<Assembly> Scanned = new List<Assembly>();

        internal static void UpdateDomainAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoadServiceRegisterAgent;
            var snapshot = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in snapshot)
            {
                if (!CheckIfAssemblyProcessed(assembly))
                {
                    ScanAssembly(assembly);
                }
            }

            UpdateDomainAssembliesProcessKeepUp();
        }

        private static void UpdateDomainAssembliesProcessKeepUp()
        {
            lock (ProcessLock)
            {
                _processed = null;
                lock (Scanned)
                {
                    foreach (var assembly in Scanned)
                    {
                        ProcessNewAssembly(assembly);
                    }
                }
            }
        }

        private static bool CheckIfAssemblyProcessed(Assembly assembly)
        {
            lock (ProcessLock)
            {
                return _processed != null && (bool) _processed?.Contains(assembly.GetName());
            }
        }

        private static void OnAssemblyLoadServiceRegisterAgent(object sender, AssemblyLoadEventArgs args)
        {
            ScanAssembly(args.LoadedAssembly);
        }

        private static void ScanForAssemblyScanners(Assembly assembly)
        {
            var allowPrivate = GetAssemblyScanPolicy(assembly) == AssemblyScanPolicy.All;
            foreach (var type in assembly.DefinedTypes)
            {
                if ((type.IsPublic || allowPrivate) && IsScannerType(type))
                {
                    InitializeScanner(type);
                }
            }
        }

        private static bool IsScannerType(Type type)
        {
            return type.IsDefined(typeof(DeclareAssemblyReflectiveScannerAttribute), false) &&
                   typeof(IAssemblyReflectiveScanner).IsAssignableFrom(type);
        }

        private static void InitializeScanner(Type type)
        {
            var currentScanner = (IAssemblyReflectiveScanner) Activator.CreateInstance(type);
            lock (Scanners)
            {
                Scanners.Add(currentScanner);
            }

            lock (ProcessLock)
            {
                if (_processed != null) return;
                ProcessPastAssemblies(currentScanner);
            }
        }

        private static void ProcessPastAssemblies(IAssemblyReflectiveScanner currentScanner)
        {
            lock (Scanned)
            {
                foreach (var assembly in Scanned)
                {
                    ProcessPastAssembly(currentScanner, assembly);
                }
            }
        }

        private static void ProcessPastAssembly(IAssemblyReflectiveScanner currentScanner, Assembly assembly)
        {
            var allowPrivate = GetAssemblyScanPolicy(assembly) == AssemblyScanPolicy.All;
            foreach (var target in assembly.DefinedTypes)
            {
                if (target.IsPublic || allowPrivate)
                {
                    currentScanner.ProcessType(target);
                }
            }
        }

        private static AssemblyScanPolicy GetAssemblyScanPolicy(Assembly assembly)
        {
            return assembly.GetCustomAttribute<DeclareNeWorldAssemblyAttribute>().Policy;
        }

        private static void ScanAssembly(Assembly assembly)
        {
            lock (ProcessLock)
            {
                _processed?.Add(assembly.GetName(true));
            }

            if (!assembly.IsDefined(typeof(DeclareNeWorldAssemblyAttribute), false)) return;

            ScanForAssemblyScanners(assembly);

            lock (Scanned)
            {
                Scanned.Add(assembly);
            }

            lock (ProcessLock)
            {
                if (_processed == null)
                {
                    ProcessNewAssembly(assembly);
                }
            }
        }

        private static void ProcessNewAssembly(Assembly assembly)
        {
            var allowPrivate = GetAssemblyScanPolicy(assembly) == AssemblyScanPolicy.All;
            foreach (var target in assembly.DefinedTypes)
            {
                if (target.IsPublic || allowPrivate)
                {
                    ProcessNewAssemblyType(target);
                }
            }
        }

        private static void ProcessNewAssemblyType(Type target)
        {
            lock (Scanners)
            {
                foreach (var currentScanner in Scanners)
                {
                    currentScanner.ProcessType(target);
                }
            }
        }
    }
}