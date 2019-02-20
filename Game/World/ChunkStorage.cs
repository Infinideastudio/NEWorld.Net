// 
// NEWorld/Game: ChunkStorage.cs
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

using System;
using System.Runtime.InteropServices;

namespace Game.World
{
    public unsafe partial class Chunk
    {
        public BlockData* Blocks { get; private set; }
        public uint CopyOnWrite { get; private set; } = uint.MaxValue;

        internal void EnableFullArray()
        {
            CopyOnWrite = uint.MaxValue;
            Blocks = Allocator.Allocate();
        }

        internal void EnableCopyOnWrite(uint other)
        {
            Blocks = StaticChunkPool.GetChunk(other).Blocks;
            CopyOnWrite = other;
        }

        private bool IsCopyOnWrite()
        {
            return CopyOnWrite != uint.MaxValue;
        }

        private void ExecuteFullCopy()
        {
            lock (this)
            {
                if (IsCopyOnWrite())
                {
                    var old = Blocks;
                    EnableFullArray();
                    for (var i = 0; i < CubeSize; ++i)
                        Blocks[i] = old[i];
                }
            }
        }

        private void ReleaseBlockData()
        {
            if (Blocks != null)
            {
                if (!IsCopyOnWrite())
                    Allocator.Release(Blocks);
                Blocks = null;
            }
        }

        partial void MoveFromImpl(Chunk other)
        {
            ReleaseBlockData();
            CopyOnWrite = other.CopyOnWrite;
            Blocks = other.Blocks;
            other.Blocks = null;
            other.CopyOnWrite = uint.MaxValue;
            IsUpdated = true;
        }

        partial void ReleaseCriticalResources()
        {
            ReleaseBlockData();
        }

        private static class Allocator
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
}