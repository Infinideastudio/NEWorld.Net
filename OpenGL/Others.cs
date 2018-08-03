// 
// OpenGL: Others.cs
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

namespace OpenGL
{
    static partial class Gl
    {
        // Draw
        public delegate void DrawArraysProc(uint mode, int first, int count);

        public delegate void DrawElementsProc(uint mode, int count, uint type, IntPtr indicies);

        public static DrawArraysProc DrawArrays;

        public static DrawElementsProc DrawElements;

        // Context Op
        public delegate void ViewportProc(int x, int y, int width, int height);

        public delegate void ClearColorProc(float red, float green, float blue, float alpha);

        public delegate void ClearDepthProc(float depth);

        public delegate void ClearProc(uint mask);

        public delegate void LineWidthProc(float width);

        public delegate void EnableProc(uint cap);

        public delegate void DisableProc(uint cap);

        public delegate void DepthFuncProc(uint cap);

        public delegate void CullFaceProc(uint cap);

        public delegate void BlendEquationProc(uint mode);

        public delegate void BlendFuncProc(uint sfactor, uint dfactor);

        public delegate void ScissorProc(int x, int y, int width, int height);

        public static ViewportProc Viewport;
        public static ClearColorProc ClearColor;
        public static ClearProc Clear;
        public static ClearDepthProc ClearDepth;
        public static LineWidthProc LineWidth;
        public static EnableProc Enable;
        public static DisableProc Disable;
        public static DepthFuncProc DepthFunc;
        public static CullFaceProc CullFaceOption;
        public static BlendEquationProc BlendEquation;
        public static BlendFuncProc BlendFunc;
        public static ScissorProc Scissor;

        static partial void InitOthers()
        {
            // Draw
            DrawArrays = Get<DrawArraysProc>("glDrawArrays");
            // Context
            Viewport = Get<ViewportProc>("glViewport");
            ClearColor = Get<ClearColorProc>("glClearColor");
            ClearDepth = Get<ClearDepthProc>("glClearDepth");
            Clear = Get<ClearProc>("glClear");
            LineWidth = Get<LineWidthProc>("glLineWidth");
            Enable = Get<EnableProc>("glEnable");
            Disable = Get<DisableProc>("glDisable");
            BlendEquation = Get<BlendEquationProc>("glBlendEquation");
            BlendFunc = Get<BlendFuncProc>("glBlendFunc");
            Scissor = Get<ScissorProc>("glScissor");
            DrawElements = Get<DrawElementsProc>("glDrawElements");
            DepthFunc = Get<DepthFuncProc>("glDepthFunc");
            CullFaceOption = Get<CullFaceProc>("glCullFace");
        }
    }
}