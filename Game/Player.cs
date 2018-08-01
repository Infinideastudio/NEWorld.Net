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

using Core;
using Core.Math;

namespace Game
{
    public class Player : PlayerObject
    {
        private class PlayerUpdateTask : IReadOnlyTask
        {
            public PlayerUpdateTask(Player player, uint worldId)
            {
                _player = player;
                _worldId = worldId;
            }

            public void Task(ChunkService srv) => _player.Update(srv.Worlds.Get(_worldId));

            IReadOnlyTask IReadOnlyTask.Clone() => (IReadOnlyTask) MemberwiseClone();

            private readonly Player _player;
            private readonly uint _worldId;
        }

        public Player(uint worldId) : base(worldId) =>
            Singleton<ChunkService>.Instance.TaskDispatcher.AddRegular(new PlayerUpdateTask(this, WorldId));

        public void Accelerate(Vec3<double> acceleration) => _speed += acceleration;

        public void AccelerateRotation(Vec3<double> acceleration) => _rotationSpeed += acceleration;

        public void SetSpeed(Vec3<double> speed) => _speed = speed;

        public Vec3<double> PositionDelta => _positionDelta;

        public Vec3<double> RotationDelta { get; private set; }

        public override void Render()
        {
        }

        private Vec3<double> _speed, _rotationSpeed;
        private Vec3<double> _positionDelta;

        public override void Update(World world)
        {
            Move(world);
            RotationMove();
            Accelerate(new Vec3<double>(0.0, -0.1, 0.0)); // Gravity
        }

        private void Move(World world)
        {
            _positionDelta = Mat4D.Rotation(Rotation.Y, new Vec3<double>(0.0f, 1.0f, 0.0f)).Transform(_speed, 0.0).Key;
            var originalDelta = _positionDelta;
            var hitboxes = world.GetHitboxes(Hitbox.Expand(_positionDelta));

            foreach (var curr in hitboxes)
                _positionDelta.X = Hitbox.MaxMoveOnXclip(curr, _positionDelta.X);
            MoveHitbox(new Vec3<double>(_positionDelta.X, 0.0, 0.0));
            if (_positionDelta.X != originalDelta.X) _speed.X = 0.0;

            foreach (var curr in hitboxes)
                _positionDelta.Z = Hitbox.MaxMoveOnZclip(curr, _positionDelta.Z);
            MoveHitbox(new Vec3<double>(0.0, 0.0, _positionDelta.Z));
            if (_positionDelta.Z != originalDelta.Z) _speed.Z = 0.0;

            foreach (var curr in hitboxes)
                _positionDelta.Y = Hitbox.MaxMoveOnYclip(curr, _positionDelta.Y);
            MoveHitbox(new Vec3<double>(0.0, _positionDelta.Y, 0.0));
            if (_positionDelta.Y != originalDelta.Y) _speed.Y = 0.0;

            Position += _positionDelta;
        }

        private static bool _rotationInteria = false;

        private void RotationMove()
        {
            if (Rotation.X + _rotationSpeed.X > 90.0)
                _rotationSpeed.X = 90.0 - Rotation.X;
            if (Rotation.X + _rotationSpeed.X < -90.0)
                _rotationSpeed.X = -90.0 - Rotation.X;
            Rotation += _rotationSpeed;
            RotationDelta = _rotationSpeed;
            if (_rotationInteria)
                _rotationSpeed *= 0.6;
            else
                _rotationSpeed *= 0;
        }
    }
}