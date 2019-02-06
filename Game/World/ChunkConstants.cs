namespace Game.World
{
    public partial class Chunk
    {
        public const int SizeLog2 = 5;
        public const int RowSize = 32;
        public const int RowLast = RowSize - 1;
        public const int SliceSize = RowSize * RowSize;
        public const int CubeSize = SliceSize * RowSize;
        public const int BitShiftX = SizeLog2 * 2;
        public const int BitShiftY = SizeLog2;
        public const int AxisBits = 0b11111;
    }
}