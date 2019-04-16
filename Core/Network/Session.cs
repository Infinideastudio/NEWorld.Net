// 
// NEWorld/Core: Session.cs
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
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Core.Network
{
    public sealed partial class Session : IDisposable
    {
        private readonly TcpClient conn;
        private readonly NetworkStream ios;
        private readonly ReaderWriterLockSlim writeLock = new ReaderWriterLockSlim();
        private MemoryStream buffer;
        private byte[] storage = new byte[8192];

        internal Session(TcpClient io)
        {
            conn = io;
            ios = io.GetStream();
            buffer = new MemoryStream(storage, 0, storage.Length, false, true);
        }

        public bool Live => conn.Connected;

        public void Dispose()
        {
            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        ~Session()
        {
            ReleaseResources();
        }

        internal Receive WaitMessage()
        {
            return new Receive(this);
        }

        public Send CreateMessage(uint protocol)
        {
            return new Send(this, protocol);
        }

        private void ReleaseResources()
        {
            ios.Close();
            conn?.Dispose();
            ios?.Dispose();
            buffer?.Dispose();
        }
    }
}