﻿using Core;
using Game.Terrain;

namespace NEWorld.Renderer
{
    [DeclareService("BlockTextures")]
    public class RdTextures : IBlockTextures
    {
        public uint Add(string path)
        {
            return 0;
        }

        public void GetTexturePos(ref BlockTexCoord pos)
        {
        } 
    }
}