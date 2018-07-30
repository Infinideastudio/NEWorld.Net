// 
// Core: ConnectionHost.cs
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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Network
{
    public class ConnectionHost
    {
        public class Connection
        {
            public Connection(ulong cid, TcpClient client, ConnectionHost server)
            {
                _cid = cid;
                _client = client;
                _server = server;
                Stream = client.GetStream();
                _finalize = Start();
            }

            public bool Valid { get; private set; }

            public void Close()
            {
                CloseDown();
                _finalize.Wait();
            }

            private async Task Start()
            {
                Valid = true;
                var headerCache = new byte[8]; // ["NWRC"] + Int32BE(Protocol Id)
                while (Valid)
                {
                    try
                    {
                        var bytesRead = await Stream.ReadAsync(headerCache, 0, 8);
                        if (VerifyPackageValidity(headerCache, bytesRead))
                            _server.Protocols[GetProtocolId(headerCache)].HandleRequest(Stream);
                        else
                            break;
                    }
                    catch (Exception)
                    {
                        if (_client.Connected == false)
                            break;
                        throw;
                    }
                }

                CloseDown();
            }

            private void CloseDown()
            {
                if (!Valid) return;
                Valid = false;
                Stream.Close(); // Cancellation Token Doesn't Work. Hard Close is adopted.
                _client.Close();
                Interlocked.Increment(ref _server._invalidConnections);
            }

            public NetworkStream Stream { get; }

            private static int GetProtocolId(byte[] head) => head[4] << 24 | head[5] << 16 | head[6] << 8 | head[7];

            private static bool VerifyPackageValidity(byte[] head, int read) =>
                head[0] == 'N' && head[1] == 'W' && head[2] == 'R' && head[3] == 'C' && read == 8;

            private ulong _cid;
            private readonly TcpClient _client;
            private readonly ConnectionHost _server;
            private readonly Task _finalize;
        }

        public ConnectionHost()
        {
            _clients = new Dictionary<ulong, Connection>();
            Protocols = new List<Protocol>();
        }

        public delegate void LockedExecProc();

        public void LockedExec(LockedExecProc proc)
        {
            lock (_protocolLock)
            {
                proc();
            }
        }

        private const double UtilizationThreshold = 0.75;

        public void RegisterProtocol(Protocol newProtocol)
        {
            lock (_protocolLock)
            {
                Protocols.Add(newProtocol);
            }
        }

        public void SweepInvalidConnectionsIfNecessary()
        {
            var utilization = 1.0 - (double) _invalidConnections / _clients.Count;
            if (utilization < UtilizationThreshold)
                SweepInvalidConnections();
        }

        public Connection GetConnection(ulong id) => _clients[id];

        private void SweepInvalidConnections()
        {
            foreach (var hd in _clients.ToList())
                if (!hd.Value.Valid)
                {
                    _clients.Remove(hd.Key);
                    Interlocked.Decrement(ref _invalidConnections);
                }
        }

        public void AddConnection(TcpClient conn)
        {
            _clients.Add(_sessionIdTop, new Connection(_sessionIdTop, conn, this));
            ++_sessionIdTop;
        }

        private ulong _sessionIdTop;
        private int _invalidConnections;
        public readonly List<Protocol> Protocols;
        private readonly Dictionary<ulong, Connection> _clients;
        private readonly object _protocolLock = new object();

        public void CloseAll()
        {
            foreach (var hd in _clients)
                hd.Value.Close();
        }
    }
}