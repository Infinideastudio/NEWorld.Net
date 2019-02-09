// 
// NEWorld/Game: Player.cs
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
using Core.Math;
using Core.Utilities;
using Xenko.Core.Mathematics;

namespace Game.World
{
    public class Player : PlayerObject
    {
        private static readonly bool RotationInteria = false;
        private Double3 positionDelta;

        private Double3 speed, rotationSpeed;

        public Player(uint worldId) : base(worldId)
        {
            ChunkService.TaskDispatcher.AddRegular(new PlayerUpdateTask(this, WorldId));
        }

        public Double3 PositionDelta => positionDelta;

        public Double3 RotationDelta { get; private set; }

        public void Accelerate(Double3 acceleration)
        {
            speed += acceleration;
        }

        public void AccelerateRotation(Double3 acceleration)
        {
            rotationSpeed += acceleration;
        }

        public void SetSpeed(Double3 speed)
        {
            this.speed = speed;
        }

        public override void Render()
        {
        }

        public override void Update(World world)
        {
            Move(world);
            RotationMove();
            Accelerate(new Double3(0.0, -0.1, 0.0)); // Gravity
        }

        private void Move(World world)
        {
            positionDelta = Mat4D.Rotation(Rotation.Y, new Double3(0.0f, 1.0f, 0.0f)).Transform(speed, 0.0).Key;
            var originalDelta = positionDelta;
            var hitboxes = world.GetHitboxes(Hitbox.Expand(positionDelta));

            foreach (var curr in hitboxes)
                positionDelta.X = Hitbox.MaxMoveOnXclip(curr, positionDelta.X);
            MoveHitbox(new Double3(positionDelta.X, 0.0, 0.0));
            if (positionDelta.X != originalDelta.X) speed.X = 0.0;

            foreach (var curr in hitboxes)
                positionDelta.Z = Hitbox.MaxMoveOnZclip(curr, positionDelta.Z);
            MoveHitbox(new Double3(0.0, 0.0, positionDelta.Z));
            if (positionDelta.Z != originalDelta.Z) speed.Z = 0.0;

            foreach (var curr in hitboxes)
                positionDelta.Y = Hitbox.MaxMoveOnYclip(curr, positionDelta.Y);
            MoveHitbox(new Double3(0.0, positionDelta.Y, 0.0));
            if (positionDelta.Y != originalDelta.Y) speed.Y = 0.0;

            Position += positionDelta;
        }

        private void RotationMove()
        {
            if (Rotation.X + rotationSpeed.X > 90.0)
                rotationSpeed.X = 90.0 - Rotation.X;
            if (Rotation.X + rotationSpeed.X < -90.0)
                rotationSpeed.X = -90.0 - Rotation.X;
            Rotation += rotationSpeed;
            RotationDelta = rotationSpeed;
            if (RotationInteria)
                rotationSpeed *= 0.6;
            else
                rotationSpeed *= 0;
        }

        private class PlayerUpdateTask : IRegularReadOnlyTask
        {
            private readonly Player player;
            private readonly uint worldId;

            public PlayerUpdateTask(Player player, uint worldId)
            {
                this.player = player;
                this.worldId = worldId;
            }

            public void Task(int instance, int count)
            {
                if (instance == count)
                {
                    player.Update(ChunkService.Worlds.Get(worldId));
                }
            }
        }
    }
}