// 
// Core: Utilities.cs
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
        public static dynamic Min(dynamic a, dynamic b) => Less(a, b) ? a : b;
        public static dynamic Max(dynamic a, dynamic b) => Larger(a, b) ? a : b;

        public static void MinEqual(ref dynamic a, dynamic b)
        {
            if (Less(b, a)) a = b;
        }

        public static void MaxEqual(ref dynamic a, dynamic b)
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