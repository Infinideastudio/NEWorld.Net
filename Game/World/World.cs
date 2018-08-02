// 
// Game: World.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Math;
using Game.Utilities;

namespace Game.World
{
    public partial class World
    {
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
        public int GetChunkCount() => Chunks.Count;

        public Chunk GetChunk(ref Vec3<int> chunkPos) => Chunks[chunkPos];

        public bool IsChunkLoaded(Vec3<int> chunkPos) => Chunks.IsLoaded(chunkPos);

        public void DeleteChunk(Vec3<int> chunkPos) => Chunks.Remove(chunkPos);

        public static int GetChunkAxisPos(int pos) => ChunkManager.GetAxisPos(pos);

        public static Vec3<int> GetChunkPos(Vec3<int> pos) => ChunkManager.GetPos(pos);

        public static int GetBlockAxisPos(int pos) => ChunkManager.GetBlockAxisPos(pos);

        public static Vec3<int> GetBlockPos(ref Vec3<int> pos) => ChunkManager.GetBlockPos(pos);

        public BlockData GetBlock(Vec3<int> pos) => Chunks.GetBlock(pos);

        public void SetBlock(ref Vec3<int> pos, BlockData block) => Chunks.SetBlock(pos, block);

        public Chunk InsertChunk(ref Vec3<int> pos, Chunk chunk)
        {
            Chunks.Add(pos, chunk);
            return Chunks[pos];
        }

        private static readonly Vec3<int>[] Delta =
        {
            new Vec3<int>(1, 0, 0), new Vec3<int>(-1, 0, 0),
            new Vec3<int>(0, 1, 0), new Vec3<int>(0, -1, 0),
            new Vec3<int>(0, 0, 1), new Vec3<int>(0, 0, -1)
        };

        public Chunk InsertChunkAndUpdate(Vec3<int> pos, Chunk chunk)
        {
            var ret = InsertChunk(ref pos, chunk);
            foreach (var dt in Delta)
                if (Chunks.TryGetValue(pos + dt, out var target))
                    target.IsUpdated = true;
            return ret;
        }

        public Chunk ResetChunk(ref Vec3<int> pos, Chunk ptr) => Chunks[pos] = ptr;

        private readonly Vec3<double> _hitboxOffset = new Vec3<double>(1.0, 1.0, 1.0);

        public List<Aabb> GetHitboxes(Aabb range)
        {
            var res = new List<Aabb>();
            Vec3<int> curr;
            for (curr.X = (int) Math.Floor(range.Min.X); curr.X < (int) Math.Ceiling(range.Max.X); curr.X++)
            {
                for (curr.Y = (int) Math.Floor(range.Min.Y); curr.Y < (int) Math.Ceiling(range.Max.Y); curr.Y++)
                {
                    for (curr.Z = (int) Math.Floor(range.Min.Z); curr.Z < (int) Math.Ceiling(range.Max.Z); curr.Z++)
                    {
                        // TODO: BlockType::getAABB
                        if (!IsChunkLoaded(GetChunkPos(curr)))
                            continue;
                        if (GetBlock(curr).Id == 0)
                            continue;
                        var currd = new Vec3<double>(curr.X, curr.Y, curr.Z);
                        res.Add(new Aabb(currd, currd + _hitboxOffset));
                    }
                }
            }

            return res;
        }

        public void UpdateChunkLoadStatus()
        {
            lock (_mutex)
            {
                foreach (var kvPair in Chunks.ToList())
                    if (kvPair.Value.CheckReleaseable())
                        Chunks.Remove(kvPair.Key);
            }
        }
        
        protected static uint IdCount;

        // All Chunks (Chunk array)
        private readonly object _mutex = new object();
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