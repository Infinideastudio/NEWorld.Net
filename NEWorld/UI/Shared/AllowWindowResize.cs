// 
// NEWorld/NEWorld: AllowWindowResize.cs
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
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Images;
using Xenko.Rendering.Compositing;

namespace NEWorld.UI.Shared
{
    public class AllowWindowResize : SyncScript
    {
        private class BackgroundRenderer : SceneRendererBase
        {
            private readonly GaussianBlur effectBlur = new GaussianBlur {Radius = 10};
            private Texture renderTexture;

            public Texture Chain(Texture value)
            {
                renderTexture?.Dispose();
                renderTexture = Texture.New2D(value.GraphicsDevice, value.Width, value.Height,
                    value.Format, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
                effectBlur.SetInput(value);
                effectBlur.SetOutput(renderTexture);
                return renderTexture;
            }

            protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
            {
                using (drawContext.PushRenderTargetsAndRestore())
                {
                    effectBlur.Draw(drawContext);
                }
            }
        }

        private RenderTextureSceneRenderer render;
        private BackgroundRenderer blit;
        
        public override void Start()
        {
            var compositor = ((Xenko.Engine.Game)Game).SceneSystem.GraphicsCompositor;
            var collection = ((SceneRendererCollection) compositor.Game).Children;
            render = (RenderTextureSceneRenderer)collection[0];
            collection[1] = blit = new BackgroundRenderer();
            DoBackgroundBufferResize();
            Game.Window.AllowUserResizing = true;
            Game.Window.ClientSizeChanged += BackgroundBufferResize;
        }

        public override void Update()
        {
        }
        
        public override void Cancel()
        {
            Game.Window.ClientSizeChanged -= BackgroundBufferResize;
            base.Cancel();
        }

        private void BackgroundBufferResize(object obj, EventArgs e)
        {
            DoBackgroundBufferResize();
        }

        private void DoBackgroundBufferResize()
        {
            var backBuffer = GraphicsDevice.Presenter.BackBuffer;
            var oldStage = render.RenderTexture;
            var stage = Texture.New2D(
                GraphicsDevice, backBuffer.Width, backBuffer.Height,
                PixelFormat.B8G8R8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource
            );
            render.RenderTexture = stage;
            Entity.Get<BackgroundComponent>().Texture = blit.Chain(stage);
            oldStage?.Dispose();
        }
    }
}