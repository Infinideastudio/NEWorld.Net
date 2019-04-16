// 
// NEWorld/Core: ConnectionHostConnection.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Core.Network
{
    public sealed partial class ConnectionHost
    {
        public sealed class Connection
        {
            private readonly Task finalize;
            private readonly List<Protocol> protocols;
            internal readonly Session Session;

            public Connection(TcpClient client, List<Protocol> protocols)
            {
                this.protocols = protocols;
                Session = new Session(client);
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
                Interlocked.Increment(ref _connectionCounter);
                while (Valid && Session.Live)
                    try
                    {
                        var message = Session.WaitMessage();
                        await ProcessRequest(await message.Wait(), message);
                    }
                    catch (Exception e)
                    {
                        if (Session.Live) LogPort.Debug($"Encountering Exception {e}");
                    }

                CloseDown();
            }

            private async Task ProcessRequest(int protocol, Session.Receive message)
            {
                var handle = protocols[protocol];
                await message.LoadExpected(handle.Expecting);
                handle.HandleRequest(message);
            }

            private void CloseDown()
            {
                if (!Valid) return;
                Valid = false;
                Interlocked.Decrement(ref _connectionCounter);
                SweepInvalidConnectionsIfNecessary();
            }
        }
    }
}