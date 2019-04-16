// 
// NEWorld/Core: SessionSendInterface.cs
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
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Core.Network
{
    public sealed partial class Session
    {
        public sealed class Send : BinaryWriter, IDisposable
        {
            private readonly Session session;

            internal Send(Session session, uint protocol)
            {
                this.session = session;
                OutStream = new SendStream(this.session.ios);
                session.writeLock.EnterWriteLock();
                Write(0x4352574E);
                Write(protocol);
            }

            void IDisposable.Dispose()
            {
                ReleaseUnmanagedResources();
                base.Dispose();
                GC.SuppressFinalize(this);
            }

            ~Send()
            {
                ReleaseUnmanagedResources();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(ArraySegment<byte> bytes)
            {
                Debug.Assert(bytes.Array != null, "bytes.Array != null");
                Write(bytes.Array, bytes.Offset, bytes.Count);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WriteObject<T>(T obj)
            {
                MessagePack.MessagePackSerializer.Serialize(OutStream, obj);
            }

            private void ReleaseUnmanagedResources()
            {
                OutStream.Flush();
                session.writeLock.ExitWriteLock();
            }
        }
    }
}