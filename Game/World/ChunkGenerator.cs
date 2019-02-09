// 
// NEWorld/Game: ChunkGenerator.cs
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