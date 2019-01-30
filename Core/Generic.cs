// 
// Core: Generic.cs
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
    public static class Generic
    {
        public static dynamic Cast<T>(dynamic a)
        {
            return (T) a;
        }

        public static dynamic Add(dynamic a, dynamic b)
        {
            return a + b;
        }

        public static dynamic Substract(dynamic a, dynamic b)
        {
            return a - b;
        }

        public static dynamic Multiply(dynamic a, dynamic b)
        {
            return a * b;
        }

        public static dynamic Divide(dynamic a, dynamic b)
        {
            return a / b;
        }

        public static dynamic Modulus(dynamic a, dynamic b)
        {
            return a % b;
        }

        public static dynamic AddBy(dynamic a, dynamic b)
        {
            return a += b;
        }

        public static dynamic SubstractBy(dynamic a, dynamic b)
        {
            return a -= b;
        }

        public static dynamic MultiplyBy(ref dynamic a, dynamic b)
        {
            return a *= b;
        }

        public static dynamic DivideBy(dynamic a, dynamic b)
        {
            return a /= b;
        }

        public static dynamic ModulusBy(dynamic a, dynamic b)
        {
            return a %= b;
        }

        public static dynamic Square(dynamic a)
        {
            return a * a;
        }

        public static dynamic Negate(dynamic a)
        {
            return -a;
        }

        public static dynamic Increase(dynamic a)
        {
            return ++a;
        }

        public static dynamic Decrease(dynamic a)
        {
            return --a;
        }

        public static dynamic IncreaseAfter(dynamic a)
        {
            return a++;
        }

        public static dynamic DecreaseAfter(dynamic a)
        {
            return a--;
        }

        public static bool Less(dynamic a, dynamic b)
        {
            return a < b;
        }

        public static bool LessEqual(dynamic a, dynamic b)
        {
            return a <= b;
        }

        public static bool Larger(dynamic a, dynamic b)
        {
            return a > b;
        }

        public static bool LargerEqual(dynamic a, dynamic b)
        {
            return a >= b;
        }

        public static bool Equal(dynamic a, dynamic b)
        {
            return a == b;
        }

        public static double Sqrt(dynamic a)
        {
            return System.Math.Sqrt((double) a);
        }

        public static double Sin(dynamic a)
        {
            return System.Math.Sin((double) a);
        }

        public static double Cos(dynamic a)
        {
            return System.Math.Cos((double) a);
        }

        public static double Tan(dynamic a)
        {
            return System.Math.Tan((double) a);
        }

        public static double Abs(dynamic a)
        {
            return System.Math.Abs(a);
        }

        public static dynamic Min(dynamic a, dynamic b)
        {
            return Less(a, b) ? a : b;
        }

        public static dynamic Max(dynamic a, dynamic b)
        {
            return Larger(a, b) ? a : b;
        }

        public static void MinEqual(dynamic a, dynamic b)
        {
            if (Less(b, a)) a = b;
        }

        public static void MaxEqual(dynamic a, dynamic b)
        {
            if (Larger(b, a)) a = b;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }
    }
}