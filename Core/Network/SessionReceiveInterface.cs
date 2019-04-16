// 
// NEWorld/Core: SessionReceiveInterface.cs
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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MessagePack;

namespace Core.Network
{
    public sealed partial class Session
    {
        public sealed class Receive : BinaryReader
        {
            internal Receive(Session s) : base(new ReceiveStream(s))
            {
                Session = s;
            }

            public Session Session { get; }

            internal async Task<int> Wait()
            {
                var head = new byte[4];
                await BaseStream.ReadAsync(head, 0, 4);
                if (CheckHeaderMark(head))
                    throw new Exception("Bad Package Received");
                return (int) ReadUInt32();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool CheckHeaderMark(byte[] head)
            {
                return ((head[0] << 24) | (head[1] << 16) | (head[2] << 8) | head[3]) != 0x4E575243;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Read<T>()
            {
                return MessagePackSerializer.Deserialize<T>(BaseStream);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Task LoadExpected(int length)
            {
                return ((ReceiveStream) BaseStream).LoadExpected(length);
            }
        }
    }
}