// 
// Main: Main.cs
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

using Core;
using Core.Module;
using Game.Terrain;
using Game.World;
using Xenko.Core.Mathematics;

namespace Main
{
    [DeclareModule]
    public class Main : IModule
    {
        private static ushort _grassId, _rockId, _dirtId, _sandId, _waterId;

        public void CoInitialize()
        {
            _grassId = Blocks.Register(new BlockType("Grass", true, false, true, 2));
            _rockId = Blocks.Register(new BlockType("Rock", true, false, true, 2));
            _dirtId = Blocks.Register(new BlockType("Dirt", true, false, true, 2));
            _sandId = Blocks.Register(new BlockType("Sand", true, false, true, 2));
            _waterId = Blocks.Register(new BlockType("Water", false, true, false, 2));
            Chunk.SetGenerator(WorldGen.Generator);
            RendererInit();
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
            var path = Path.Asset("Infinideas.Main") + "blocks/";
            uint[] id =
            {
                textures.Add(path + "grass_top.png"),
                textures.Add(path + "grass_round.png"),
                textures.Add(path + "dirt.png"),
                textures.Add(path + "rock.png"),
                textures.Add(path + "sand.png"),
                textures.Add(path + "water.png")
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
                var intX = (int) x;
                var fractionalX = x - intX;
                var intY = (int) y;
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

            public static void Generator(Int3 pos, BlockData[] blocks, int daylightBrightness)
            {
                for (var x = 0; x < Chunk.Size; x++)
                for (var z = 0; z < Chunk.Size; z++)
                {
                    var absHeight = (int) PerlinNoise2D((pos.X * Chunk.Size + x) / NoiseScaleX,
                                        (pos.Z * Chunk.Size + z) / NoiseScaleZ) / 2 - 64;
                    var height = absHeight - pos.Y * Chunk.Size;
                    var underWater = absHeight <= 0;
                    for (var y = 0; y < Chunk.Size; y++)
                    {
                        ref var block = ref blocks[x * Chunk.Size * Chunk.Size + y * Chunk.Size + z];
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
                            block.Id = pos.Y * Chunk.Size + y <= 0 ? _waterId : (ushort) 0;
                            block.Brightness = (byte) daylightBrightness;
                            block.Data = 0;
                        }
                    }
                }
            }
        }
    }
}