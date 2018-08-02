// 
// Game: Object.cs
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
using Core.Math;
using Game.Utilities;

namespace Game.World
{
    public abstract class Object
    {
        protected Object(uint worldId)
        {
            WorldId = worldId;
            Scale = new Vec3<double>(1.0, 1.0, 1.0);
        }

        protected Object(uint worldId, Vec3<double> position, Vec3<double> rotation, Vec3<double> scale, Aabb hitbox)
        {
            WorldId = worldId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Hitbox = hitbox;
        }

        public void MoveHitbox(Vec3<double> delta) => Hitbox.Move(delta);

        public abstract void Render();
        public abstract void Update(World world);

        public Vec3<double> Position;
        public Vec3<double> Rotation;
        public Vec3<double> Scale;
        public Aabb Hitbox;
        public uint WorldId { get; }
    }
}