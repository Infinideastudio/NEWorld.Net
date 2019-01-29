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
using Xenko.Rendering;

namespace Core.Network
{
    public class ConnectionHost
    {
        public class Connection
        {
            public Connection(ulong cid, TcpClient client, ConnectionHost server)
            {
                this.cid = cid;
                this.client = client;
                this.server = server;
                Stream = client.GetStream();
                finalize = Start();
            }

            public bool Valid { get; private set; }

            public void Close()
            {
                CloseDown();
                finalize.Wait();
            }

            private async Task Start()
            {
                Valid = true;
                var headerCache = new byte[8]; // ["NWRC"] + Int32BE(Protocol Id)
                while (Valid)
                {
                    try
                    {
                        int bytesRead;
                        do
                        {
                            bytesRead = await Stream.ReadAsync(headerCache, 0, 8);
                        } while (bytesRead != 8);
                        if (VerifyPackageValidity(headerCache, bytesRead))
                            try
                            {
                                server.Protocols[GetProtocolId(headerCache)].HandleRequest(Stream);
                            }
                            catch (Exception e)
                            {
                                LogPort.Debug(e.ToString());
                            }
                        else
                            throw new Exception("Bad Package Recieved");
                    }
                    catch (Exception e)
                    {
                        if (client.Connected == false)
                            break;
                        LogPort.Debug($"Encountering Exception {e}");
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
                client.Close();
                Interlocked.Increment(ref server.invalidConnections);
            }

            public NetworkStream Stream { get; }

            private static int GetProtocolId(byte[] head) => head[4] << 24 | head[5] << 16 | head[6] << 8 | head[7];

            private static bool VerifyPackageValidity(byte[] head, int read) =>
                head[0] == 'N' && head[1] == 'W' && head[2] == 'R' && head[3] == 'C' && read == 8;

            private ulong cid;
            private readonly TcpClient client;
            private readonly ConnectionHost server;
            private readonly Task finalize;
        }

        public ConnectionHost()
        {
            clients = new Dictionary<ulong, Connection>();
            Protocols = new List<Protocol>();
        }

        public object Lock => protocolLock;

        private const double UtilizationThreshold = 0.75;

        public void RegisterProtocol(Protocol newProtocol)
        {
            lock (protocolLock)
            {
                Protocols.Add(newProtocol);
            }
        }

        public void SweepInvalidConnectionsIfNecessary()
        {
            var utilization = 1.0 - (double) invalidConnections / clients.Count;
            if (utilization < UtilizationThreshold)
                SweepInvalidConnections();
        }

        public Connection GetConnection(ulong id) => clients[id];

        private void SweepInvalidConnections()
        {
            foreach (var hd in clients.ToList())
                if (!hd.Value.Valid)
                {
                    clients.Remove(hd.Key);
                    Interlocked.Decrement(ref invalidConnections);
                }
        }

        public void AddConnection(TcpClient conn)
        {
            clients.Add(sessionIdTop, new Connection(sessionIdTop, conn, this));
            ++sessionIdTop;
        }

        private ulong sessionIdTop;
        private int invalidConnections;
        public readonly List<Protocol> Protocols;
        private readonly Dictionary<ulong, Connection> clients;
        private readonly object protocolLock = new object();

        public void CloseAll()
        {
            foreach (var hd in clients)
                hd.Value.Close();
        }

        public int CountConnections() => clients.Count - invalidConnections;
    }
}