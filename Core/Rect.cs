// 
// Core: Rect.cs
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

namespace Core
{
    using static Generic;

    public struct Rect<T>
    {
        public Vec2<T> Min, Max;

        public Rect(Vec2<T> min, Vec2<T> max)
        {
            Min = min;
            Max = max;
        }

        public Rect(T minX, T minY, T maxX, T maxY)
        {
            Min = new Vec2<T>(minX, minY);
            Max = new Vec2<T>(maxX, maxY);
        }

        public Vec2<T> Delta => Max - Min;

        public Rect(Vec2<T> start, params Vec2<T>[] args)
        {
            object minX = start.X, minY = start.Y;
            object maxX = start.X, maxY = start.Y;
            foreach (var point in args)
            {
                object boxX = point.X, boxY = point.Y;
                MinEqual(ref minX, boxX);
                MinEqual(ref minY, boxY);
                MaxEqual(ref maxX, boxX);
                MaxEqual(ref maxY, boxY);
            }

            Min = new Vec2<T>((T) minX, (T) minY);
            Max = new Vec2<T>((T) maxX, (T) maxY);
        }

        public Rect(params T[] args)
        {
            object minX = args[0], minY = args[1];
            object maxX = args[0], maxY = args[1];
            for (var i = 2; i < args.Length; ++i)
            {
                object boxX = args[i++], boxY = args[i];
                MinEqual(ref minX, boxX);
                MinEqual(ref minY, boxY);
                MaxEqual(ref maxX, boxX);
                MaxEqual(ref maxY, boxY);
            }

            Min = new Vec2<T>((T) minX, (T) minY);
            Max = new Vec2<T>((T) maxX, (T) maxY);
        }
    }
}