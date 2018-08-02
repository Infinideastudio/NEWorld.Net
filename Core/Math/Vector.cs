// 
// Core: Vector.cs
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

namespace Core.Math
{
    using static Generic;
    
    public struct Vec2<T> : IEquatable<Vec2<T>>
    {
        public Vec2(T x, T y)
        {
            X = x;
            Y = y;
        }

        public T X, Y;

        public double LengthSqr() => Square(X) + Square(Y);

        public double Length() => System.Math.Sqrt(LengthSqr());

        public void Normalize()
        {
            object length = Cast<T>(Length());
            X = Divide(X, length);
            Y = Divide(Y, length);
        }

        public static Vec2<T> operator +(Vec2<T> lhs, Vec2<T> rhs) =>
            new Vec2<T>(Add(lhs.X, rhs.X), Add(lhs.Y, rhs.Y));

        public static Vec2<T> operator -(Vec2<T> lhs, Vec2<T> rhs) =>
            new Vec2<T>(Substract(lhs.X, rhs.X), Substract(lhs.Y, rhs.Y));

        public bool Equals(Vec2<T> other) =>
            EqualityComparer<T>.Default.Equals(X, other.X) && EqualityComparer<T>.Default.Equals(Y, other.Y);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vec2<T> && Equals((Vec2<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(X) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Y);
            }
        }
    }

    public struct Vec3<T> : IEquatable<Vec3<T>>
    {
        public Vec3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public T X;
        public T Y;
        public T Z;

        public T LengthSqr() => Square(X) + Square(Y) + Square(Z);

        public double Length() => Sqrt(LengthSqr());

        public void Normalize()
        {
            object length = Cast<T>(Length());
            X = Divide(X, length);
            Y = Divide(Y, length);
            Z = Divide(Z, length);
        }


        public static Vec3<T> operator +(Vec3<T> lhs, Vec3<T> rhs) =>
            new Vec3<T>(Add(lhs.X, rhs.X), Add(lhs.Y, rhs.Y), Add(lhs.Z, rhs.Z));

        public static Vec3<T> operator -(Vec3<T> lhs, Vec3<T> rhs) =>
            new Vec3<T>(Substract(lhs.X, rhs.X), Substract(lhs.Y, rhs.Y), Substract(lhs.Z, rhs.Z));

        public static Vec3<T> operator -(Vec3<T> lhs) =>
            new Vec3<T>(Negate(lhs.X), Negate(lhs.Y), Negate(lhs.Z));

        public static Vec3<T> operator *(Vec3<T> lhs, T rhs) =>
            new Vec3<T>(Multiply(lhs.X, rhs), Multiply(lhs.Y, rhs), Multiply(lhs.Z, rhs));

        public static Vec3<T> operator /(Vec3<T> lhs, T rhs) =>
            new Vec3<T>(Divide(lhs.X, rhs), Divide(lhs.Y, rhs), Divide(lhs.Z, rhs));

        public T ChebyshevDistance(Vec3<T> rhs) =>
            (T)Max(Max(Abs(Substract(X, rhs.X)), Abs(Substract(Y, rhs.Y))), Abs(Substract(Z, rhs.Z)));

        public bool Equals(Vec3<T> other) =>
            EqualityComparer<T>.Default.Equals(X, other.X) && EqualityComparer<T>.Default.Equals(Y, other.Y) &&
            EqualityComparer<T>.Default.Equals(Z, other.Z);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vec3<T> vec3 && Equals(vec3);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(X);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Y);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Z);
                return hashCode;
            }
        }
    }
}