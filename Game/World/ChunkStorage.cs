using System;
using System.Runtime.InteropServices;

namespace Game.World
{
    public unsafe partial class Chunk
    {
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
    }
}
