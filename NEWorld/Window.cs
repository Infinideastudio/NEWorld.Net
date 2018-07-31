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
using SDL2;
using OpenGL;
using static NuklearSharp.Nuklear;

namespace NEWorld
{
    public struct MouseState
    {
        public int x, y;
        public bool left, mid, right, relative;
    }

    public class Window
    {
        private Window(string title, int width, int height)
        {
            _title = title;
            _width = width;
            _height = height; 
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 3);

            _window = SDL.SDL_CreateWindow(_title, 100, 100, _width, _height, 
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            Gl.Init(SDL.SDL_GL_GetProcAddress);
            _context = SDL.SDL_GL_CreateContext(_window);
            SDL.SDL_GL_SetSwapInterval(0); // VSync
            makeCurrentDraw();
            //Renderer::init();
            _nuklearContext = new NkSdl(_window);
        }
        
        ~Window()
        {
            SDL.SDL_DestroyWindow(_window);
            SDL.SDL_GL_DeleteContext(_context);
            SDL.SDL_Quit();
        }

        public void makeCurrentDraw() => SDL.SDL_GL_MakeCurrent(_window, _context);

        public void swapBuffers() => SDL.SDL_GL_SwapWindow(_window);

        public static unsafe byte* getKeyBoardState() => (byte*) SDL.SDL_GetKeyboardState(out var number);

        public int getWidth() => _width;

        public int getHeight() => _height;

        public void pollEvents()
        {
            if (SDL.SDL_GetRelativeMouseMode() == SDL.SDL_bool.SDL_TRUE)
            {
                var buttons = SDL.SDL_GetRelativeMouseState(out _mouse.x, out _mouse.y);
                _mouse.left = (buttons & SDL.SDL_BUTTON_LEFT) != 0;
                _mouse.right = (buttons & SDL.SDL_BUTTON_RIGHT) != 0;
                _mouse.mid = (buttons & SDL.SDL_BUTTON_MIDDLE) != 0;
                _mouse.relative = true;
            }
            else
            {
                _prevMouse = _mouse;
                var buttons = SDL.SDL_GetMouseState(out _mouse.x, out _mouse.y);
                _mouse.left = (buttons & SDL.SDL_BUTTON_LEFT) != 0;
                _mouse.right = (buttons & SDL.SDL_BUTTON_RIGHT) != 0;
                _mouse.mid = (buttons & SDL.SDL_BUTTON_MIDDLE) != 0;
                if (_mouse.relative) _prevMouse = _mouse;
                _mouse.relative = false;
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

        public bool shouldQuit() => _shouldQuit;

        public NkSdl getNkContext() => _nuklearContext;

        /**
         * \brief Get the relative motion of mouse
         * \return The relative motion of mouse
         */
        public MouseState getMouseMotion()   {
            var res = _mouse;
            res.x -= _prevMouse.x;
            res.y -= _prevMouse.y;
            return res;
        }

        public static void lockCursor() => SDL.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_TRUE);

        public static void unlockCursor() => SDL.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_FALSE);

        private static Window _win;
        
        public static Window getInstance(string title = "", int width = 0, int height = 0) => 
            _win ?? (_win = new Window(title, width, height));

        private string _title;
        private int _width, _height;
        private MouseState _mouse, _prevMouse;
        private bool _shouldQuit;
        private readonly IntPtr _window, _context;
        private readonly NkSdl _nuklearContext;
    };
}
