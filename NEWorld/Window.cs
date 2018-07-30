/*
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
        int x, y;
        bool left, mid, right, relative;
    };

    public class Window
    {
        public void makeCurrentDraw() {
            SDL.SDL_GL_MakeCurrent(mWindow, mContext);
        }

        public void swapBuffers() {
            SDL.SDL_GL_SwapWindow(mWindow);
        }

        public static Uint8* getKeyBoardState() {
            return SDL.SDL_GetKeyboardState(null);
        }

        public int getWidth()   {
            return mWidth;
        }

        public int getHeight()   {
            return mHeight;
        }

        public void pollEvents()
        {
            if (SDL.SDL_GetRelativeMouseMode() == SDL.SDL_bool.SDL_TRUE)
            {
                UInt32 buttons = SDL_GetRelativeMouseState(&mMouse.x, &mMouse.y);
                mMouse.left = buttons & SDL_BUTTON_LEFT;
                mMouse.right = buttons & SDL_BUTTON_RIGHT;
                mMouse.mid = buttons & SDL_BUTTON_MIDDLE;
                mMouse.relative = true;
            }
            else
            {
                mPrevMouse = mMouse;
                UInt32 buttons = SDL_GetMouseState(&mMouse.x, &mMouse.y);
                mMouse.left = buttons & SDL_BUTTON_LEFT;
                mMouse.right = buttons & SDL_BUTTON_RIGHT;
                mMouse.mid = buttons & SDL_BUTTON_MIDDLE;
                if (mMouse.relative) mPrevMouse = mMouse;
                mMouse.relative = false;
            }

            nk_input_begin(mNuklearContext);
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                nk_sdl_handle_event(&e);
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        mShouldQuit = true;
                        break;
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        switch (e.window.windowEvent)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                                mWidth = e.window.data1;
                                mHeight = e.window.data2;
                                break;
                        }

                        break;
                }
            }

            nk_input_end(mNuklearContext);
        }

        public static Window& getInstance(string title = "", int width = 0, int height = 0) {
            static Window win(title, width, height);
            return win;
        }

        public bool shouldQuit()   {
            return mShouldQuit;
        }

        public nk_context* getNkContext()   {
            return mNuklearContext;
        }

        /**
         * \brief Get the relative motion of mouse
         * \return The relative motion of mouse
         #1#
        public MouseState getMouseMotion()   {
            MouseState res = mMouse;
            res.x -= mPrevMouse.x;
            res.y -= mPrevMouse.y;
            return res;
        }

        public static void lockCursor()
        {
            SDL.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_TRUE);
        }

        public static void unlockCursor()
        {
            SDL.SDL_SetRelativeMouseMode(SDL.SDL_bool.SDL_FALSE);
        }

        private IntPtr mWindow;
        private string mTitle;
        private int mWidth, mHeight;
        private MouseState mMouse, mPrevMouse;
        private bool mShouldQuit = false;

        private Window(string title, int width, int height)
        {
            mTitle = title;
            mWidth = width;
            mHeight = height; 
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
            //SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 3);

            mWindow = SDL.SDL_CreateWindow(mTitle, 100, 100, mWidth, mHeight, SDL.SDL_WINDOW_OPENGL | SDL_WINDOW_RESIZABLE);
            Gl.Init(SDL.SDL_GL_GetProcAddress);
            mContext = SDL.SDL_GL_CreateContext(mWindow);
            SDL.SDL_GL_SetSwapInterval(0); // VSync
            makeCurrentDraw();
            Renderer::init();
            mNuklearContext = nk_sdl_init(mWindow);
        }
        
        ~Window()
        {
            nk_sdl_shutdown();
            SDL.SDL_DestroyWindow(mWindow);
            SDL.SDL_GL_DeleteContext(mContext);
            SDL.SDL_Quit();
        }

        private IntPtr mContext;
        private nk_context mNuklearContext;
    };
}
*/
