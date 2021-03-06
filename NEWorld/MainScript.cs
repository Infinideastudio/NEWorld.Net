﻿// 
// NEWorld/NEWorld: MainScript.cs
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
using System.Threading.Tasks;
using Akarin;
using Game;
using Game.Network;
using Game.Terrain;
using Game.World;
using NEWorld.Effects;
using NEWorld.Renderer;
using Xenko.Core.Diagnostics;
using Xenko.Core.Mathematics;
using Xenko.Core.Serialization.Contents;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Graphics;
using Xenko.Rendering;
using Buffer = Xenko.Graphics.Buffer;
using LogPort = Core.LogPort;

namespace NEWorld
{
    public static class Context
    {
        private static IGame _game;

        public static readonly VertexDeclaration VertexLayout = new VertexDeclaration(
            VertexElement.Color(PixelFormat.R32G32_UInt)
        );

        public static IGame Game
        {
            get => _game;
            set
            {
                _game = value;
                IndexBuffer = new IndexBufferBinding(IndexBufferBuilder.Build(), true, 262144 / 2 * 3);
            }
        }

        public static Material Material { get; set; }

        public static Material MaterialTransparent { get; set; }

        public static Scene OperatingScene { get; set; }

        public static GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

        public static GraphicsContext GraphicsContext => Game.GraphicsContext;

        public static IndexBufferBinding IndexBuffer { get; private set; }

        public static ContentManager Content { get; set; }

        public static RenderDrawContext RdwContext { get; set; }
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
                idx[cnt++] = b + 2;
                idx[cnt++] = b + 1;
                idx[cnt++] = b;
                idx[cnt++] = b + 3;
                idx[cnt++] = b + 2;
                idx[cnt++] = b;
            }

            return Buffer.Index.New(Context.GraphicsDevice, idx);
        }
    }

    public class MainScript : SyncScript
    {
        // Current world
        private World currentWorld;
        public Material Material;

        public Material MaterialTransparent;

        // Player
        private Player player;

        private RdWorld rdWorld;

        // Local server
        private Server server;

        private void InitializeModules()
        {
            Modules.Load("Main");
        }

        private void InitializeContext()
        {
            Context.Game = Game;
            Context.Content = Content;
            Context.Material = Material;
            Context.MaterialTransparent = MaterialTransparent;
            Context.OperatingScene = Entity.Scene;
            LogPort.Logger = Log;
            Context.RdwContext =
                new RenderDrawContext(Services, RenderContext.GetShared(Services), Game.GraphicsContext);
            Log.ActivateLog(LogMessageType.Debug);
            EventBus.Broadcast(this, new GameRenderPrepareEvent());
        }

        private void EstablishChunkService()
        {
            if (IsClient())
            {
                ChunkService.IsAuthority = false;
            }
            else
            {
                // Initialize server
                server = Akarin.Services.Get<Server>("Game.Server");
                server.Enable(31111);
                server.Run();
            }
        }

        private async Task EstablishGameConnection()
        {
            await Akarin.Services.Get<Client>("Game.Client").Enable("127.0.0.1", 31111);
            await Client.GetStaticChunkIds.Call();
            EventBus.Broadcast(this, new GameLoadEvent());
        }

        private void LoadPlayer()
        {
            player = new Player(0)
            {
                Position = new Double3(-16.0, 48.0, 32.0), Rotation = new Double3(-45.0, -22.5, 0.0)
            };
        }

        private async Task EnterCurrentWorld()
        {
            currentWorld = ChunkService.Worlds.Get(await RequestWorld());
            currentWorld.RegisterChunkTasks(player);
            ChunkService.EnableDispatcher();
        }

        private void StartTerrainRenderService()
        {
            rdWorld = new RdWorld(currentWorld, player, 4);
        }

        private async void Initialize()
        {
            InitializeContext();
            InitializeModules();
            LoadTextures();
            EstablishChunkService();
            await EstablishGameConnection();
            LoadPlayer();
            await EnterCurrentWorld();
            StartTerrainRenderService();
        }

        private void LoadTextures()
        {
            var texture = RdTextures.FlushTextures();
            BlockRenderers.FlushTextures(Akarin.Services.Get<IBlockTextures>("BlockTextures"));
            Material.Passes[0].Parameters.Set(VertexTextureTerrainKeys.Almg, texture);
            Material.Passes[0].Parameters.Set(VertexTextureTerrainKeys.TexturePerLine, RdTextures.TexturesPerLine);
            MaterialTransparent.Passes[0].Parameters.Set(VertexTextureTerrainKeys.Almg, texture);
            MaterialTransparent.Passes[0].Parameters
                .Set(VertexTextureTerrainKeys.TexturePerLine, RdTextures.TexturesPerLine);
        }

        public override void Start()
        {
            Initialize();
        }

        public override void Cancel()
        {
            TearDown();
        }

        private void TearDown()
        {
            Akarin.Services.Get<TaskDispatcher>("Game.TaskDispatcher").Reset();
            EventBus.Broadcast(this, new GameUnloadEvent());
            EventBus.Broadcast(this, new GameRenderFinalizeEvent());
        }

        private static async Task<uint> RequestWorld()
        {
            // TODO: change this
            if (IsClient())
            {
                var worldIds = await Client.GetAvailableWorldId.Call();
                if (worldIds.Length == 0) throw new Exception("The server didn't response with any valid worlds.");

                var worldInfo = await Client.GetWorldInfo.Call(worldIds[0]);

                ChunkService.Worlds.Add(worldInfo["name"]);
            }

            // It's a simple wait-until-we-have-a-world procedure now.
            // But it should be changed into get player information
            // and get the world id from it.
            while (ChunkService.Worlds.Get(0) == null)
                Thread.Yield();
            return 0;
        }

        private static bool IsClient()
        {
            return true;
        }

        public override void Update()
        {
            ChunkService.TaskDispatcher.ProcessRenderTasks();
        }
    }
}