// 
// NEWorld/Game: ChunkAccess.cs
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

using Xenko.Core.Mathematics;

namespace Game.World
{
    public unsafe partial class Chunk
    {
        public BlockData this[int x, int y, int z]
        {
            get => Blocks[(x << BitShiftX) | (y << BitShiftY) | z];
            set
            {
                if (IsCopyOnWrite())
                    ExecuteFullCopy();
                Blocks[(x << BitShiftX) | (y << BitShiftY) | z] = value;
                IsUpdated = true;
            }
        }

        public BlockData this[Int3 pos]
        {
            get => Blocks[(pos.X << BitShiftX) | (pos.Y << BitShiftY) | pos.Z];
            set
            {
                if (IsCopyOnWrite())
                    ExecuteFullCopy();
                Blocks[(pos.X << BitShiftX) | (pos.Y << BitShiftY) | pos.Z] = value;
                IsUpdated = true;
            }
        }

        public void SerializeTo(byte[] data)
        {
            fixed (byte* buffer = data)
            {
                SerializeTo(buffer);
            }
        }

        public void SerializeTo(byte[] data, int offset)
        {
            fixed (byte* buffer = data)
            {
                SerializeTo(buffer + offset);
            }
        }

        public void SerializeTo(byte* buffer)
        {
            for (var i = 0; i < 32768 * 4; ++i)
            {
                var block = Blocks[i >> 2];
                buffer[i++] = (byte) (block.Id >> 4);
                buffer[i++] = (byte) ((block.Id << 4) | block.Brightness);
                buffer[i++] = (byte) (block.Data >> 8);
                buffer[i] = (byte) block.Data;
            }
        }

        public void DeserializeFrom(byte[] data)
        {
            fixed (byte* buffer = data)
            {
                DeserializeFrom(buffer);
            }
        }

        public void DeserializeFrom(byte[] data, int offset)
        {
            fixed (byte* buffer = data)
            {
                DeserializeFrom(buffer + offset);
            }
        }

        public void DeserializeFrom(byte* buffer)
        {
            for (var i = 0; i < 32768 * 4; i += 4)
            {
                ref var block = ref Blocks[i >> 2];
                block.Id = (ushort) ((buffer[i] << 4) | (buffer[i + 1] >> 4));
                block.Brightness = (byte) (buffer[i + 1] | 0xF);
                block.Data = (uint) ((buffer[i + 2] << 8) | buffer[i + 3]);
            }
        }
    }
}