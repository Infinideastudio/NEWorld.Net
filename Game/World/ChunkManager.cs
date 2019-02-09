// 
// NEWorld/Game: ChunkManager.cs
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
using System.Collections.Generic;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public class ChunkManager : Dictionary<Int3, Chunk>
    {
        public bool IsLoaded(Int3 chunkPos)
        {
            return ContainsKey(chunkPos);
        }

        // Convert world position to chunk coordinate (all axes)
        public static Int3 GetPos(Int3 pos)
        {
            return new Int3(pos.X >> Chunk.SizeLog2, pos.Y >> Chunk.SizeLog2, pos.Z >> Chunk.SizeLog2);
        }

        public BlockData GetBlock(Int3 pos)
        {
            return this[GetPos(pos)][pos.X & Chunk.AxisBits, pos.Y & Chunk.AxisBits, pos.Z & Chunk.AxisBits];
        }

        public void SetBlock(Int3 pos, BlockData block)
        {
            this[GetPos(pos)][pos.X & Chunk.AxisBits, pos.Y & Chunk.AxisBits, pos.Z & Chunk.AxisBits] = block;
        }
    }
}