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

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Math;
using Game;
using Game.Terrain;
using Game.World;
using OpenGL;

namespace NEWorld.Renderer
{
    /**
     * \brief Manage the VBO of a world. It includes ChunkRenderer.
     */
    public class WorldRenderer : IDisposable
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
                            crd.Generate(chunk);
                            cs.TaskDispatcher.Add(new VboGenerateTask(world, chunkPosition, crd,
                                _worldRenderer._chunkRenderers));
                            if (counter++ == 3) break;
                        }
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
            _chunkRenderers = new Dictionary<Vec3<int>, ChunkRenderer>();
            _prog = new Program();
            using (Shader vertex = new Shader(Gl.VertexShader, @"
#version 450 core
layout(shared, row_major) uniform;
layout (std140, binding = 0) uniform vertexMvp { mat4 ProjMtx; };
layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TexCoord;
layout (location = 2) in vec4 Color;
out vec2 Frag_UV;
out vec4 Frag_Color;
void main() {
   Frag_UV = TexCoord;
   Frag_Color = Color;
   gl_Position = ProjMtx * vec4(Position.xyz, 1);
}"),
                fragment = new Shader(Gl.FragmentShader, @"
#version 450 core
precision mediump float;
layout (binding = 1) uniform sampler2D Texture;
in vec2 Frag_UV;
in vec4 Frag_Color;
out vec4 Out_Color;
void main(){
   Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
}"))
                _prog.Link(new[] {vertex, fragment});
            
            _ubo = new DataBuffer(16 * sizeof(float));
            _vao = new VertexArray();

            _vao.EnableAttrib(0);
            _vao.EnableAttrib(1);
            _vao.EnableAttrib(2);
            _vao.AttribFormat(0, 3, Gl.Float, false, 5 * sizeof(float));
            _vao.AttribFormat(1, 2, Gl.Float, false, 0);
            _vao.AttribFormat(2, 3, Gl.Float, false, 2 * sizeof(float));
            _vao.AttribBinding(0, 0);
            _vao.AttribBinding(1, 0);
            _vao.AttribBinding(2, 0);
        }

        public void RenderBuffer(ConstDataBuffer buffer, int verts)
        {
            _vao.BindBuffer(0, buffer, 0, 8 * sizeof(float));
            Gl.DrawArrays(Gl.Quads, 0, verts);
        }

        // Render all chunks
        private int Render(Vec3<int> position)
        {
            var chunkPending = new List<KeyValuePair<Vec3<int>, ChunkRenderer>>();

            var chunkpos = World.GetChunkPos(position);
            _vao.Use();
            _ubo.BindBase(Gl.UniformBuffer, 0);
            foreach (var c in _chunkRenderers)
            {
                if (chunkpos.ChebyshevDistance(c.Key) > RenderDist) continue;
                c.Value.Render(c.Key, this);
                chunkPending.Add(c);
            }

            Gl.Enable(Gl.Blend);
            Gl.BlendFunc(Gl.SrcAlpha, Gl.OneMinusSrcAlpha);
            foreach (var c in chunkPending)
            {
                c.Value.RenderTrans(c.Key, this);
            }

            Gl.Disable(Gl.Blend);
            return chunkPending.Count;
        }

        public int Render(Vec3<double> v) => Render(new Vec3<int>((int) v.X, (int) v.Y, (int) v.Z)); 

        public void RegisterTask(ChunkService chunkService, Player player) => 
            chunkService.TaskDispatcher.AddRegular(new RenderDetectorTask(this, _world.Id, player));

        public void FlushMatrix() => _ubo.DataSection(0, Matrix.Get().Data);

        private readonly World _world;

        // Ranges
        public readonly int RenderDist;

        // Chunk Renderers
        private readonly Dictionary<Vec3<int>, ChunkRenderer> _chunkRenderers;
        private readonly Program _prog;
        private readonly VertexArray _vao;
        private readonly DataBuffer _ubo;

        public void Dispose()
        {
            _prog?.Dispose();
            _vao?.Dispose();
            _ubo?.Dispose();
            foreach (var renderer in _chunkRenderers)
                renderer.Value.Dispose();
        }
    }
}