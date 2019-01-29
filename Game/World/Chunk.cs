// 
// Game: Chunk.cs
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

using System;
using System.Collections.Generic;
using System.Threading;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public class Chunk
    {
        public delegate void Generator(Int3 chunkPos, BlockData[] chunkData, int daylightBrightness);

        // Chunk size
        private static bool _chunkGeneratorLoaded;
        private static Generator _chunkGen;
        public const int BlocksSize = 0b1000000000000000;
        public const int SizeLog2 = 5;
        public const int Size = 0b100000;

        public static void SetGenerator(Generator gen)
        {
            if (!_chunkGeneratorLoaded)
            {
                _chunkGen = gen;
                _chunkGeneratorLoaded = true;
            }
            else
            {
                throw new Exception("Chunk Generator Alreadly Loaded");
            }
        }

        public Chunk(Int3 position, World world)
        {
            Position = position;
            World = world;
            Blocks = new BlockData[BlocksSize];
            Build(world.DaylightBrightness);
        }

        // TODO: somehow avoid it! not safe.
        public bool IsUpdated { get; set; }

        public Int3 Position { get; }

        public World World { get; }

        public BlockData[] Blocks { get; }

        public BlockData this[Int3 pos]
        {
            get => Blocks[pos.X * Size * Size + pos.Y * Size + pos.Z];
            set
            {
                Blocks[pos.X * Size * Size + pos.Y * Size + pos.Z] = value;
                IsUpdated = true;
            }
        }

        // Build chunk
        public void Build(int daylightBrightness)
        {
            _chunkGen(Position, Blocks, daylightBrightness);
            IsUpdated = true;
        }

        // Reference Counting
        public void MarkRequest() => mLastRequestTime = DateTime.Now.Ticks;

        public bool CheckReleaseable() =>
            DateTime.Now - new DateTime(Interlocked.Read(ref mLastRequestTime)) > TimeSpan.FromSeconds(10);

        // For Garbage Collection
        private long mLastRequestTime;
    }

    [Serializable]
    public class ChunkManager : Dictionary<Int3, Chunk>
    {
        public bool IsLoaded(Int3 chunkPos) => ContainsKey(chunkPos);

        // Convert world position to chunk coordinate (one axis)
        public static int GetAxisPos(int pos) => pos >> Chunk.SizeLog2;

        // Convert world position to chunk coordinate (all axes)
        public static Int3 GetPos(Int3 pos) =>
            new Int3(GetAxisPos(pos.X), GetAxisPos(pos.Y), GetAxisPos(pos.Z));

        // Convert world position to block coordinate in chunk (one axis)
        public static int GetBlockAxisPos(int pos) => pos & (Chunk.Size - 1);

        // Convert world position to block coordinate in chunk (all axes)
        public static Int3 GetBlockPos(Int3 pos) =>
            new Int3(GetBlockAxisPos(pos.X), GetBlockAxisPos(pos.Y), GetBlockAxisPos(pos.Z));

        public BlockData GetBlock(Int3 pos) => this[GetPos(pos)][GetBlockPos(pos)];

        public void SetBlock(Int3 pos, BlockData block) => this[GetPos(pos)][GetBlockPos(pos)] = block;
    }
}