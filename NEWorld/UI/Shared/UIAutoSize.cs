// 
// NEWorld/NEWorld: UIAutoSize.cs
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
using Xenko.Core.Mathematics;
using Xenko.Engine;

namespace NEWorld.UI.Shared
{
    public class UIAutoSize : SyncScript
    {
        public override void Start()
        {
            AdjustVirtualResolution(this, EventArgs.Empty);
            Game.Window.ClientSizeChanged += AdjustVirtualResolution;
        }

        public override void Cancel()
        {
            Game.Window.ClientSizeChanged -= AdjustVirtualResolution;
        }

        private void AdjustVirtualResolution(object sender, EventArgs e)
        {
            var backBufferSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width,
                GraphicsDevice.Presenter.BackBuffer.Height);
            Entity.Get<UIComponent>().Resolution = new Vector3(backBufferSize, 1000);
        }

        public override void Update()
        {
        }
    }
}