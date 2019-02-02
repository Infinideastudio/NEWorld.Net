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

        public const int SizeLog2 = 5;
        public const int RowSize = 32;
        public const int RowLast = RowSize - 1;
        public const int SliceSize = RowSize * RowSize;
        public const int CubeSize = SliceSize * RowSize;
        public const int BitShiftX = SizeLog2 * 2;
        public const int BitShiftY = SizeLog2;
        public const int AxisBits = 0b11111;

        // Chunk size
        private static bool _chunkGeneratorLoaded;
        private static Generator _chunkGen;

        // For Garbage Collection
        private long mLastRequestTime;

        public Chunk(Int3 position, World world, bool build = true)
        {
            Position = position;
            World = world;
            Blocks = new BlockData[CubeSize];
            if (build)
                Build(world.DaylightBrightness);
        }

        // TODO: somehow avoid it! not safe.
        public bool IsUpdated { get; set; }

        public Int3 Position { get; }

        public World World { get; }

        public BlockData[] Blocks { get; }
        
        public BlockData this[int x, int y, int z]
        {
            get => Blocks[x << BitShiftX | y << BitShiftY | z];
            set
            {
                Blocks[x << BitShiftX | y << BitShiftY | z] = value;
                IsUpdated = true;
            }
        }

        public BlockData this[Int3 pos]
        {
            get => Blocks[pos.X << BitShiftX | pos.Y << BitShiftY | pos.Z];
            set
            {
                Blocks[pos.X << BitShiftX | pos.Y << BitShiftY | pos.Z] = value;
                IsUpdated = true;
            }
        }

        public static void SetGenerator(Generator gen)
        {
            if (!_chunkGeneratorLoaded)
            {
                _chunkGen = gen;
                _chunkGeneratorLoaded = true;
            }
            else
            {
                throw new Exception("Chunk Generator Already Loaded");
            }
        }

        // Build chunk
        public void Build(int daylightBrightness)
        {
            _chunkGen(Position, Blocks, daylightBrightness);
            IsUpdated = true;
        }

        // Reference Counting
        public void MarkRequest()
        {
            mLastRequestTime = DateTime.Now.Ticks;
        }

        public bool CheckReleaseable()
        {
            return DateTime.Now - new DateTime(Interlocked.Read(ref mLastRequestTime)) > TimeSpan.FromSeconds(10);
        }
    }

    public class ChunkManager : Dictionary<Int3, Chunk>
    {
        public bool IsLoaded(Int3 chunkPos)
        {
            return ContainsKey(chunkPos);
        }

        // Convert world position to chunk coordinate (one axis)
        public static int GetAxisPos(int pos)
        {
            return pos >> Chunk.SizeLog2;
        }

        // Convert world position to chunk coordinate (all axes)
        public static Int3 GetPos(Int3 pos)
        {
            return new Int3(GetAxisPos(pos.X), GetAxisPos(pos.Y), GetAxisPos(pos.Z));
        }

        // Convert world position to block coordinate in chunk (one axis)
        public static int GetBlockAxisPos(int pos)
        {
            return pos & Chunk.AxisBits;
        }

        // Convert world position to block coordinate in chunk (all axes)
        public static Int3 GetBlockPos(Int3 pos)
        {
            return new Int3(GetBlockAxisPos(pos.X), GetBlockAxisPos(pos.Y), GetBlockAxisPos(pos.Z));
        }

        public BlockData GetBlock(Int3 pos)
        {
            return this[GetPos(pos)][GetBlockPos(pos)];
        }

        public void SetBlock(Int3 pos, BlockData block)
        {
            this[GetPos(pos)][GetBlockPos(pos)] = block;
        }
    }
}