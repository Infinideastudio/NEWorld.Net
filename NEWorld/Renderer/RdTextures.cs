// 
// NEWorld/NEWorld: RdTextures.cs
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
using System.Collections.Generic;
using Core;
using Game.Terrain;
using Xenko.Graphics;
using System;
using Xenko.Core.Mathematics;

namespace NEWorld.Renderer
{
    [DeclareService("BlockTextures")]
    public class RdTextures : IBlockTextures
    {
        public uint Add(string assetUri)
        {
            var id = (uint) textures.Count;
            var texture = Context.Content.Load<Texture>(assetUri);
            if (Context.Content.IsLoaded(assetUri))
            {
                textures.Add(texture);
            }

            return id;
        }

        public void GetTexturePos(ref BlockTexCoord pos)
        {
            pos.Tex = new Int2(0, 0); //new Int2((int) (pos.Id % texturePerLine), (int) (pos.Id / texturePerLine));
        }

        public static Texture FlushTextures()
        {
            var count = textures.Count;
            texturePerLine = (1 << (int)(Math.Ceiling(Math.Log(Math.Ceiling(Math.Sqrt(count))) / Math.Log(2))));
            var wid = texturePerLine * pixelPerTexture;
            var texture = Texture.New2D(Context.GraphicsDevice, wid, wid,
                1/*(int) (Math.Log(pixelPerTexture) / Math.Log(2))*/, PixelFormat.R8G8B8A8_UInt);
            for (var i = 0; i < count; ++i)
            {
                var tile = textures[i];
                /*var raw = tile.GetData<byte>(Context.CommandList);
                var unit = raw.Length / (tile.Width * tile.Height);
                var down = new byte[unit * pixelPerTexture * pixelPerTexture];
                var xFact = (double)tile.Width / pixelPerTexture;
                var yFact = (double)tile.Height / pixelPerTexture;
                for (var xi = 0; xi < pixelPerTexture; ++xi)
                {
                    for (var yi = 0; yi < pixelPerTexture; ++yi)
                    {
                        var from = (int)(unit * (Math.Round(xi * xFact) * pixelPerTexture + Math.Round(yi * yFact)));
                        var to = unit * (xi * pixelPerTexture + yi);
                        for (var n = 0; n < unit; ++n)
                        {
                            down[to + n] = raw[from + n];
                        }
                    }
                }

                var re = Texture.New2D(Context.GraphicsDevice, pixelPerTexture, pixelPerTexture, tile.Format, down);*/
                var re = tile;
                var x = i % texturePerLine;
                var y = i / texturePerLine;
                var rx = x * pixelPerTexture;
                var ry = y * pixelPerTexture;
                Context.CommandList.CopyRegion(
                    re, re.GetSubResourceIndex(0, 0), null, 
                    texture, texture.GetSubResourceIndex(0, 0), rx, ry);
            }
            return texture;
        }

        public static int TexturesPreLine => 1;

        private static int texturePerLine, pixelPerTexture = 32;
        private static List<Texture>textures = new List<Texture>();
    }
}