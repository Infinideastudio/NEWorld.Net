// 
// GUI: widgetmanager.h
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

namespace NEWorld
{
    internal class WidgetManager : Dictionary<string, Widget>
    {

        public WidgetManager(NkSdl nkctx) => _mNkContext = nkctx;

        public void Render()
        {
            foreach (var widget in this)
            widget.Value._render(_mNkContext);
            _mNkContext.End();
            // TODO: add an option to adjust the arguments
            _mNkContext.Draw();
        }

        public void Update()
        {
            foreach (var widget in this)
            widget.Value.Update();
        }

        public void Add(Widget widget) => Add(widget.Name, widget);

        private readonly NkSdl _mNkContext;
    };
}