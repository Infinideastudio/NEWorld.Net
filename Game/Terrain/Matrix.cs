// 
// Game: Matrix.cs
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

namespace Game.Terrain
{
    public static class Matrix
    {
        public static void RestoreModel() => _model = Mat4F.Identity();
        public static void RestoreView() => _view = Mat4F.Identity();
        public static void RestoreProjection() => _projection = Mat4F.Identity();

        private static Mat4F _model = Mat4F.Identity(), _view = Mat4F.Identity(), _projection = Mat4F.Identity();

        public static void ApplyPerspective(float fov, float aspect, float zNear, float zFar) =>
            _projection *= Mat4F.Perspective(fov, aspect, zNear, zFar);

        public static void ViewRotate(float degree, Vec3<float> axis) => _view *= Mat4F.Rotation(degree, axis);

        public static void ViewTranslate(Vec3<float> diff) => _view *= Mat4F.Translation(diff);

        public static void ModelRotate(float degree, Vec3<float> axis) => _model *= Mat4F.Rotation(degree, axis);

        public static void ModelTranslate(Vec3<float> diff) => _model *= Mat4F.Translation(diff);

        public static void ViewRotate(float degree, Vec3<double> axis) => _view *= Mat4F.Rotation(degree, Conv(axis));

        public static void ViewTranslate(Vec3<double> diff) => _view *= Mat4F.Translation(Conv(diff));

        public static void ModelRotate(float degree, Vec3<double> axis) => _model *= Mat4F.Rotation(degree, Conv(axis));

        public static void ViewRotate(float degree, Vec3<int> axis) => _view *= Mat4F.Rotation(degree, Conv(axis));

        public static void ViewTranslate(Vec3<int> diff) => _view *= Mat4F.Translation(Conv(diff));

        public static void ModelRotate(float degree, Vec3<int> axis) => _model *= Mat4F.Rotation(degree, Conv(axis));

        public static void ModelSetTranslate(Vec3<int> diff) => _model = Mat4F.Translation(Conv(diff));

        public static Mat4F Get() => _model * _view * _projection;

        private static Vec3<float> Conv(Vec3<double> v) => new Vec3<float>((float) v.X, (float) v.Y, (float) v.Z);

        private static Vec3<float> Conv(Vec3<int> v) => new Vec3<float>(v.X, v.Y, v.Z);
    }
}