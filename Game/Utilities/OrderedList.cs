// 
// Game: OrderedList.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Game.Utilities
{
    public class OrderedListIntBase<TD> : IEnumerable<KeyValuePair<int, TD>>
    {
        protected OrderedListIntBase(int fixedSize)
        {
            Size = 0;
            FixedSize = fixedSize;
            Data = new KeyValuePair<int, TD>[fixedSize];
        }

        protected void InsertBase(int first, int key, TD data)
        {
            if (first > Size || first >= FixedSize) return;
            Size = Math.Min(FixedSize, Size + 1);
            for (var j = FixedSize - 1; j > first; j--)
                Data[j] = Data[j - 1];
            Data[first] = new KeyValuePair<int, TD>(key, data);
        }

        public void Clear() => Size = 0;

        public int Size { get; private set; }
        public readonly int FixedSize;
        public readonly KeyValuePair<int, TD>[] Data;

        public IEnumerator<KeyValuePair<int, TD>> GetEnumerator() => new OrderedListIntBaseEnum<TD>(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class OrderedListIntBaseEnum<TD> : IEnumerator<KeyValuePair<int, TD>>
    {
        public OrderedListIntBaseEnum(OrderedListIntBase<TD> host) => _base = host;

        public bool MoveNext() => ++_position < _base.Size;

        public void Reset() => _position = -1;

        public KeyValuePair<int, TD> Current => _base.Data[_position];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        private int _position = -1;

        private readonly OrderedListIntBase<TD> _base;
    }

    public class OrderedListIntLess<TD> : OrderedListIntBase<TD>
    {
        public OrderedListIntLess(int fixedSize) : base(fixedSize)
        {
        }

        public void Insert(int key, TD data)
        {
            int first = 0, last = Size - 1;
            while (first <= last)
            {
                var middle = (first + last) / 2;
                if (key < Data[middle].Key)
                    last = middle - 1;
                else
                    first = middle + 1;
            }

            InsertBase(first, key, data);
        }
    }

    public class OrderedListIntGreater<TD> : OrderedListIntBase<TD>
    {
        public OrderedListIntGreater(int fixedSize) : base(fixedSize)
        {
        }

        public void Insert(int key, TD data)
        {
            int first = 0, last = Size - 1;
            while (first <= last)
            {
                var middle = (first + last) / 2;
                if (key > Data[middle].Key)
                    last = middle - 1;
                else
                    first = middle + 1;
            }

            InsertBase(first, key, data);
        }
    }
}