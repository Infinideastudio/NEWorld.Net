// 
// NEWorld/Game: World.cs
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
using System.Collections.Generic;
using System.Linq;
using Game.Utilities;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public partial class World
    {
        private static readonly Int3[] Delta =
        {
            new Int3(1, 0, 0), new Int3(-1, 0, 0),
            new Int3(0, 1, 0), new Int3(0, -1, 0),
            new Int3(0, 0, 1), new Int3(0, 0, -1)
        };

        private readonly Double3 hitboxOffset = new Double3(1.0, 1.0, 1.0);

        // All Chunks (Chunk array)

        public World(string name)
        {
            Name = name;
            Id = 0;
            DaylightBrightness = 15;
            Chunks = new ChunkManager();
        }

        ////////////////////////////////////////
        // World Properties
        ////////////////////////////////////////
        public string Name { get; }

        public uint Id { get; }

        public int DaylightBrightness { get; }

        ////////////////////////////////////////
        // Chunk Management
        ////////////////////////////////////////
        // Raw Access
        public ChunkManager Chunks { get; }

        // Alias declearations for Chunk management
        public int GetChunkCount()
        {
            return Chunks.Count;
        }

        public Chunk GetChunk(Int3 chunkPos)
        {
            return Chunks[chunkPos];
        }

        public bool IsChunkLoaded(Int3 chunkPos)
        {
            return Chunks.IsLoaded(chunkPos);
        }

        private void DeleteChunk(Int3 chunkPos)
        {
            Chunks.Remove(chunkPos);
        }

        public static Int3 GetChunkPos(Int3 pos)
        {
            return ChunkManager.GetPos(pos);
        }

        public BlockData GetBlock(Int3 pos)
        {
            return Chunks.GetBlock(pos);
        }

        public void SetBlock(ref Int3 pos, BlockData block)
        {
            Chunks.SetBlock(pos, block);
        }

        private void InsertChunk(Chunk chunk)
        {
            Chunks.Add(chunk.Position, chunk);
        }

        public Chunk InsertChunkAndUpdate(Chunk chunk)
        {
            InsertChunk(chunk);
            foreach (var dt in Delta)
                if (Chunks.TryGetValue(chunk.Position + dt, out var target))
                    target.IsUpdated = true;
            return chunk;
        }

        private void ResetChunk(Chunk ptr)
        {
            Chunks[ptr.Position].MoveFrom(ptr);
        }

        public List<Aabb> GetHitboxes(Aabb range)
        {
            var res = new List<Aabb>();
            Int3 curr;
            for (curr.X = (int) Math.Floor(range.Min.X); curr.X < (int) Math.Ceiling(range.Max.X); curr.X++)
            for (curr.Y = (int) Math.Floor(range.Min.Y); curr.Y < (int) Math.Ceiling(range.Max.Y); curr.Y++)
            for (curr.Z = (int) Math.Floor(range.Min.Z); curr.Z < (int) Math.Ceiling(range.Max.Z); curr.Z++)
            {
                // TODO: BlockType::getAABB
                if (!IsChunkLoaded(GetChunkPos(curr)))
                    continue;
                if (GetBlock(curr).Id == 0)
                    continue;
                var currd = new Double3(curr.X, curr.Y, curr.Z);
                res.Add(new Aabb(currd, currd + hitboxOffset));
            }

            return res;
        }

        private void ResetChunkAndUpdate(Chunk chunk)
        {
            ResetChunk(chunk);
            foreach (var dt in Delta)
                if (Chunks.TryGetValue(chunk.Position + dt, out var target))
                    target.IsUpdated = true;
        }
    }

    public class WorldManager : List<World>
    {
        public World Add(string name)
        {
            Add(new World(name));
            return this.Last();
        }

        public World Get(string name)
        {
            foreach (var world in this)
                if (world.Name == name)
                    return world;
            return null;
        }

        public World Get(uint id)
        {
            foreach (var world in this)
                if (world.Id == id)
                    return world;
            return null;
        }
    }
}