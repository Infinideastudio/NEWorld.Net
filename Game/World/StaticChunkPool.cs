// 
// NEWorld/Game: StaticChunkPool.cs
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
using System.Collections.Generic;

namespace Game.World
{
    public static class StaticChunkPool
    {
        private static uint _airChunkId = uint.MaxValue;
        private static List<Chunk> _staticList = new List<Chunk>();
        private static Dictionary<string, uint> _id;

        static StaticChunkPool()
        {
            _id = new Dictionary<string, uint>();
            Register("Default.AirChunk", new Chunk(new BlockData(0)));
        }

        internal static Dictionary<string, uint> Id
        {
            get => _id;
            set
            {
                var oldId = value;
                var old = _staticList;
                _id = value;
                _staticList = new List<Chunk>(_id.Count);
                for (var i = 0; i < _id.Count; ++i)
                    _staticList.Add(null);
                foreach (var record in value)
                    _staticList[(int) record.Value] = old[(int) oldId[record.Key]];
            }
        }

        public static void Register(string name, Chunk staticChunk)
        {
            if (_id.TryGetValue(name, out var sid))
                if (_staticList[(int) sid] == null)
                    _staticList[(int) sid] = staticChunk;

            _id.Add(name, (uint) _staticList.Count);
            _staticList.Add(staticChunk);
        }

        public static uint GetAirChunk()
        {
            if (_airChunkId == uint.MaxValue)
                _airChunkId = _id["Default.AirChunk"];
            return _airChunkId;
        }

        public static Chunk GetChunk(uint id)
        {
            return _staticList[(int) id];
        }

        public static uint GetId(string name)
        {
            return _id[name];
        }
    }
}