// 
// NEWorld/Core: SessionReceiveStream.cs
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

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Network
{
    public sealed partial class Session
    {
        private sealed class ReceiveStream : StreamNothing
        {
            private readonly Session session;
            private Stream stream;

            public ReceiveStream(Session s)
            {
                stream = s.ios;
                session = s;
            }

            public override bool CanRead => stream.CanRead;
            public override long Length => stream.Length;

            public override long Position
            {
                get => stream.Position;
                set => stream.Position = value;
            }

            internal async Task LoadExpected(int length)
            {
                if (length == 0) return;
                EnsureSize(length);
                await stream.ReadAsync(session.storage, 0, length);
                stream = session.buffer;
            }

            private void EnsureSize(int length)
            {
                ref var storage = ref session.storage;

                if (length > storage.Length)
                {
                    storage = new byte[1 << (int) System.Math.Ceiling(System.Math.Log(length) / System.Math.Log(2))];
                    session.buffer = new MemoryStream(storage, 0, storage.Length, false, true);
                }
                else
                {
                    session.buffer.Seek(0, SeekOrigin.Begin);
                }
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int ReadByte()
            {
                return stream.ReadByte();
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Read(byte[] buffer, int begin, int count)
            {
                var end = begin + count;
                while (begin != end) begin += stream.Read(buffer, begin, end - begin);

                return count;
            }
            
            // ReSharper disable once TooManyArguments
            public override async Task<int> ReadAsync(byte[] buffer, int begin, int count,
                CancellationToken cancellationToken)
            {
                var end = begin + count;
                while (begin != end) begin += await stream.ReadAsync(buffer, begin, end - begin, cancellationToken);
                return count;
            }
        }
    }
}