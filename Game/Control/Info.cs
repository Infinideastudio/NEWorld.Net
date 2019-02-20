// 
// NEWorld/Game: Info.cs
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
using MessagePack;

namespace Game.Control
{
    // IMPORTANT NOTICE: This structure is NOT necessary the same for all players on the same servers
    // only the information a user has been granted access to should be sent to a target user
    [MessagePackObject]
    public class UniverseInfo
    {
        #region Basic Voxel Information (By Order Of Loading) Needing For Entering the Game

        [Key(0)] public RequiredModulesInfo RequiredModules { get; set; }

        [Key(1)] public BlocksInfo RequireBlocks { get; set; }

        [Key(2)] public AvailableWorlds AvailableWorlds { get; set; }

        #endregion
    }

    [MessagePackObject]
    public class ModuleInfo
    {
        [Key(0)] public string Name { get; set; }

        [Key(1)] public string Uri { get; set; }

        [Key(2)] public uint[] Version { get; set; }
    }

    [MessagePackObject]
    public class RequiredModulesInfo
    {
        [Key(0)] public List<ModuleInfo> Modules { get; set; }
    }

    [MessagePackObject]
    public struct BlockInfo
    {
        [Key(0)] public string Name { get; set; }

        [Key(1)] public uint Id { get; set; }
    }

    [MessagePackObject]
    public class BlocksInfo
    {
        [Key(0)] public List<BlockInfo> Blocks { get; set; }
    }

    [MessagePackObject]
    public class WorldInfo
    {
    }

    [MessagePackObject]
    public class AvailableWorlds
    {
        [Key(0)] public List<WorldInfo> Worlds { get; set; }
    }
}