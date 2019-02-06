using Xenko.Core.Mathematics;

namespace Game.World
{
    public unsafe partial class Chunk
    {
        public BlockData this[int x, int y, int z]
        {
            get => Blocks[(x << BitShiftX) | (y << BitShiftY) | z];
            set
            {
                if (IsCopyOnWrite())
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
                if (IsCopyOnWrite())
                    ExecuteFullCopy();
                Blocks[(pos.X << BitShiftX) | (pos.Y << BitShiftY) | pos.Z] = value;
                IsUpdated = true;
            }
        }
    }
}