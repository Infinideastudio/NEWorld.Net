// 
// Game: PlayerObject.cs
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
using Core;
using Core.Math;

namespace Game
{
    public class PlayerObject : Object
    {
        public PlayerObject(uint worldId) :
            base(worldId, new Vec3<double>(), new Vec3<double>(), new Vec3<double>(1.0, 1.0, 1.0), new Aabb())
        {
            _height = 1.6;
            _width = 0.6;
            Speed = 0.2;
            RefreshHitbox();
        }

        public void Rotate(Vec3<double> rotation) => Rotation += rotation;

        // Body direction, head direction is `mRotation` in class Object
        public Vec3<double> Direction { get; set; }

        public double Speed { get; set; }

        private double _height, _width;
        private Vec3<double> _hitboxSize;

        private void RefreshHitbox()
        {
            _hitboxSize = new Vec3<double>(_width, _height, _width);
            Hitbox = new Aabb(-_hitboxSize / 2, _hitboxSize / 2);
        }

        public override void Render()
        {
            throw new NotImplementedException();
        }

        public override void Update(World world)
        {
            throw new NotImplementedException();
        }
    }
}