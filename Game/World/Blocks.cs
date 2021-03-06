﻿// 
// NEWorld/Game: Blocks.cs
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
    public struct BlockData
    {
        public BlockData(ushort id)
        {
            Id = id;
            Brightness = 15;
            Data = 0;
            Unused = 0;
        }

        public ushort Id;
        public byte Brightness;
        public byte Unused;
        public uint Data;
    }

    public class BlockType
    {
        public BlockType(string name, bool solid, bool translucent, bool opaque, int hardness)
        {
            Name = name;
            IsSolid = solid;
            IsTranslucent = translucent;
            IsOpaque = opaque;
            Hardness = hardness;
        }

        public string Name { get; }
        public int Hardness { get; }
        public bool IsSolid { get; }
        public bool IsTranslucent { get; }
        public bool IsOpaque { get; }
    }

    // TODO: Implement ID Synchronization for each Game Instance
    public static class Blocks
    {
        private static readonly BlockType Air = new BlockType("Air", false, false, false, 0);

        public static readonly BlockType[] Index;
        private static ushort _count;

        static Blocks()
        {
            Index = new BlockType[1 << 12];
            Register(Air);
        }

        public static ushort Register(BlockType block)
        {
            Index[_count] = block;
            return _count++;
        }
    }
}