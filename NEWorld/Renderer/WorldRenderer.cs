// 
// GUI: worldrenderer.h
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

using System.Collections.Generic;
using System.Linq;
using Core;
using Game;
using OpenGL;

namespace NEWorld.Renderer
{
    /**
     * \brief Manage the VBO of a world. It includes ChunkRenderer.
     */
    public class WorldRenderer
    {
        public const int MaxChunkRenderCount = 4;

        private class RenderDetectorTask : IReadOnlyTask
        {
            public RenderDetectorTask(WorldRenderer worldRenderer, uint currentWorldId, Player player)
            {
                _worldRenderer = worldRenderer;
                _currentWorldId = currentWorldId;
                _player = player;
            }

            public void Task(ChunkService cs)
            {
                var counter = 0;
                // TODO: improve performance by adding multiple instances of this and set a step when itering the chunks.
                var position = _player.Position;
                var positionInt = new Vec3<int>((int)position.X, (int)position.Y, (int)position.Z);
                var chunkpos = World.GetChunkPos(positionInt);
                var world = cs.Worlds.Get(_currentWorldId);
                foreach (var c in world.Chunks)
                {
                    var chunk = c.Value;
                    var chunkPosition = chunk.Position;
                    // In render range, pending to render
                    if (chunk.IsUpdated && chunkpos.ChebyshevDistance(chunkPosition) <= _worldRenderer.RenderDist)
                    {
                        if (NeighbourChunkLoadCheck(world, chunkPosition))
                        {
                            // TODO: maybe build a VA pool can speed this up.
                            var crd = new ChunkRenderData();
                            crd.generate(chunk);
                            cs.TaskDispatcher.AddRenderTask(new VboGenerateTask(world, chunkPosition, crd,
                                _worldRenderer.ChunkRenderers));
                            if (counter++ == 3) break;
                        }
                    }
                    else
                    {
                        // TODO: Unload unneeded VBO.
                        //       We can't do it here since it's not thread safe
                        /*
                        auto iter = mChunkRenderers.find(chunkPosition);
                        if (iter != mChunkRenderers.end())
                        mChunkRenderers.erase(iter);
                        */
                    }
                }
            }

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            private static readonly Vec3<int>[] Delta =
            {
                new Vec3<int>(1, 0, 0), new Vec3<int>(-1, 0, 0),
                new Vec3<int>(0, 1, 0), new Vec3<int>(0, -1, 0),
                new Vec3<int>(0, 0, 1), new Vec3<int>(0, 0, -1)
            };

            private static bool NeighbourChunkLoadCheck(World world, Vec3<int> pos) =>
                Delta.All(p => world.IsChunkLoaded(pos + p));

            private readonly WorldRenderer _worldRenderer;
            private readonly uint _currentWorldId;
            private readonly Player _player;
        }

        private class VboGenerateTask : IRenderTask
        {
            public VboGenerateTask(World world, Vec3<int> position, ChunkRenderData crd,
                Dictionary<Vec3<int>, ChunkRenderer> chunkRenderers)
            {
                _world = world;
                _position = position;
                _chunkRenderData = crd;
                _chunkRenderers = chunkRenderers;
            }

            public void Task(ChunkService srv)
            {
                if (!_world.Chunks.TryGetValue(_position, out var chunk)) return;
                chunk.IsUpdated = false;
                if (_chunkRenderers.TryGetValue(_position, out var it))
                {
                    it.Update(_chunkRenderData);
                }
                else
                {
                    _chunkRenderers.Add(_position, new ChunkRenderer(_chunkRenderData));
                }
            }

            public IRenderTask Clone() => (IRenderTask) MemberwiseClone();

            private readonly World _world;
            private readonly Vec3<int> _position;
            private readonly ChunkRenderData _chunkRenderData;
            private readonly Dictionary<Vec3<int>, ChunkRenderer> _chunkRenderers;
        }


        public WorldRenderer(World world, int renderDistance)
        {
            _world = world;
            RenderDist = renderDistance;
        }

        // Render all chunks
        public int render(Vec3<int> position)
        {
            var chunkPending = new List<KeyValuePair<Vec3<int>, ChunkRenderer>>();

            var chunkpos = World.GetChunkPos(position);
            foreach (var c in ChunkRenderers)
            {
                if (chunkpos.ChebyshevDistance(c.Key) > RenderDist) continue;
                c.Value.render(c.Key);
                chunkPending.Add(c);
            }

            Gl.Enable(Gl.Blend);
            Gl.BlendFunc(Gl.SrcAlpha, Gl.OneMinusSrcAlpha);
            foreach (var c in chunkPending)
            {
                c.Value.renderTrans(c.Key);
            }

            Gl.Disable(Gl.Blend);
            return chunkPending.Count;
        }

        public void registerTask(ChunkService chunkService, Player player) => 
            chunkService.TaskDispatcher.AddRegularReadOnlyTask(new RenderDetectorTask(this, _world.Id, player));

        private readonly World _world;

        // Ranges
        public readonly int RenderDist;

        // Chunk Renderers
        public Dictionary<Vec3<int>, ChunkRenderer> ChunkRenderers;
    }
}