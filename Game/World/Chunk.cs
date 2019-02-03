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
using System.Runtime.InteropServices;
using System.Threading;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public class ChunkGeneratorContext
    {
        public readonly Chunk Current;

        public readonly int DaylightBrightness;

        public ChunkGeneratorContext(Chunk current, int daylightBrightness)
        {
            Current = current;
            DaylightBrightness = daylightBrightness;
        }

        public void EnableCopyOnWrite(uint target)
        {
            Current.EnableCopyOnWrite(target);
        }

        public void EnableFullArray()
        {
            Current.EnableFullArray();
        }
    }

    public unsafe class Chunk : IDisposable
    {
        public delegate void Generator(ChunkGeneratorContext context);

        public enum InitOption
        {
            None,
            Build,
            AllocateUnique
        }

        public const int SizeLog2 = 5;
        public const int RowSize = 32;
        public const int RowLast = RowSize - 1;
        public const int SliceSize = RowSize * RowSize;
        public const int CubeSize = SliceSize * RowSize;
        public const int BitShiftX = SizeLog2 * 2;
        public const int BitShiftY = SizeLog2;
        public const int AxisBits = 0b11111;

        // Flags
        private const uint CopyOnWriteBit = 0b1;

        // Chunk size
        private static bool _chunkGeneratorLoaded;
        private static Generator _chunkGen;

        private uint cowId;
        private uint flags;

        // For Garbage Collection
        private long mLastRequestTime;

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

        public uint CopyOnWrite => (flags & CopyOnWriteBit) == CopyOnWriteBit ? cowId : uint.MaxValue;

        // TODO: somehow avoid it! not safe.
        public bool IsUpdated { get; set; }

        public Int3 Position { get; }

        public World World { get; }

        public BlockData* Blocks { get; private set; }

        public BlockData this[int x, int y, int z]
        {
            get => Blocks[(x << BitShiftX) | (y << BitShiftY) | z];
            set
            {
                if ((flags & CopyOnWriteBit) == CopyOnWriteBit)
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
                if ((flags & CopyOnWriteBit) == CopyOnWriteBit)
                    ExecuteFullCopy();
                Blocks[(pos.X << BitShiftX) | (pos.Y << BitShiftY) | pos.Z] = value;
                IsUpdated = true;
            }
        }

        public void Dispose()
        {
            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        internal void EnableFullArray()
        {
            flags = 0;
            Blocks = ChunkDataAllocator.Allocate();
        }

        internal void EnableCopyOnWrite(uint other)
        {
            Blocks = StaticChunkPool.GetChunk(other).Blocks;
            flags = CopyOnWriteBit;
            cowId = other;
        }

        private void ExecuteFullCopy()
        {
            lock (this)
            {
                if ((flags & CopyOnWriteBit) == CopyOnWriteBit)
                {
                    var old = Blocks;
                    EnableFullArray();
                    for (var i = 0; i < CubeSize; ++i)
                        Blocks[i] = old[i];
                }
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
        private void Build(int daylightBrightness)
        {
            _chunkGen(new ChunkGeneratorContext(this, daylightBrightness));
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

        private void ReleaseResources()
        {
            if ((flags & CopyOnWriteBit) != CopyOnWriteBit)
                ChunkDataAllocator.Release(Blocks);
            Blocks = null;
        }

        ~Chunk()
        {
            ReleaseResources();
        }

        private static class ChunkDataAllocator
        {
            internal static BlockData* Allocate()
            {
                return (BlockData*) Marshal.AllocHGlobal(CubeSize * sizeof(BlockData)).ToPointer();
            }

            internal static void Release(BlockData* data)
            {
                Marshal.FreeHGlobal((IntPtr) data);
            }
        }
    }

    public static class StaticChunkPool
    {
        private static uint _airChunkId = uint.MaxValue;
        private static List<Chunk> _staticList = new List<Chunk>();
        private static Dictionary<string, uint> _id;

        static StaticChunkPool()
        {
            _id = new Dictionary<string, uint>();
            Register("Default.AirChunk", new Chunk(new BlockData(0)));
        }

        internal static Dictionary<string, uint> Id
        {
            get => _id;
            set
            {
                var oldId = value;
                var old = _staticList;
                _id = value;
                _staticList = new List<Chunk>(_id.Count);
                for (var i = 0; i < _id.Count; ++i)
                    _staticList.Add(null);
                foreach (var record in value)
                    _staticList[(int) record.Value] = old[(int) oldId[record.Key]];
            }
        }

        public static void Register(string name, Chunk staticChunk)
        {
            if (_id.TryGetValue(name, out var sid))
                if (_staticList[(int) sid] == null)
                    _staticList[(int) sid] = staticChunk;

            _id.Add(name, (uint) _staticList.Count);
            _staticList.Add(staticChunk);
        }

        public static uint GetAirChunk()
        {
            if (_airChunkId == uint.MaxValue)
                _airChunkId = _id["Default.AirChunk"];
            return _airChunkId;
        }

        public static Chunk GetChunk(uint id)
        {
            return _staticList[(int) id];
        }

        public static uint GetId(string name)
        {
            return _id[name];
        }
    }

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