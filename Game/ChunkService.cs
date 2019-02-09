// 
// NEWorld/Game: ChunkService.cs
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
using Core;
using Game.World;

namespace Game
{
    /**
     * \brief This class manages worlds and chunks in NEWorld, and it's responsible
    *        for synchronizing these with the server in multiplayer situation.
     */
    // TODO: Hide this and only expose Task Dispatcher.
    public class ChunkService
    {
        /**
         * \brief constructor
         * \param isAuthority if a chunk service is authoritative, its chunk data
         *                    will be used when there are differences between
         *                    chunk data in different chunk services.
         *                    Normally, chunk services in the singleplayer mode
         *                    and in the server-side of the multiplayer mode
         *                    are authoritative.
         */
        static ChunkService()
        {
            IsAuthority = true;
            Worlds = new WorldManager();
            TaskDispatcher = Services.Get<TaskDispatcher>("Game.TaskDispatcher");
        }

        public static TaskDispatcher TaskDispatcher { get; private set; }

        public static WorldManager Worlds { get; private set; }

        public static bool IsAuthority { set; get; }

        public static void EnableDispatcher()
        {
            TaskDispatcher.Start();
        }
    }
}