// 
// NEWorld/Core: ConnectionHost.cs
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
using System.Net.Sockets;

namespace Core.Network
{
    [DeclareService("Core.Network.ConnectionHost")]
    public sealed partial class ConnectionHost : IDisposable
    {
        private const double UtilizationThreshold = 0.25;
        private static int _connectionCounter;
        private static List<Connection> _connections;

        static ConnectionHost()
        {
            _connections = new List<Connection>();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ConnectionHost()
        {
            ReleaseUnmanagedResources();
        }

        private static void SweepInvalidConnectionsIfNecessary()
        {
            var utilization = (double) _connectionCounter / _connections.Count;
            if (utilization < UtilizationThreshold)
                SweepInvalidConnections();
        }

        private static void SweepInvalidConnections()
        {
            var swap = new List<Connection>();
            foreach (var hd in _connections)
                if (hd.Valid)
                    swap.Add(hd);
            _connections = swap;
        }

        public static Connection Add(TcpClient conn, List<Protocol> protocols)
        {
            var connect = new Connection(conn, protocols);
            _connections.Add(connect);
            return connect;
        }

        public static int CountConnections()
        {
            return _connectionCounter;
        }

        private void ReleaseUnmanagedResources()
        {
            foreach (var hd in _connections)
                hd.Close();
        }
    }
}