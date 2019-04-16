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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Core.Network
{
    public sealed partial class Session
    {
        private sealed class SendStream : StreamNothing
        {
            private const int BufferSize = 4096;

            private readonly NetworkStream stream;

            private byte[] current;

            private Task lastWrite;

            private int used;

            internal SendStream(NetworkStream stream)
            {
                current = GetNextBuffer();
                this.stream = stream;
            }
            
            public override bool CanWrite => true;      

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void WriteByte(byte value)
            {
                if (used == BufferSize)
                    Flush0();
                current[used++] = value;
            }

            public override void Flush()
            {
                Flush0();
                lastWrite?.Wait();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (count > BufferSize)
                {
                    WriteDirect(buffer, offset, count);
                    return;
                }

                CopyToBuffer(buffer, offset, count);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void CopyToBuffer(byte[] buffer, int offset, int count)
            {
                var remain = BufferSize - used;
                if (remain >= count)
                {
                    Array.Copy(buffer, offset, current, used, count);
                    used += count;
                }
                else
                {
                    Array.Copy(buffer, offset, current, used, remain);
                    used = BufferSize;
                    Flush0();
                    used = count - remain;
                    Array.Copy(buffer, offset + remain, current, 0, used);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static byte[] GetNextBuffer()
            {
                return new byte[BufferSize];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteDirect(byte[] buffer, int offset, int count)
            {
                Flush0();
                Write0(new ArraySegment<byte>(buffer, offset, count));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Flush0()
            {
                if (used > 0)
                {
                    Write0(new ArraySegment<byte>(current, 0, used));
                    current = GetNextBuffer();
                    used = 0;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Write0(ArraySegment<byte> target)
            {
                lastWrite = Write1(target, lastWrite);
            }

            private async Task Write1(ArraySegment<byte> target, Task last)
            {
                if (last != null) await last;

                stream.Write(target.Array, target.Offset, target.Count);
            }
        }
    }
}