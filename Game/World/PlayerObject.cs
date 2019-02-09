// 
// NEWorld/Game: PlayerObject.cs
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
using Game.Utilities;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public class PlayerObject : Object
    {
        private readonly double height;
        private readonly double width;
        private Double3 hitboxSize;

        public PlayerObject(uint worldId) :
            base(worldId, new Double3(), new Double3(), new Double3(1.0, 1.0, 1.0), new Aabb())
        {
            height = 1.6;
            width = 0.6;
            Speed = 0.2;
            RefreshHitbox();
        }

        // Body direction, head direction is `mRotation` in class Object
        public Double3 Direction { get; set; }

        public double Speed { get; set; }

        public void Rotate(Double3 rotation)
        {
            Rotation += rotation;
        }

        private void RefreshHitbox()
        {
            hitboxSize = new Double3(width, height, width);
            Hitbox = new Aabb(-hitboxSize / 2, hitboxSize / 2);
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