// 
// NEWorld/Core: SessionStreamNothing.cs
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

namespace Core.Network
{
    public sealed partial class Session
    {
        private abstract class StreamNothing : Stream
        {
            private void ThrowException()
            {
                throw new Exception("Not Allowed");
            }

            private T ThrowException<T>()
            {
                throw new Exception("Not Allowed");
            }

            public override void Flush()
            {
                ThrowException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ThrowException<int>();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return ThrowException<long>();
            }

            public override void SetLength(long value)
            {
                ThrowException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                ThrowException();
            }

            public override int ReadByte()
            {
                return ThrowException<int>();
            }

            public override void WriteByte(byte value)
            {
                ThrowException();
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => 0;
            public override long Position { get; set; } = 0;
        }
    }
}