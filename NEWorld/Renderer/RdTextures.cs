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
using Xenko.Rendering.Images;

namespace NEWorld.Renderer
{
    [DeclareService("BlockTextures")]
    public class RdTextures : IBlockTextures
    {
        public uint Add(string assetUri)
        {
            var id = (uint) Textures.Count;
            var texture = Context.Content.Load<Texture>(assetUri);
            if (Context.Content.IsLoaded(assetUri))
            {
                Textures.Add(texture);
            }

            return id;
        }

        public void GetTexturePos(ref BlockTexCoord pos)
        {
            pos.Tex = new Int2((int) (pos.Id % TexturesPerLine), (int) (pos.Id / TexturesPerLine));
        }

        public static Texture FlushTextures()
        {
            var count = Textures.Count;
            TexturesPerLine = (1 << (int)(Math.Ceiling(Math.Log(Math.Ceiling(Math.Sqrt(count))) / Math.Log(2))));
            var wid = TexturesPerLine * pixelPerTexture;
            using (Texture texture = Texture.New2D(Context.GraphicsDevice, wid, wid, PixelFormat.R8G8B8A8_UNorm),
                result = Texture.New2D(Context.GraphicsDevice, pixelPerTexture, pixelPerTexture,
                    PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource))
            {
                using (var scaler = new ImageScaler(SamplingPattern.Linear))
                {
                    scaler.SetOutput(result);
                    for (var i = 0; i < count; ++i)
                    {
                        var tile = Textures[i];
                        scaler.SetInput(tile);
                        scaler.Draw(Context.RdwContext);
                        var x = i % TexturesPerLine;
                        var y = i / TexturesPerLine;
                        var rx = x * pixelPerTexture;
                        var ry = y * pixelPerTexture;
                        Context.RdwContext.CommandList.CopyRegion(
                            result, result.GetSubResourceIndex(0, 0), null,
                            texture, texture.GetSubResourceIndex(0, 0), rx, ry);
                    }
                    return MakeMipmap(texture, (int) Math.Floor(Math.Log(pixelPerTexture) / Math.Log(2)), scaler);
                }
            }
        }

        private static Texture MakeMipmap(Texture input, int levels, ImageScaler scaler)
        {
            var ret = Texture.New2D(Context.GraphicsDevice, input.Width, input.Height, levels + 1, input.Format);
            var fact = 1;
            scaler.SetInput(input);
            for (var i = 0; i <= levels; ++i)
            {
                using (var target = Texture.New2D(Context.GraphicsDevice, input.Width / fact, input.Height / fact,
                    input.Format, TextureFlags.RenderTarget | TextureFlags.ShaderResource))
                {
                    scaler.SetOutput(target);
                    scaler.Draw(Context.RdwContext);
                    Context.RdwContext.CommandList.CopyRegion(
                        target, target.GetSubResourceIndex(0, 0), null,
                        ret, ret.GetSubResourceIndex(0, i), 0, 0);
                }

                fact *= 2;
            }

            return ret;
        }

        public static int TexturesPerLine { get; private set; }

        private static int pixelPerTexture = 32;
        private static readonly List<Texture> Textures = new List<Texture>();
    }
}