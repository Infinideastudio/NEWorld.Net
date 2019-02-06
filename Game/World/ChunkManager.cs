using System.Collections.Generic;
using Xenko.Core.Mathematics;

namespace Game.World
{
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