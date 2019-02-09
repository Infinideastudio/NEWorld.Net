// 
// NEWorld/Game: Object.cs
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
using Game.Utilities;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public abstract class Object
    {
        public Aabb Hitbox;

        public Double3 Position;
        public Double3 Rotation;
        public Double3 Scale;

        protected Object(uint worldId)
        {
            WorldId = worldId;
            Scale = new Double3(1.0, 1.0, 1.0);
        }

        protected Object(uint worldId, Double3 position, Double3 rotation, Double3 scale, Aabb hitbox)
        {
            WorldId = worldId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Hitbox = hitbox;
        }

        public uint WorldId { get; }

        public void MoveHitbox(Double3 delta)
        {
            Hitbox.Move(delta);
        }

        public abstract void Render();
        public abstract void Update(World world);
    }
}