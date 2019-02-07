using Game.World;
using Xenko.Core.Mathematics;

namespace Game.Client
{
    public class PlayerChunkView
    {
        private const int SectionMask = 0b1111;

        private const int SectionBits = 4;

        private Chunk[,,][,,] Section;

        private Int3 BasePosition;

        private Chunk[,,] GetSectionRelative(Int3 offset)
        {
            return Section[offset.X >> SectionBits, offset.Y >> SectionBits, offset.Y >> SectionBits];
        }

        private Chunk GetChunkRelative(Int3 offset)
        {
            return GetSectionRelative(offset)[offset.X & SectionMask, offset.Y & SectionMask, offset.Z & SectionMask];
        }

        private Int3 ComputeRelative(Int3 absolute)
        {
            return absolute - BasePosition;
        }

        public Chunk GetChunk(Int3 chunk)
        {
            return GetChunkRelative(ComputeRelative(chunk));
        }
    }
}