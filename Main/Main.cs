// 
// NEWorld/Main: Main.cs
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
using Core;
using Core.Module;
using Game;
using Game.Terrain;
using Game.World;

namespace Main
{
    [DeclareModule]
    public class Main : IModule
    {
        private static ushort _grassId, _rockId, _dirtId, _sandId, _waterId;

        private static uint _rockChunkId, _waterChunkId;

        public void CoInitialize()
        {
            _grassId = Blocks.Register(new BlockType("Grass", true, false, true, 2));
            _rockId = Blocks.Register(new BlockType("Rock", true, false, true, 2));
            _dirtId = Blocks.Register(new BlockType("Dirt", true, false, true, 2));
            _sandId = Blocks.Register(new BlockType("Sand", true, false, true, 2));
            _waterId = Blocks.Register(new BlockType("Water", false, true, false, 2));
            Chunk.SetGenerator(WorldGen.Generator);
            StaticChunkPool.Register("Main.RockChunk", new Chunk(new BlockData(_rockId)));
            StaticChunkPool.Register("Main.WaterChunk", new Chunk(new BlockData(_waterId)));
            EventBus.AddCollection(this);
        }

        [DeclareBusEventHandler]
        public void GameRenderInit(object sender, GameRenderPrepareEvent load)
        {
            RendererInit();
        }

        [DeclareBusEventHandler]
        public void GameLoads(object sender, GameLoadEvent load)
        {
            _rockChunkId = StaticChunkPool.GetId("Main.RockChunk");
            _waterChunkId = StaticChunkPool.GetId("Main.WaterChunk");
        }
        
        [DeclareBusEventHandler]
        public void GameUnloads(object sender, GameUnloadEvent load)
        {
        }

        [DeclareBusEventHandler]
        public void GameRenderFinalize(object sender, GameRenderFinalizeEvent load)
        {
        }

        public void CoFinalize()
        {
        }

        public void OnMemoryWarning()
        {
        }

        private static void RendererInit()
        {
            if (!Services.TryGet<IBlockTextures>("BlockTextures", out var textures)) return;
            uint[] id =
            {
                textures.Add("Textures/Blocks/GrassTop"),
                textures.Add("Textures/Blocks/GrassRound"),
                textures.Add("Textures/Blocks/Dirt"),
                textures.Add("Textures/Blocks/Rock"),
                textures.Add("Textures/Blocks/Sand"),
                textures.Add("Textures/Blocks/Water")
            };

            var grass = new DefaultBlockRenderer(new[] {id[1], id[1], id[0], id[2], id[1], id[1]});
            var rock = new DefaultBlockRenderer(new[] {id[3], id[3], id[3], id[3], id[3], id[3]});
            var dirt = new DefaultBlockRenderer(new[] {id[2], id[2], id[2], id[2], id[2], id[2]});
            var sand = new DefaultBlockRenderer(new[] {id[4], id[4], id[4], id[4], id[4], id[4]});
            var water = new DefaultBlockRenderer(new[] {id[5], id[5], id[5], id[5], id[5], id[5]});

            BlockRenderers.Add(_grassId, grass);
            BlockRenderers.Add(_rockId, rock);
            BlockRenderers.Add(_dirtId, dirt);
            BlockRenderers.Add(_sandId, sand);
            BlockRenderers.Add(_waterId, water);
        }

        private static class WorldGen
        {
            private const double NoiseScaleX = 64.0;
            private const double NoiseScaleZ = 64.0;
            private static int Seed { get; } = 1025;

            private static double Noise(int x, int y)
            {
                var xx = x * 107 + y * 13258953287;
                xx = (xx >> 13) ^ xx;
                return ((xx * (xx * xx * 15731 + 789221) + 1376312589) & 0x7fffffff) / 16777216.0;
            }

            private static double InterpolatedNoise(double x, double y)
            {
                var intX = (int)System.Math.Floor(x);
                var fractionalX = x - intX;
                var intY = (int)System.Math.Floor(y);
                var fractionalY = y - intY;
                var v1 = Noise(intX, intY);
                var v2 = Noise(intX + 1, intY);
                var v3 = Noise(intX, intY + 1);
                var v4 = Noise(intX + 1, intY + 1);
                var i1 = v1 * (1.0 - fractionalX) + v2 * fractionalX;
                var i2 = v3 * (1.0 - fractionalX) + v4 * fractionalX;
                return i1 * (1.0 - fractionalY) + i2 * fractionalY;
            }

            private static double PerlinNoise2D(double x, double y)
            {
                double total = 0, frequency = 1, amplitude = 1;
                for (var i = 0; i <= 4; i++)
                {
                    total += InterpolatedNoise(x * frequency, y * frequency) * amplitude;
                    frequency *= 2;
                    amplitude /= 2.0;
                }

                return total;
            }

            public static unsafe void Generator(ChunkGeneratorContext context)
            {
                var pos = context.Current.Position;
                var heights = new int[Chunk.RowSize, Chunk.RowSize];
                var low = int.MaxValue;
                var high = int.MinValue;
                {
                    for (var x = 0; x < Chunk.RowSize; x++)
                    for (var z = 0; z < Chunk.RowSize; z++)
                    {
                        var val = heights[x, z] = (int) PerlinNoise2D((pos.X * Chunk.RowSize + x) / NoiseScaleX,
                                            (pos.Z * Chunk.RowSize + z) / NoiseScaleZ) / 2 - 64;
                        if (val < low) low = val;
                        if (val > high) high = val;
                    }

                    if (pos.Y * Chunk.RowSize > high && high >= 0)
                    {
                        context.EnableCopyOnWrite(StaticChunkPool.GetAirChunk());
                        return;
                    }

                    if ((0-Chunk.RowSize) >= pos.Y * Chunk.RowSize && pos.Y * Chunk.RowSize > high)
                    {
                        context.EnableCopyOnWrite(StaticChunkPool.GetAirChunk());
                        return;
                    }

                    if (pos.Y * Chunk.RowSize < (low - Chunk.RowSize - 3))
                    {
                        context.EnableCopyOnWrite(_rockChunkId);
                        return;
                    }
                }
                {
                    context.EnableFullArray();
                    var blocks = context.Current.Blocks;
                    for (var x = 0; x < Chunk.RowSize; x++)
                    for (var z = 0; z < Chunk.RowSize; z++)
                    {
                        var absHeight = heights[x, z];
                        var height = absHeight - pos.Y * Chunk.RowSize;
                        var underWater = absHeight <= 0;
                        for (var y = 0; y < Chunk.RowSize; y++)
                        {
                            ref var block = ref blocks[x * Chunk.RowSize * Chunk.RowSize + y * Chunk.RowSize + z];
                            if (y <= height)
                            {
                                if (y == height)
                                    block.Id = underWater ? _sandId : _grassId;
                                else if (y >= height - 3)
                                    block.Id = underWater ? _sandId : _dirtId;
                                else
                                    block.Id = _rockId;

                                block.Brightness = 0;
                                block.Data = 0;
                            }
                            else
                            {
                                block.Id = pos.Y * Chunk.RowSize + y <= 0 ? _waterId : (ushort) 0;
                                block.Brightness = (byte) context.DaylightBrightness;
                                block.Data = 0;
                            }
                        }
                    }
                }
            }
        }
    }
}