// 
// NEWorld: Widget.cs
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

using NuklearSharp;

namespace NEWorld
{
    // widget base class
    public abstract class Widget
    {
        public Widget(string name, Nuklear.nk_rect size, uint flags)
        {
            Name = name;
            _size = size;
            _flags = flags;
            Open = true;
        }

        public void _render(NkSdl ctx)
        {
            if (ctx.Begin(Name, _size, _flags))
            {
                Render(ctx);
                ctx.End();
            }
        }

        public abstract void Update();

        public bool Open { get; set; }

        public string Name { get; }

        protected abstract void Render(NkSdl ctx);

        private readonly uint _flags;
        private readonly Nuklear.nk_rect _size;
    }

    // callback style widget
    public class WidgetCallback : Widget
    {
        public delegate void RenderCallback(NkSdl ctx);

        public delegate void UpdateCallback();

        public WidgetCallback(string name, Nuklear.nk_rect size, uint flags,
            RenderCallback renderFunc, UpdateCallback updateFunc = null) : base(name, size, flags)
        {
            _renderFunc = renderFunc;
            _updateFunc = updateFunc;
        }

        protected override void Render(NkSdl ctx) => _renderFunc(ctx);

        public override void Update() => _updateFunc?.Invoke();

        private readonly RenderCallback _renderFunc;
        private readonly UpdateCallback _updateFunc;
    }
}