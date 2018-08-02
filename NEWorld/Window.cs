// 
// GUI: window.h
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
using OpenGL;
using SDL2;

namespace NEWorld
{
    public struct MouseState
    {
        public int X, Y;
        public bool Left, Mid, Right, Relative;
    }

    public class Window
    {
        private Window(string title, int width, int height)
        {
            _title = title;
            _width = width;
            _height = height; 
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
                throw new Exception("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 4);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 5);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK,
                (int) SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);

            _window = SDL.SDL_CreateWindow(_title, 100, 100, _width, _height, 
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            _context = SDL.SDL_GL_CreateContext(_window);
            SDL.SDL_GL_SetSwapInterval(0); // VSync
            MakeCurrentDraw();
            Gl.Init(SDL.SDL_GL_GetProcAddress);
            _nuklearContext = new NkSdl(_window);
        }
        
        ~Window()
        {
            SDL.SDL_DestroyWindow(_window);
            SDL.SDL_GL_DeleteContext(_context);
            SDL.SDL_Quit();
        }

        public void MakeCurrentDraw() => SDL.SDL_GL_MakeCurrent(_window, _context);

        public void SwapBuffers() => SDL.SDL_GL_SwapWindow(_window);

        public static unsafe byte* GetKeyBoardState() => (byte*) SDL.SDL_GetKeyboardState(out var number);

        public int GetWidth() => _width;

        public int GetHeight() => _height;

        public void PollEvents()
        {
            if (SDL.SDL_GetRelativeMouseMode() == SDL.SDL_bool.SDL_TRUE)
            {
                var buttons = SDL.SDL_GetRelativeMouseState(out _mouse.X, out _mouse.Y);
                _mouse.Left = (buttons & SDL.SDL_BUTTON_LEFT) != 0;
                _mouse.Right = (buttons & SDL.SDL_BUTTON_RIGHT) != 0;
                _mouse.Mid = (buttons & SDL.SDL_BUTTON_MIDDLE) != 0;
                _mouse.Relative = true;
            }
            else
            {
                _prevMouse = _mouse;
                var buttons = SDL.SDL_GetMouseState(out _mouse.X, out _mouse.Y);
                _mouse.Left = (buttons & SDL.SDL_BUTTON_LEFT) != 0;
                _mouse.Right = (buttons & SDL.SDL_BUTTON_RIGHT) != 0;
                _mouse.Mid = (buttons & SDL.SDL_BUTTON_MIDDLE) != 0;
                if (_mouse.Relative) _prevMouse = _mouse;
                _mouse.Relative = false;
            }

            _nuklearContext.InputBegin();
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                _nuklearContext.HandleEvent(ref e);
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        _shouldQuit = true;
                        break;
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        switch (e.window.windowEvent)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                                _width = e.window.data1;
                                _height = e.window.data2;
                                break;
                        }

                        break;
                }
            }

            _nuklearContext.InputEnd();
        }

        public bool ShouldQuit() => _shouldQuit;

        public NkSdl GetNkContext() => _nuklearContext;

        /**
         * \brief Get the relative motion of mouse
         * \return The relative motion of mouse
         */
        public MouseState GetMouseMotion()   {
            var res = _mouse;
            res.X -= _prevMouse.X;
            res.Y -= _prevMouse.Y;
            return res;
        }

        public static void LockCursor() => SDL.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_TRUE);

        public static void UnlockCursor() => SDL.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_FALSE);

        private static Window _win;
        
        public static Window GetInstance(string title = "", int width = 0, int height = 0) => 
            _win ?? (_win = new Window(title, width, height));

        private readonly string _title;
        private int _width, _height;
        private MouseState _mouse, _prevMouse;
        private bool _shouldQuit;
        private readonly IntPtr _window, _context;
        private readonly NkSdl _nuklearContext;
    }
}
