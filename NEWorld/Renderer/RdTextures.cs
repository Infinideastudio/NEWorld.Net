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
            var texture = Texture.New2D(Context.GraphicsDevice, wid, wid,
                1/*(int) (Math.Log(pixelPerTexture) / Math.Log(2))*/, PixelFormat.R8G8B8A8_UNorm);
            var result = Texture.New2D(Context.GraphicsDevice, pixelPerTexture, pixelPerTexture, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            var scaler = new ImageScaler(SamplingPattern.Expanded);
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
            return texture;
        }

        public static int TexturesPerLine { get; private set; }

        private static int pixelPerTexture = 32;
        private static readonly List<Texture> Textures = new List<Texture>();
    }
}