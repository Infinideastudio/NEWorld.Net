using System.Collections.Generic;
using Core;
using Game.Terrain;
using Xenko.Graphics;

namespace NEWorld.Renderer
{
    [DeclareService("BlockTextures")]
    public class RdTextures : IBlockTextures
    {
        public uint Add(string assetUri)
        {
            var id = (uint) textures.Count;
            textures.Add(Context.Content.Load<Texture>(assetUri));
            return id;
        }

        public void GetTexturePos(ref BlockTexCoord pos)
        {
        }

        private List<Texture> textures = new List<Texture>();
    }
}