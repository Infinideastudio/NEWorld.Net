// 
// NEWorld: GameScene.cs
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
using System.Threading;
using Core;
using Core.Math;
using Core.Utilities;
using Game;
using Game.Network;
using Game.Terrain;
using Game.World;
using NEWorld.Renderer;
using OpenGL;
using SDL2;
using static NuklearSharp.Nuklear;

namespace NEWorld
{
    public class GameScene
    {
        private class PutBlockTask : IReadWriteTask
        {
            public PutBlockTask(uint worldId, Vec3<int> blockPosition, ushort blockId)
            {
                _worldId = worldId;
                _blockPosition = blockPosition;
                _blockId = blockId;
            }

            public void Task(ChunkService srv)
            {
                srv.Worlds.Get(_worldId).SetBlock(ref _blockPosition, new BlockData(_blockId));
            }

            public IReadWriteTask Clone() => (IReadWriteTask) MemberwiseClone();

            private readonly uint _worldId;
            private Vec3<int> _blockPosition;
            private readonly ushort _blockId;
        }

        private class PlayerControlTask : IReadOnlyTask
        {
            private const double SelectDistance = 5.0;
            private const double SelectPrecision = 200.0;

            public PlayerControlTask(Player player)
            {
                _player = player;
            }

            public unsafe void Task(ChunkService cs)
            {
                const double speed = 0.1;

                // TODO: Read keys from the configuration file
                var state = Window.GetKeyBoardState();
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_UP] != 0)
                    _player.AccelerateRotation(new Vec3<double>(1, 0.0, 0.0));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_DOWN] != 0 && _player.Rotation.X > -90)
                    _player.AccelerateRotation(new Vec3<double>(-1, 0.0, 0.0));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] != 0)
                    _player.AccelerateRotation(new Vec3<double>(0.0, -1, 0.0));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LEFT] != 0)
                    _player.AccelerateRotation(new Vec3<double>(0.0, 1, 0.0));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_W] != 0)
                    _player.Accelerate(new Vec3<double>(0.0, 0.0, -speed));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_S] != 0)
                    _player.Accelerate(new Vec3<double>(0.0, 0.0, speed));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_A] != 0)
                    _player.Accelerate(new Vec3<double>(-speed, 0.0, 0.0));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_D] != 0)
                    _player.Accelerate(new Vec3<double>(speed, 0.0, 0.0));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_E] != 0)
                    _player.Accelerate(new Vec3<double>(0.0, 0.0, -speed * 10));
                if (state[(int) SDL.SDL_Scancode.SDL_SCANCODE_SPACE] != 0)
                    _player.Accelerate(new Vec3<double>(0.0, 2 * speed, 0.0));
                _player.Accelerate(new Vec3<double>(0.0, -2 * speed, 0.0));

                // Handle left-click events
                HandleLeftClickEvent(cs);
                //    mGUIWidgets.update();
            }

            private void HandleLeftClickEvent(ChunkService cs)
            {
                if (!Window.GetInstance().GetMouseMotion().Left) return;
                // Selection
                var world = cs.Worlds.Get(_player.WorldId);
                var trans = new Mat4D(1.0f);
                var position = _player.Position;
                var rotation = _player.Rotation;
                trans *= Mat4D.Rotation(rotation.Y, new Vec3<double>(0.0, 1.0, 0.0));
                trans *= Mat4D.Rotation(rotation.X, new Vec3<double>(1.0, 0.0, 0.0));
                trans *= Mat4D.Rotation(rotation.Z, new Vec3<double>(0.0, 0.0, 1.0));
                var dir = trans.Transform(new Vec3<double>(0.0, 0.0, -1.0), 0.0f).Key;
                dir.Normalize();

                for (double i = 0.0f; i < SelectDistance; i += 1.0f / SelectPrecision)
                {
                    var pos = position + dir * i;
                    var blockPos = new Vec3<int>((int) Math.Floor(pos.X), (int) Math.Floor(pos.Y),
                        (int) Math.Floor(pos.Z));
                    try
                    {
                        if (world.GetBlock(blockPos).Id == 0) continue;
                        cs.TaskDispatcher.Add(new PutBlockTask(_player.WorldId, blockPos, 0));
                        break;
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            private readonly Player _player;
        }

        private class UpsCounter : IReadOnlyTask
        {
            public UpsCounter(object updateCounter) => _updateCounter = updateCounter;

            public void Task(ChunkService srv) => Generic.Increase(_updateCounter);

            public IReadOnlyTask Clone() => (IReadOnlyTask) MemberwiseClone();

            private readonly object _updateCounter;
        }

        // GameScene update frequency
        public const int UpdateFrequency = 30;

        private static bool IsClient()
        {
            return true;
        }

        public GameScene(string name, Window window)
        {
            _window = window;
            _player = new Player(0);
            _guiWidgets = new WidgetManager(_window.GetNkContext());

            _player.Position = new Vec3<double>(-16.0, 48.0, 32.0);
            _player.Rotation = new Vec3<double>(-45.0, -22.5, 0.0);
            Window.LockCursor();

            if (IsClient())
            {
                Singleton<ChunkService>.Instance.IsAuthority = false;
            }
            else
            {
                // Initialize server
                var server = Services.Get<Server>("Game.Server");
                server.Enable(31111);
                server.Run();
            }

            // Initialize connection
            Services.Get<Client>("Game.Client").Enable("127.0.0.1", 31111);

            _currentWorld = Singleton<ChunkService>.Instance.Worlds.Get(RequestWorld());
            _worldRenderer = new WorldRenderer(_currentWorld, 4);

            // Initialize update events
            _currentWorld.RegisterChunkTasks(Singleton<ChunkService>.Instance, _player);
            _worldRenderer.RegisterTask(Singleton<ChunkService>.Instance, _player);
            Singleton<ChunkService>.Instance.TaskDispatcher.AddRegular(new PlayerControlTask(_player));
            Singleton<ChunkService>.Instance.TaskDispatcher.AddRegular(new UpsCounter(_upsCounter));

            // Initialize rendering
            _texture = BlockTextures.BuildAndFlush();
            BlockRenderers.FlushTextures(Services.Get<IBlockTextures>("BlockTextures"));
            Gl.Enable(Gl.DepthTest);
            Gl.DepthFunc(Gl.Lequal);

            // Initialize Widgets
            _guiWidgets.Add(new WidgetCallback(
                "Debug", nk_rect_(20, 20, 300, 300),
                NK_WINDOW_BORDER | NK_WINDOW_MOVABLE | NK_WINDOW_SCALABLE |
                NK_WINDOW_CLOSABLE | NK_WINDOW_MINIMIZABLE | NK_WINDOW_TITLE, ctx =>
                {
                    if (_rateCounterScheduler.IsDue())
                    {
                        // Update FPS & UPS
                        _fpsLatest = _fpsCounter;
                        _upsLatest = (uint) _upsCounter;
                        _fpsCounter = 0;
                        Generic.MultiplyBy(ref _upsCounter, 0u);
                        _rateCounterScheduler.IncreaseTimer();
                    }

                    ctx.LayoutRowDynamic(15, 1);
                    ctx.Label($"NEWorld {"0.5.0 Alpha"} ({39})", NK_TEXT_LEFT);
                    ctx.Label($"FPS {_fpsLatest}, UPS {_upsLatest}", NK_TEXT_LEFT);
                    ctx.Label($"Position: x {_player.Position.X} y {_player.Position.Y} z {_player.Position.Z}",
                        NK_TEXT_LEFT);
                    ctx.Label($"GUI Widgets: {_guiWidgets.Count}", NK_TEXT_LEFT);
                    ctx.Label($"Chunks Loaded: {_currentWorld.GetChunkCount()}", NK_TEXT_LEFT);
                    ctx.Label("Modules Loaded: %zu", NK_TEXT_LEFT);
                    ctx.Label("Update threads workload:", NK_TEXT_LEFT);
                    var dispatcher = Singleton<ChunkService>.Instance.TaskDispatcher;
                    var threadId = 0;
                    foreach (var timeReal in dispatcher.TimeUsed)
                    {
                        var time = Math.Max(timeReal, 0L);
                        ctx.Label($"Thread {threadId++}: {time} ms {time / 33.3333}", NK_TEXT_LEFT);
                    }

                    ctx.Label(
                        $"Regular Tasks: read {dispatcher.GetRegularReadOnlyTaskCount()} write {dispatcher.GetRegularReadWriteTaskCount()}",
                        NK_TEXT_LEFT);
                }));

            Singleton<ChunkService>.Instance.TaskDispatcher.Start();
        }

        ~GameScene()
        {
            Singleton<ChunkService>.Instance.TaskDispatcher.Stop();
        }

        public void Render()
        {
            Singleton<ChunkService>.Instance.TaskDispatcher.ProcessRenderTasks();

            // Camera control by mouse
            const double mouseSensitivity = 0.3;

            var mouse = Window.GetInstance().GetMouseMotion();
            _player.AccelerateRotation(new Vec3<double>(-mouse.Y * mouseSensitivity, -mouse.X * mouseSensitivity, 0.0));

            Gl.ClearColor(0.6f, 0.9f, 1.0f, 1.0f);
            Gl.ClearDepth(1.0f);
            Gl.Enable(Gl.DepthTest);
            Gl.Enable(Gl.CullFace);
            Gl.CullFaceOption(Gl.Back);

            var timeDelta = _updateScheduler.GetDeltaTimeMs() / 1000.0 * UpdateFrequency;
            if (timeDelta > 1.0) timeDelta = 1.0;
            var playerRenderedPosition = _player.Position - _player.PositionDelta * (1.0 - timeDelta);
            var playerRenderedRotation = _player.Rotation - _player.RotationDelta * (1.0 - timeDelta);

            _texture.Use(0);
            Gl.Clear(Gl.ColorBufferBit | Gl.DepthBufferBit);
            Gl.Viewport(0, 0, _window.GetWidth(), _window.GetHeight());
            Matrix.RestoreProjection();
            Matrix.ApplyPerspective(70.0f, (float) _window.GetWidth() / _window.GetHeight(), 0.1f, 3000.0f);
            Matrix.ViewRotate((float) -playerRenderedRotation.X, new Vec3<float>(1.0f, 0.0f, 0.0f));
            Matrix.ViewRotate((float) -playerRenderedRotation.Y, new Vec3<float>(0.0f, 1.0f, 0.0f));
            Matrix.ViewRotate((float) -playerRenderedRotation.Z, new Vec3<float>(0.0f, 0.0f, 1.0f));
            Matrix.ViewTranslate(-playerRenderedPosition);

            // Render
            _worldRenderer.Render(_player.Position);
            // mPlayer.render();

            Gl.Disable(Gl.DepthTest);

            _guiWidgets.Render();

            _fpsCounter++;
        }

        private uint RequestWorld()
        {
            // TODO: change this
            if (IsClient())
            {
                var worldIds = Client.GetAvailableWorldId.Call();
                if (worldIds.Length == 0)
                {
                    throw new Exception("The server didn't response with any valid worlds.");
                }

                var worldInfo = Client.GetWorldInfo.Call(worldIds[0]);

                Singleton<ChunkService>.Instance.Worlds.Add(worldInfo["name"]);
            }

            // It's a simple wait-until-we-have-a-world procedure now.
            // But it should be changed into get player information
            // and get the world id from it.
            while (Singleton<ChunkService>.Instance.Worlds.Get(0) == null)
                Thread.Yield();
            return 0;
        }

        // Local server
        private readonly Server _server;

        // Window
        private readonly Window _window;

        // Texture test
        private readonly Texture _texture;

        // Player
        private readonly Player _player;

        // Widget manager
        private readonly WidgetManager _guiWidgets;

        // Update scheduler
        private RateController _updateScheduler = new RateController(UpdateFrequency);

        // Rate counters
        private uint _fpsCounter, _fpsLatest, _upsLatest;
        private object _upsCounter = new uint();

        // Current world
        private readonly World _currentWorld;

        // World renderer
        private readonly WorldRenderer _worldRenderer;

        private RateController _rateCounterScheduler = new RateController(1);
    }
}