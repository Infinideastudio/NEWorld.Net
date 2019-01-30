// 
// Core: EndianCheck.cs
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

namespace Core.Utilities
{
    public static class EndianCheck
    {
        private static bool _isBigEndian;

        private static bool _isEndianChecked;

        public static bool BigEndian => IsBigEndian();
        public static bool LittleEndian => !IsBigEndian();

        private static bool IsBigEndian()
        {
            if (!_isEndianChecked)
            {
                _isEndianChecked = true;
                const int nCheck = 0x01aa;
                _isBigEndian = (nCheck & 0xff) == 0x01;
            }

            return _isBigEndian;
        }
    }
}