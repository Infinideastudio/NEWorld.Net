// 
// NEWorld/Game: ChunkUpdate.cs
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

using Xenko.Core.Mathematics;

namespace Game.World
{
    public partial class Chunk
    {
        public delegate void ChunkUpdateHandler(Chunk chunk);

        private bool pendingLocalUpdate, pendingNeighborUpdate;

        public static event ChunkUpdateHandler OnChunkUpdate;

        public static event ChunkUpdateHandler OnNeighborUpdate;

        private void TriggerUpdate()
        {
            EnqueueLocalUpdateTask();
            TriggerNeighborUpdate();
        }

        private async void EnqueueLocalUpdateTask()
        {
            pendingLocalUpdate = true;
            await ChunkService.TaskDispatcher.NextReadOnlyChance();
            OnChunkUpdate?.Invoke(this);
        }

        private void TriggerNeighborUpdate()
        {
            foreach (var neighbor in GetNeighbors()) neighbor.EnqueueNeighborUpdateTask();
        }

        private async void EnqueueNeighborUpdateTask()
        {
            pendingNeighborUpdate = true;
            await ChunkService.TaskDispatcher.NextReadOnlyChance();
            OnNeighborUpdate?.Invoke(this);
        }

        public Chunk[] GetNeighbors()
        {
            // TODO: Cache The Value
            return new[]
            {
                World.GetChunk(new Int3(Position.X + 1, Position.Y, Position.Z)),
                World.GetChunk(new Int3(Position.X - 1, Position.Y, Position.Z)),
                World.GetChunk(new Int3(Position.X, Position.Y + 1, Position.Z)),
                World.GetChunk(new Int3(Position.X, Position.Y - 1, Position.Z)),
                World.GetChunk(new Int3(Position.X, Position.Y, Position.Z + 1)),
                World.GetChunk(new Int3(Position.X, Position.Y, Position.Z - 1))
            };
        }
    }
}