using System;

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

    public partial class Chunk
    {
        public delegate void Generator(ChunkGeneratorContext context);

        private static bool _chunkGeneratorLoaded;
        private static Generator _chunkGen;

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

        private void Build(int daylightBrightness)
        {
            _chunkGen(new ChunkGeneratorContext(this, daylightBrightness));
            IsUpdated = true;
        }
    }
}