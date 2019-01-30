// 
// Game: Player.cs
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

using Core.Math;
using Core.Utilities;

namespace Game.World
{
    public class Player : PlayerObject
    {
        private class PlayerUpdateTask : IReadOnlyTask
        {
            public PlayerUpdateTask(Player player, uint worldId)
            {
                this.player = player;
                this.worldId = worldId;
            }

            public void Task(ChunkService srv) => player.Update(srv.Worlds.Get(worldId));

            private readonly Player player;
            private readonly uint worldId;
        }

        public Player(uint worldId) : base(worldId) =>
            Singleton<ChunkService>.Instance.TaskDispatcher.AddRegular(new PlayerUpdateTask(this, WorldId));

        public void Accelerate(Vec3<double> acceleration) => speed += acceleration;

        public void AccelerateRotation(Vec3<double> acceleration) => rotationSpeed += acceleration;

        public void SetSpeed(Vec3<double> speed) => this.speed = speed;

        public Vec3<double> PositionDelta => positionDelta;

        public Vec3<double> RotationDelta { get; private set; }

        public override void Render()
        {
        }

        private Vec3<double> speed, rotationSpeed;
        private Vec3<double> positionDelta;

        public override void Update(World world)
        {
            Move(world);
            RotationMove();
            Accelerate(new Vec3<double>(0.0, -0.1, 0.0)); // Gravity
        }

        private void Move(World world)
        {
            positionDelta = Mat4D.Rotation(Rotation.Y, new Vec3<double>(0.0f, 1.0f, 0.0f)).Transform(speed, 0.0).Key;
            var originalDelta = positionDelta;
            var hitboxes = world.GetHitboxes(Hitbox.Expand(positionDelta));

            foreach (var curr in hitboxes)
                positionDelta.X = Hitbox.MaxMoveOnXclip(curr, positionDelta.X);
            MoveHitbox(new Vec3<double>(positionDelta.X, 0.0, 0.0));
            if (positionDelta.X != originalDelta.X) speed.X = 0.0;

            foreach (var curr in hitboxes)
                positionDelta.Z = Hitbox.MaxMoveOnZclip(curr, positionDelta.Z);
            MoveHitbox(new Vec3<double>(0.0, 0.0, positionDelta.Z));
            if (positionDelta.Z != originalDelta.Z) speed.Z = 0.0;

            foreach (var curr in hitboxes)
                positionDelta.Y = Hitbox.MaxMoveOnYclip(curr, positionDelta.Y);
            MoveHitbox(new Vec3<double>(0.0, positionDelta.Y, 0.0));
            if (positionDelta.Y != originalDelta.Y) speed.Y = 0.0;

            Position += positionDelta;
        }

        private static readonly bool RotationInteria = false;

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
    }
}