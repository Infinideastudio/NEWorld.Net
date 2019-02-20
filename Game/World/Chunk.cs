// 
// NEWorld/Game: Chunk.cs
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
    // TODO: Implement Neighbor Chunk Handle Storage
    // TODO: Implement Chunk Update Event Hook
    // TODO: Implement The Unloaded Status Indicator
    public unsafe partial class Chunk
    {
        public enum InitOption
        {
            None,
            Build,
            AllocateUnique
        }

        public Chunk(BlockData fill)
        {
            EnableFullArray();
            for (var i = 0; i < CubeSize; ++i)
                Blocks[i] = fill;
        }

        public Chunk(Int3 position, World world, InitOption option = InitOption.Build)
        {
            Position = position;
            World = world;
            switch (option)
            {
                case InitOption.Build:
                    Build(world.DaylightBrightness);
                    break;
                case InitOption.AllocateUnique:
                    EnableFullArray();
                    break;
                default:
                    EnableCopyOnWrite(StaticChunkPool.GetAirChunk());
                    break;
            }
        }

        public Chunk(Int3 position, World world, uint other)
        {
            Position = position;
            World = world;
            EnableCopyOnWrite(other);
        }

        // TODO: Only Set On Real Content Updation
        public bool IsUpdated { get; set; }

        public Int3 Position { get; }

        public World World { get; }

        public void MoveFrom(Chunk other)
        {
            MoveFromImpl(other);
        }

        partial void MoveFromImpl(Chunk other);
    }
}