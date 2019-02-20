// 
// NEWorld/Core: Mat4.cs
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

using System.Collections.Generic;
using Xenko.Core.Mathematics;

namespace Core.Math
{
    public struct Mat4D
    {
        private const double Pi = System.Math.PI;

        public double[] Data;

        public Mat4D(double x)
        {
            Data = new[]
            {
                x, 0.0f, 0.0f, 0.0f,
                0.0f, x, 0.0f, 0.0f,
                0.0f, 0.0f, x, 0.0f,
                0.0f, 0.0f, 0.0f, x
            };
        }

        public static Mat4D operator +(Mat4D lhs, Mat4D rhs)
        {
            var result = lhs;
            for (var i = 0; i < 16; ++i) result.Data[i] += rhs.Data[i];

            return result;
        }

        public static Mat4D operator *(Mat4D lhs, Mat4D rhs)
        {
            var res = new Mat4D(0.0f);
            res.Data[0] = lhs.Data[0] * rhs.Data[0] + lhs.Data[1] * rhs.Data[4] + lhs.Data[2] * rhs.Data[8] +
                          lhs.Data[3] * rhs.Data[12];
            res.Data[1] = lhs.Data[0] * rhs.Data[1] + lhs.Data[1] * rhs.Data[5] + lhs.Data[2] * rhs.Data[9] +
                          lhs.Data[3] * rhs.Data[13];
            res.Data[2] = lhs.Data[0] * rhs.Data[2] + lhs.Data[1] * rhs.Data[6] + lhs.Data[2] * rhs.Data[10] +
                          lhs.Data[3] * rhs.Data[14];
            res.Data[3] = lhs.Data[0] * rhs.Data[3] + lhs.Data[1] * rhs.Data[7] + lhs.Data[2] * rhs.Data[11] +
                          lhs.Data[3] * rhs.Data[15];
            res.Data[4] = lhs.Data[4] * rhs.Data[0] + lhs.Data[5] * rhs.Data[4] + lhs.Data[6] * rhs.Data[8] +
                          lhs.Data[7] * rhs.Data[12];
            res.Data[5] = lhs.Data[4] * rhs.Data[1] + lhs.Data[5] * rhs.Data[5] + lhs.Data[6] * rhs.Data[9] +
                          lhs.Data[7] * rhs.Data[13];
            res.Data[6] = lhs.Data[4] * rhs.Data[2] + lhs.Data[5] * rhs.Data[6] + lhs.Data[6] * rhs.Data[10] +
                          lhs.Data[7] * rhs.Data[14];
            res.Data[7] = lhs.Data[4] * rhs.Data[3] + lhs.Data[5] * rhs.Data[7] + lhs.Data[6] * rhs.Data[11] +
                          lhs.Data[7] * rhs.Data[15];
            res.Data[8] = lhs.Data[8] * rhs.Data[0] + lhs.Data[9] * rhs.Data[4] + lhs.Data[10] * rhs.Data[8] +
                          lhs.Data[11] * rhs.Data[12];
            res.Data[9] = lhs.Data[8] * rhs.Data[1] + lhs.Data[9] * rhs.Data[5] + lhs.Data[10] * rhs.Data[9] +
                          lhs.Data[11] * rhs.Data[13];
            res.Data[10] = lhs.Data[8] * rhs.Data[2] + lhs.Data[9] * rhs.Data[6] + lhs.Data[10] * rhs.Data[10] +
                           lhs.Data[11] * rhs.Data[14];
            res.Data[11] = lhs.Data[8] * rhs.Data[3] + lhs.Data[9] * rhs.Data[7] + lhs.Data[10] * rhs.Data[11] +
                           lhs.Data[11] * rhs.Data[15];
            res.Data[12] = lhs.Data[12] * rhs.Data[0] + lhs.Data[13] * rhs.Data[4] + lhs.Data[14] * rhs.Data[8] +
                           lhs.Data[15] * rhs.Data[12];
            res.Data[13] = lhs.Data[12] * rhs.Data[1] + lhs.Data[13] * rhs.Data[5] + lhs.Data[14] * rhs.Data[9] +
                           lhs.Data[15] * rhs.Data[13];
            res.Data[14] = lhs.Data[12] * rhs.Data[2] + lhs.Data[13] * rhs.Data[6] + lhs.Data[14] * rhs.Data[10] +
                           lhs.Data[15] * rhs.Data[14];
            res.Data[15] = lhs.Data[12] * rhs.Data[3] + lhs.Data[13] * rhs.Data[7] + lhs.Data[14] * rhs.Data[11] +
                           lhs.Data[15] * rhs.Data[15];
            return res;
        }

        // Get transposed matrix
        public Mat4D Transposed()
        {
            return new Mat4D(0.0f)
            {
                Data =
                {
                    [0] = Data[0],
                    [1] = Data[4],
                    [2] = Data[8],
                    [3] = Data[12],
                    [4] = Data[1],
                    [5] = Data[5],
                    [6] = Data[9],
                    [7] = Data[13],
                    [8] = Data[2],
                    [9] = Data[6],
                    [10] = Data[10],
                    [11] = Data[14],
                    [12] = Data[3],
                    [13] = Data[7],
                    [14] = Data[11],
                    [15] = Data[15]
                }
            };
        }

        // Construct a translation matrix
        public static Mat4D Translation(Double3 delta)
        {
            return new Mat4D(1.0f)
            {
                Data =
                {
                    [3] = delta.X,
                    [7] = delta.Y,
                    [11] = delta.Z
                }
            };
        }

        // Construct a identity matrix
        public static Mat4D Identity()
        {
            return new Mat4D(1.0f);
        }

        // Construct a rotation matrix
        public static Mat4D Rotation(double degrees, Double3 vec)
        {
            vec.Normalize();
            var alpha = degrees * Pi / 180.0f;
            var s = System.Math.Sin(alpha);
            var c = System.Math.Cos(alpha);
            var t = 1.0f - c;
            return new Mat4D(0.0f)
            {
                Data =
                {
                    [0] = t * vec.X * vec.X + c,
                    [1] = t * vec.X * vec.Y - s * vec.Z,
                    [2] = t * vec.X * vec.Z + s * vec.Y,
                    [4] = t * vec.X * vec.Y + s * vec.Z,
                    [5] = t * vec.Y * vec.Y + c,
                    [6] = t * vec.Y * vec.Z - s * vec.X,
                    [8] = t * vec.X * vec.Z - s * vec.Y,
                    [9] = t * vec.Y * vec.Z + s * vec.X,
                    [10] = t * vec.Z * vec.Z + c,
                    [15] = 1.0f
                }
            };
        }

        public KeyValuePair<Double3, double> Transform(Double3 vec, double w)
        {
            var res = new Double3(Data[0] * vec.X + Data[1] * vec.Y + Data[2] * vec.Z + Data[3] * w,
                Data[4] * vec.X + Data[5] * vec.Y + Data[6] * vec.Z + Data[7] * w,
                Data[8] * vec.X + Data[9] * vec.Y + Data[10] * vec.Z + Data[11] * w);
            var rw = Data[12] * vec.X + Data[13] * vec.Y + Data[14] * vec.Z + Data[15] * w;
            return new KeyValuePair<Double3, double>(res, rw);
        }
    }
}