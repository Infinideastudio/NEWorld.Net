// 
// NEWorld/NEWorld: CubeRotate.cs
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
using Xenko.Core.Mathematics;
using Xenko.Engine;

namespace NEWorld.UI.Lounge
{
    public class CubeRotate : SyncScript
    {
        private TransformComponent skyboxRotation;
        private DateTime start;

        public override void Start()
        {
            start = DateTime.Now;
            skyboxRotation = Entity.Get<TransformComponent>();
        }

        public override void Update()
        {
            var axisVec = new Vector3(0.1f, 1.0f, 0.1f);
            axisVec.Normalize();
            var elapsed = (DateTime.Now - start).TotalSeconds;
            var angle = elapsed * 2.0 * Math.PI / 180.0;
            var rotationVec = axisVec * (float) Math.Sin(angle);
            var rotate = new Quaternion(0.0f)
            {
                W = (float) Math.Cos(angle),
                X = rotationVec.X,
                Y = rotationVec.Y,
                Z = rotationVec.Z
            };
            skyboxRotation.Rotation = rotate;
        }
    }
}