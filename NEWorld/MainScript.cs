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

using Core.Math;
using Core.Utilities;
using Game;
using Game.Network;
using Game.Terrain;
using Game.World;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xenko.Core.Diagnostics;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Graphics;
using Xenko.Rendering;
using Buffer = Xenko.Graphics.Buffer;

namespace NEWorld
{
    public static class Context
    {
        private static IGame _game;

        public static IGame Game
        {
            get => _game;
            set
            {
                _game = value;
                IndexBuffer = IndexBufferBuilder.Build();
            }
        }

        public static Scene OperatingScene { get; set; }

        public static GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

        public static GraphicsContext GraphicsContext => Game.GraphicsContext;

        public static CommandList CommandList => Game.GraphicsContext.CommandList;

        public static readonly VertexDeclaration VertexLayout = new VertexDeclaration(
            VertexElement.TextureCoordinate<Vector2>(),
            VertexElement.Color<Vector3>(),
            VertexElement.Position<Vector3>()
        );

        public static Buffer IndexBuffer { get; private set; }
    }
    
    public static class IndexBufferBuilder
    {
        public static Buffer Build()
        {
            var idx = new int[262144 / 2 * 3];
            var cnt = 0;
            for (var i = 0; i < 262144 / 4; ++i)
            {
                var b = i * 4;
                idx[cnt++] = b;
                idx[cnt++] = b + 1;
                idx[cnt++] = b + 2;
                idx[cnt++] = b;
                idx[cnt++] = b + 2;
                idx[cnt++] = b + 3;
            }

            return Buffer.Index.New(Context.GraphicsDevice, idx);
        }
    }

    public class VertexBuilder : IVertexBuilder
    {
        public VertexBuilder(int size) => Data = new float[size];

        public void AddPrimitive(int verts, params float[] data)
        {
            Count += verts;
            data.CopyTo(Data, Size);
            Size += data.Length;
        }

        public Mesh Dump()
        {
            return new Mesh
            {
                Draw =
                {
                    DrawCount = 1, 
                    IndexBuffer = new IndexBufferBinding(Context.IndexBuffer, true, Count / 2 * 3),
                    PrimitiveType = PrimitiveType.TriangleList,
                    StartLocation = 0,
                    VertexBuffers = new[]
                    {
                       new VertexBufferBinding(Buffer.Vertex.New(Context.Game.GraphicsDevice, Data), Context.VertexLayout, Count)
                    }
                }
            };
        }
        
        public int Size;
        public int Count;
        public readonly float[] Data;
    }

    public class MainScript : SyncScript
    {
        private void InitializeModules()
        {
            Core.Module.Modules.Instance.Load("Main");
        }

        private void InitializeContext()
        {
            Context.Game = Game;
            Context.OperatingScene = SceneSystem.SceneInstance.RootScene;
            Core.LogPort.Logger = Log;
            Log.ActivateLog(LogMessageType.Debug);
        }

        private void EstablishChunkService()
        {
            if (IsClient())
            {
                Singleton<ChunkService>.Instance.IsAuthority = false;
            }
            else
            {
                // Initialize server
                server = Core.Services.Get<Server>("Game.Server");
                server.Enable(31111);
                server.Run();
            }
        }

        private async Task EstablishGameConnection()
        {
            await Core.Services.Get<Client>("Game.Client").Enable("127.0.0.1", 31111);
        }

        private void LoadPlayer()
        {
            player = new Player(0)
            {
                Position = new Vec3<double>(-16.0, 48.0, 32.0), Rotation = new Vec3<double>(-45.0, -22.5, 0.0)
            };
        }

        private async Task EnterCurrentWorld()
        {
            currentWorld = Singleton<ChunkService>.Instance.Worlds.Get(await RequestWorld());
        }

        private void StartTerrainRenderService()
        {
            rdWorld = new Renderer.RdWorld(currentWorld, player, 4);
        }

        private async void Initialize()
        {
            InitializeContext();
            InitializeModules();
            EstablishChunkService();
            await EstablishGameConnection();
            LoadPlayer();
            await EnterCurrentWorld();
            StartTerrainRenderService();
        }

        public override void Start()
        {
            Initialize();
        }

        private static async Task<uint> RequestWorld()
        {
            // TODO: change this
            if (IsClient())
            {
                var worldIds = await Client.GetAvailableWorldId.Call();
                if (worldIds.Length == 0)
                {
                    throw new Exception("The server didn't response with any valid worlds.");
                }

                var worldInfo = await Client.GetWorldInfo.Call(worldIds[0]);

                Singleton<ChunkService>.Instance.Worlds.Add(worldInfo["name"]);
            }

            // It's a simple wait-until-we-have-a-world procedure now.
            // But it should be changed into get player information
            // and get the world id from it.
            while (Singleton<ChunkService>.Instance.Worlds.Get(0) == null)
                Thread.Yield();
            return 0;
        }

        private static bool IsClient()
        {
            return true;
        }

        public override void Update()
        {
            Singleton<ChunkService>.Instance.TaskDispatcher.ProcessRenderTasks();
        }
        
        // Local server
        private Server server;

        // Player
        private Player player;
        
        // Current world
        private World currentWorld;

        private Renderer.RdWorld rdWorld;
    }
}
