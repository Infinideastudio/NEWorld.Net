// 
// NEWorld/Game: ChunkConstants.cs
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

namespace Game.World
{
    public partial class Chunk
    {
        public const int SizeLog2 = 5;
        public const int RowSize = 32;
        public const int RowLast = RowSize - 1;
        public const int SliceSize = RowSize * RowSize;
        public const int CubeSize = SliceSize * RowSize;
        public const int BitShiftX = SizeLog2 * 2;
        public const int BitShiftY = SizeLog2;
        public const int AxisBits = 0b11111;
    }
}