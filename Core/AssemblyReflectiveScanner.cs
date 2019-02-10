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

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core
{
    public sealed class DeclareNeWorldAssemblyAttribute : Attribute
    {
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
                if (!CheckIfAssemblyProcessed(assembly))
                    ScanAssembly(assembly);

            lock (ProcessLock)
            {
                _processed = null;
                lock (Scanned)
                {
                    foreach (var assembly in Scanned) ProcessAssembly(assembly);
                }
            }
        }

        private static bool CheckIfAssemblyProcessed(Assembly assembly)
        {
            lock (ProcessLock)
            {
                return _processed != null && (bool) (_processed?.Contains(assembly.GetName()));
            }
        }

        private static void OnAssemblyLoadServiceRegisterAgent(object sender, AssemblyLoadEventArgs args)
        {
            ScanAssembly(args.LoadedAssembly);
        }

        private static void ScanForAssemblyScanners(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
                if (CheckScannerType(type))
                    InitializeScanner(type);
        }

        private static bool CheckScannerType(Type type)
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
                lock (Scanned)
                {
                    foreach (var assembly in Scanned)
                    foreach (var target in assembly.GetExportedTypes())
                        currentScanner.ProcessType(target);
                }
            }
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
                if (_processed == null) ProcessAssembly(assembly);
            }
        }

        private static void ProcessAssembly(Assembly assembly)
        {
            foreach (var target in assembly.GetExportedTypes())
                lock (Scanners)
                {
                    foreach (var currentScanner in Scanners) currentScanner.ProcessType(target);
                }
        }
    }
}