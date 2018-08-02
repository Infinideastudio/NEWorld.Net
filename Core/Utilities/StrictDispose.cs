// 
// Core: StrictDispose.cs
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

namespace Core.Utilities
{
    public class StrictDispose : IDisposable
    {
        ~StrictDispose() => Dispose();

        protected T Inject<T>(T target) where T : StrictDispose
        {
            if (_first != null)
                target._sibling = _first;
            _first = target;
            return target;
        }

        protected void Reject<T>(T target, bool disposeNow = true) where T : StrictDispose
        {
            if (target == _first)
            {
                _first = target._sibling;
                return;
            }

            var current = _first;
            while (current._sibling != target)
                current = current._sibling;
            current._sibling = target._sibling;

            if (disposeNow)
                target.Dispose();
        }

        protected virtual void Release()
        {
        }

        private void TravelRelease()
        {
            for (var current = _first; current != null; current = current._sibling)
                current.Dispose();
        }

        public void Dispose()
        {
            if (_released)
                return;
            TravelRelease();
            Release();
            _released = true;
        }

        public bool Valid() => !_released;

        private StrictDispose _first, _sibling;

        private bool _released;
    }
}