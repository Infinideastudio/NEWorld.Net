// 
// NEWorld/Game: ChunkReleaseTimer.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2019 NEWorld Team
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
using System.Threading;

namespace Game.World
{
    public partial class Chunk
    {
        private long mLastRequestTime;

        public bool CheckReleaseable()
        {
            return DateTime.Now - new DateTime(Interlocked.Read(ref mLastRequestTime)) > TimeSpan.FromSeconds(10);
        }

        public void MarkRequest()
        {
            mLastRequestTime = DateTime.Now.Ticks;
        }
    }
}