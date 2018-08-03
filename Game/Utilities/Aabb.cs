// 
// Game: Aabb.cs
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
using Core.Math;

namespace Game.Utilities
{
    public struct Aabb
    {
        // Min bound, Max bound
        public Vec3<double> Min, Max;

        public Aabb(Vec3<double> min, Vec3<double> max)
        {
            Min = min;
            Max = max;
        }

        public Aabb(ref Aabb rhs)
        {
            Min = rhs.Min;
            Max = rhs.Max;
        }

        // Is intersect on X axis
        public bool IntersectX(ref Aabb box)
        {
            return Min.X > box.Min.X && Min.X < box.Max.X || Max.X > box.Min.X && Max.X < box.Max.X ||
                   box.Min.X > Min.X && box.Min.X < Max.X || box.Max.X > Min.X && box.Max.X < Max.X;
        }

        // Is intersect on Y axis
        public bool IntersectY(ref Aabb box)
        {
            return Min.Y > box.Min.Y && Min.Y < box.Max.Y || Max.Y > box.Min.Y && Max.Y < box.Max.Y ||
                   box.Min.Y > Min.Y && box.Min.Y < Max.Y || box.Max.Y > Min.Y && box.Max.Y < Max.Y;
        }

        // Is intersect on Z axis
        public bool IntersectZ(ref Aabb box)
        {
            return Min.Z > box.Min.Z && Min.Z < box.Max.Z || Max.Z > box.Min.Z && Max.Z < box.Max.Z ||
                   box.Min.Z > Min.Z && box.Min.Z < Max.Z || box.Max.Z > Min.Z && box.Max.Z < Max.Z;
        }

        // Is intersect on all axes
        public bool Intersect(ref Aabb box)
        {
            return IntersectX(ref box) && IntersectY(ref box) && IntersectZ(ref box);
        }

        // Get Max move distance <= orgmove on X axis, when blocked by another Aabb
        public double MaxMoveOnXclip(Aabb box, double orgmove)
        {
            if (!(IntersectY(ref box) && IntersectZ(ref box)))
                return orgmove;
            if (Min.X >= box.Max.X && orgmove < 0.0)
                return Math.Max(box.Max.X - Min.X, orgmove);
            if (Max.X <= box.Min.X && orgmove > 0.0)
                return Math.Min(box.Min.X - Max.X, orgmove);

            return orgmove;
        }

        // Get Max move distance <= orgmove on Y axis, when blocked by another Aabb
        public double MaxMoveOnYclip(Aabb box, double orgmove)
        {
            if (!(IntersectX(ref box) && IntersectZ(ref box)))
                return orgmove;
            if (Min.Y >= box.Max.Y && orgmove < 0.0)
                return Math.Max(box.Max.Y - Min.Y, orgmove);
            if (Max.Y <= box.Min.Y && orgmove > 0.0)
                return Math.Min(box.Min.Y - Max.Y, orgmove);

            return orgmove;
        }

        // Get Max move distance <= orgmove on Z axis, when blocked by another Aabb
        public double MaxMoveOnZclip(Aabb box, double orgmove)
        {
            if (!(IntersectX(ref box) && IntersectY(ref box)))
                return orgmove;
            if (Min.Z >= box.Max.Z && orgmove < 0.0)
                return Math.Max(box.Max.Z - Min.Z, orgmove);
            if (Max.Z <= box.Min.Z && orgmove > 0.0)
                return Math.Min(box.Min.Z - Max.Z, orgmove);

            return orgmove;
        }

        // Get expanded Aabb
        public Aabb Expand(Vec3<double> arg)
        {
            Aabb res = this;

            if (arg.X > 0.0)
                res.Max.X += arg.X;
            else
                res.Min.X += arg.X;

            if (arg.Y > 0.0)
                res.Max.Y += arg.Y;
            else
                res.Min.Y += arg.Y;

            if (arg.Z > 0.0)
                res.Max.Z += arg.Z;
            else
                res.Min.Z += arg.Z;

            return res;
        }

        // Move Aabb
        public void Move(Vec3<double> arg)
        {
            Min += arg;
            Max += arg;
        }
    }
}