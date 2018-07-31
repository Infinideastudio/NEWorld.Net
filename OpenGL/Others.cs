// OpenGL: Others.cs
// Graphics.Net: General Application Framework API and GUI For .Net
// Copyright (C) 2015-2018 NEWorld Team
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

        public delegate void ClearProc(uint mask);

        public delegate void LineWidthProc(float width);

        public delegate void EnableProc(uint cap);

        public delegate void DisableProc(uint cap);

        public delegate void BlendEquationProc(uint mode);

        public delegate void BlendFuncProc(uint sfactor, uint dfactor);

        public delegate void ScissorProc(int x, int y, int width, int height);

        public static ViewportProc Viewport;
        public static ClearColorProc ClearColor;
        public static ClearProc Clear;
        public static LineWidthProc LineWidth;
        public static EnableProc Enable;
        public static DisableProc Disable;
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
            Clear = Get<ClearProc>("glClear");
            LineWidth = Get<LineWidthProc>("glLineWidth");
            Enable = Get<EnableProc>("glEnable");
            Disable = Get<DisableProc>("glDisable");
            BlendEquation = Get<BlendEquationProc>("glBlendEquation");
            BlendFunc = Get<BlendFuncProc>("glBlendFunc");
            Scissor = Get<ScissorProc>("glScissor");
            DrawElements = Get<DrawElementsProc>("glDrawElements");
        }
    }
}