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
        private StrictDispose first, sibling;

        private bool released;

        public void Dispose()
        {
            if (released)
                return;
            TravelRelease();
            Release();
            released = true;
        }

        ~StrictDispose()
        {
            Dispose();
        }

        protected T Inject<T>(T target) where T : StrictDispose
        {
            if (first != null)
                target.sibling = first;
            first = target;
            return target;
        }

        protected void Reject<T>(T target, bool disposeNow = true) where T : StrictDispose
        {
            if (target == first)
            {
                first = target.sibling;
                return;
            }

            var current = first;
            while (current.sibling != target)
                current = current.sibling;
            current.sibling = target.sibling;

            if (disposeNow)
                target.Dispose();
        }

        protected virtual void Release()
        {
        }

        private void TravelRelease()
        {
            for (var current = first; current != null; current = current.sibling)
                current.Dispose();
        }

        public bool Valid()
        {
            return !released;
        }
    }
}