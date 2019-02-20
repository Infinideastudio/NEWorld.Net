// 
// NEWorld/Game: PlayerChunkView.cs
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

using Game.World;
using Xenko.Core.Mathematics;

namespace Game.Client
{
    public class PlayerChunkView
    {
        private const int SectionMask = 0b1111;

        private const int SectionBits = 4;

        private Int3 BasePosition;

        private Chunk[,,][,,] Section;

        private Chunk[,,] GetSectionRelative(Int3 offset)
        {
            return Section[offset.X >> SectionBits, offset.Y >> SectionBits, offset.Y >> SectionBits];
        }

        private Chunk GetChunkRelative(Int3 offset)
        {
            return GetSectionRelative(offset)[offset.X & SectionMask, offset.Y & SectionMask, offset.Z & SectionMask];
        }

        private Int3 ComputeRelative(Int3 absolute)
        {
            return absolute - BasePosition;
        }

        public Chunk GetChunk(Int3 chunk)
        {
            return GetChunkRelative(ComputeRelative(chunk));
        }
    }
}