// 
// Core: Arithmetic.cs
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
    public static partial class Generic
    {
        public static dynamic Cast<T>(dynamic a) => (T) a;
        public static dynamic Add(dynamic a, dynamic b) => a + b;
        public static dynamic Substract(dynamic a, dynamic b) => a - b;
        public static dynamic Multiply(dynamic a, dynamic b) => a * b;
        public static dynamic Divide(dynamic a, dynamic b) => a / b;
        public static dynamic Modulus(dynamic a, dynamic b) => a % b;
        public static dynamic AddBy(ref dynamic a, dynamic b) => a += b;
        public static dynamic SubstractBy(ref dynamic a, dynamic b) => a -= b;
        public static dynamic MultiplyBy(ref dynamic a, dynamic b) => a *= b;
        public static dynamic DivideBy(ref dynamic a, dynamic b) => a /= b;
        public static dynamic ModulusBy(ref dynamic a, dynamic b) => a %= b;
        public static dynamic Square(dynamic a) => a * a;
        public static dynamic Negate(dynamic a) => -a;
        public static dynamic Increase(dynamic a) => ++a;
        public static dynamic Decrease(dynamic a) => --a;
        public static dynamic IncreaseAfter(dynamic a) => a++;
        public static dynamic DecreaseAfter(dynamic a) => a--;
    }
}