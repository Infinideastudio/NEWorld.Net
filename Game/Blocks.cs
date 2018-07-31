// 
// Game: Blocks.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2018 NEWorld Team
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

namespace Game
{
    public struct BlockData
    {
        public BlockData(ushort id) => Id = id;
        public ushort Id;
        public byte Brightness;
        public byte Data0;
        public uint Data1;
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

    public static class Blocks
    {
        private static readonly BlockType Air = new BlockType("Air", false, false, false, 0);

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

        public static readonly BlockType[] Index;
        private static ushort _count;
    }
}