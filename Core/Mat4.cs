// 
// Core: Mat4.cs
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
using System.Collections.Generic;

namespace Core
{
    public struct Mat4F
    {
        private const float Pi = 3.1415926535897932f;

        public float[] Data;

        public Mat4F(float x)
        {
            Data = new[]
            {
                x, 0.0f, 0.0f, 0.0f,
                0.0f, x, 0.0f, 0.0f,
                0.0f, 0.0f, x, 0.0f,
                0.0f, 0.0f, 0.0f, x
            };
        }

        public static Mat4F operator +(Mat4F lhs, Mat4F rhs)
        {
            var result = lhs;
            for (var i = 0; i < 16; ++i)
            {
                result.Data[i] += rhs.Data[i];
            }

            return result;
        }

        public static Mat4F operator *(Mat4F lhs, Mat4F rhs)
        {
            var res = new Mat4F(0.0f);
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

        // Swap row r1, row r2
        public void SwapRows(uint r1, uint r2)
        {
            Generic.Swap(ref Data[r1 * 4 + 0], ref Data[r2 * 4 + 0]);
            Generic.Swap(ref Data[r1 * 4 + 1], ref Data[r2 * 4 + 1]);
            Generic.Swap(ref Data[r1 * 4 + 2], ref Data[r2 * 4 + 2]);
            Generic.Swap(ref Data[r1 * 4 + 3], ref Data[r2 * 4 + 3]);
        }

        // Row r *= k
        public void MultRow(uint r, float k)
        {
            Data[r * 4 + 0] *= k;
            Data[r * 4 + 1] *= k;
            Data[r * 4 + 2] *= k;
            Data[r * 4 + 3] *= k;
        }

        // Row dst += row src * k
        public void MultAndAdd(uint src, uint dst, float k)
        {
            Data[dst * 4 + 0] += Data[src * 4 + 0] * k;
            Data[dst * 4 + 1] += Data[src * 4 + 1] * k;
            Data[dst * 4 + 2] += Data[src * 4 + 2] * k;
            Data[dst * 4 + 3] += Data[src * 4 + 3] * k;
        }


        // Get transposed matrix
        public Mat4F Transposed()
        {
            return new Mat4F(0.0f)
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

        // Inverse matrix
        public Mat4F Inverse(float[] data)
        {
            Data = data;
            var res = Identity();
            for (uint i = 0; i < 4; i++)
            {
                var p = i;
                for (var j = i + 1; j < 4; j++)
                {
                    if (Math.Abs(Data[j * 4 + i]) > Math.Abs(Data[p * 4 + i])) p = j;
                }

                res.SwapRows(i, p);
                SwapRows(i, p);
                res.MultRow(i, 1.0f / Data[i * 4 + i]);
                MultRow(i, 1.0f / Data[i * 4 + i]);
                for (var j = i + 1; j < 4; j++)
                {
                    res.MultAndAdd(i, j, -Data[j * 4 + i]);
                    MultAndAdd(i, j, -Data[j * 4 + i]);
                }
            }

            for (var i = 3; i >= 0; i--)
            {
                for (uint j = 0; j < i; j++)
                {
                    res.MultAndAdd((uint) i, j, -Data[j * 4 + i]);
                    MultAndAdd((uint) i, j, -Data[j * 4 + i]);
                }
            }

            return this;
        }

        // Construct a translation matrix
        public static Mat4F Translation(Vec3<float> delta) => new Mat4F(1.0f)
        {
            Data =
            {
                [3] = delta.X,
                [7] = delta.Y,
                [11] = delta.Z
            }
        };

        // Construct a identity matrix
        public static Mat4F Identity() => new Mat4F(1.0f);

        // Construct a rotation matrix
        public static Mat4F Rotation(float degrees, Vec3<float> vec)
        {
            vec.Normalize();
            var alpha = degrees * Pi / 180.0f;
            var s = (float) Math.Sin(alpha);
            var c = (float) Math.Cos(alpha);
            var t = 1.0f - c;
            return new Mat4F(0.0f)
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

        // Construct a perspective projection matrix
        public static Mat4F Perspective(float fov, float aspect, float zNear, float zFar)
        {
            var f = 1.0f / Math.Tan(fov * Pi / 180.0 / 2.0);
            var a = zNear - zFar;
            return new Mat4F(0.0f)
            {
                Data =
                {
                    [0] = (float) (f / aspect),
                    [5] = (float) f,
                    [10] = (zFar + zNear) / a,
                    [11] = 2.0f * zFar * zNear / a,
                    [14] = -1.0f
                }
            };
        }

        // Construct an orthogonal projection matrix
        public static Mat4F Ortho(float left, float right, float top, float bottom, float zNear, float zFar)
        {
            var a = right - left;
            var b = top - bottom;
            var c = zFar - zNear;
            return new Mat4F(0.0f)
            {
                Data =
                {
                    [0] = 2.0f / a,
                    [3] = -(right + left) / a,
                    [5] = 2.0f / b,
                    [7] = -(top + bottom) / b,
                    [10] = -2.0f / c,
                    [11] = -(zFar + zNear) / c,
                    [15] = 1.0f
                }
            };
        }

        public KeyValuePair<Vec3<float>, float> Transform(Vec3<float> vec, float w)
        {
            var res = new Vec3<float>(Data[0] * vec.X + Data[1] * vec.Y + Data[2] * vec.Z + Data[3] * w,
                Data[4] * vec.X + Data[5] * vec.Y + Data[6] * vec.Z + Data[7] * w,
                Data[8] * vec.X + Data[9] * vec.Y + Data[10] * vec.Z + Data[11] * w);
            var rw = Data[12] * vec.X + Data[13] * vec.Y + Data[14] * vec.Z + Data[15] * w;
            return new KeyValuePair<Vec3<float>, float>(res, rw);
        }
    }

    public struct Mat4D
    {
        private const double Pi = 3.1415926535897932f;

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
            for (var i = 0; i < 16; ++i)
            {
                result.Data[i] += rhs.Data[i];
            }

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

        // Swap row r1, row r2
        public void SwapRows(uint r1, uint r2)
        {
            Generic.Swap(ref Data[r1 * 4 + 0], ref Data[r2 * 4 + 0]);
            Generic.Swap(ref Data[r1 * 4 + 1], ref Data[r2 * 4 + 1]);
            Generic.Swap(ref Data[r1 * 4 + 2], ref Data[r2 * 4 + 2]);
            Generic.Swap(ref Data[r1 * 4 + 3], ref Data[r2 * 4 + 3]);
        }

        // Row r *= k
        public void MultRow(uint r, double k)
        {
            Data[r * 4 + 0] *= k;
            Data[r * 4 + 1] *= k;
            Data[r * 4 + 2] *= k;
            Data[r * 4 + 3] *= k;
        }

        // Row dst += row src * k
        public void MultAndAdd(uint src, uint dst, double k)
        {
            Data[dst * 4 + 0] += Data[src * 4 + 0] * k;
            Data[dst * 4 + 1] += Data[src * 4 + 1] * k;
            Data[dst * 4 + 2] += Data[src * 4 + 2] * k;
            Data[dst * 4 + 3] += Data[src * 4 + 3] * k;
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

        // Inverse matrix
        public Mat4D Inverse(double[] data)
        {
            Data = data;
            var res = Identity();
            for (uint i = 0; i < 4; i++)
            {
                var p = i;
                for (var j = i + 1; j < 4; j++)
                {
                    if (Math.Abs(Data[j * 4 + i]) > Math.Abs(Data[p * 4 + i])) p = j;
                }

                res.SwapRows(i, p);
                SwapRows(i, p);
                res.MultRow(i, 1.0f / Data[i * 4 + i]);
                MultRow(i, 1.0f / Data[i * 4 + i]);
                for (var j = i + 1; j < 4; j++)
                {
                    res.MultAndAdd(i, j, -Data[j * 4 + i]);
                    MultAndAdd(i, j, -Data[j * 4 + i]);
                }
            }

            for (var i = 3; i >= 0; i--)
            {
                for (uint j = 0; j < i; j++)
                {
                    res.MultAndAdd((uint) i, j, -Data[j * 4 + i]);
                    MultAndAdd((uint) i, j, -Data[j * 4 + i]);
                }
            }

            return this;
        }

        // Construct a translation matrix
        public static Mat4D Translation(Vec3<double> delta) => new Mat4D(1.0f)
        {
            Data =
            {
                [3] = delta.X,
                [7] = delta.Y,
                [11] = delta.Z
            }
        };

        // Construct a identity matrix
        public static Mat4D Identity() => new Mat4D(1.0f);

        // Construct a rotation matrix
        public static Mat4D Rotation(double degrees, Vec3<double> vec)
        {
            vec.Normalize();
            var alpha = degrees * Pi / 180.0f;
            var s = Math.Sin(alpha);
            var c = Math.Cos(alpha);
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

        // Construct a perspective projection matrix
        public static Mat4D Perspective(double fov, double aspect, double zNear, double zFar)
        {
            var f = 1.0f / Math.Tan(fov * Pi / 180.0 / 2.0);
            var a = zNear - zFar;
            return new Mat4D(0.0f)
            {
                Data =
                {
                    [0] = f / aspect,
                    [5] = f,
                    [10] = (zFar + zNear) / a,
                    [11] = 2.0f * zFar * zNear / a,
                    [14] = -1.0f
                }
            };
        }

        // Construct an orthogonal projection matrix
        public static Mat4D Ortho(double left, double right, double top, double bottom, double zNear, double zFar)
        {
            var a = right - left;
            var b = top - bottom;
            var c = zFar - zNear;
            return new Mat4D(0.0f)
            {
                Data =
                {
                    [0] = 2.0f / a,
                    [3] = -(right + left) / a,
                    [5] = 2.0f / b,
                    [7] = -(top + bottom) / b,
                    [10] = -2.0f / c,
                    [11] = -(zFar + zNear) / c,
                    [15] = 1.0f
                }
            };
        }

        public KeyValuePair<Vec3<double>, double> Transform(Vec3<double> vec, double w)
        {
            var res = new Vec3<double>(Data[0] * vec.X + Data[1] * vec.Y + Data[2] * vec.Z + Data[3] * w,
                Data[4] * vec.X + Data[5] * vec.Y + Data[6] * vec.Z + Data[7] * w,
                Data[8] * vec.X + Data[9] * vec.Y + Data[10] * vec.Z + Data[11] * w);
            var rw = Data[12] * vec.X + Data[13] * vec.Y + Data[14] * vec.Z + Data[15] * w;
            return new KeyValuePair<Vec3<double>, double>(res, rw);
        }
    }
}