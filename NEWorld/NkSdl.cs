/*
 * Nuklear - 1.32.0 - public domain
 * no warrenty implied; use at your own risk.
 * authored from 2015-2016 by Micha Mettke
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core;
using NuklearSharp;
using OpenGL;
using SDL2;
using static NuklearSharp.Nuklear;

namespace NEWorld
{
    public class NkSdl : BaseContext, IDisposable
    {
        public unsafe NkSdl(IntPtr win)
        {
            _win = win;
            Ctx.clip.copy = ClipBoardCopy;
            Ctx.clip.paste = ClipboardPaste;
            Ctx.clip.userdata = nk_handle_ptr(null);
            _prog = new Program();
            using (Shader vertex = new Shader(Gl.VertexShader, @"
#version 450 core
layout (std140, binding = 0) uniform { mat4 ProjMtx; }
layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 TexCoord;
layout (location = 2) in vec4 Color;
out vec2 Frag_UV;
out vec4 Frag_Color;
void main() {
   Frag_UV = TexCoord;
   Frag_Color = Color;
   gl_Position = ProjMtx * vec4(Position.xy, 0, 1);
}"),
                fragment = new Shader(Gl.FragmentShader, @"
#version 450 core
precision mediump float;
layout (binding = 1) uniform sampler2D Texture;
in vec2 Frag_UV;
in vec4 Frag_Color;
out vec4 Out_Color
void main(){
   Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
}"))
                _prog.Link(new[] {vertex, fragment});
            /* buffer setup */
            _ubo = new DataBuffer(16 * sizeof(float));
            _vao = new VertexArray();

            _vao.EnableAttrib(0);
            _vao.EnableAttrib(1);
            _vao.EnableAttrib(2);
            _vao.AttribFormat(0, 2, Gl.Float, false, 0);
            _vao.AttribFormat(1, 2, Gl.Float, false, 2 * sizeof(float));
            _vao.AttribFormat(2, 4, Gl.UnsignedByte, true, 4 * sizeof(float));
            _vao.AttribBinding(0, 0);
            _vao.AttribBinding(1, 0);
            _vao.AttribBinding(2, 0);
            ConvertConfig.vertex_layout = VertexLayout;
            ConvertConfig.vertex_size = 4 * sizeof(float) + 4;
            _textures = new List<Texture>();
        }

        private static readonly nk_draw_vertex_layout_element[] VertexLayout =
        {
            new nk_draw_vertex_layout_element
            {
                attribute = NK_VERTEX_POSITION,
                format = NK_FORMAT_FLOAT,
                offset = 0
            },
            new nk_draw_vertex_layout_element
            {
                attribute = NK_VERTEX_COLOR,
                format = NK_FORMAT_R8G8B8A8,
                offset = 4 * sizeof(float)
            },
            new nk_draw_vertex_layout_element
            {
                attribute = NK_VERTEX_TEXCOORD,
                format = NK_FORMAT_FLOAT,
                offset = 2 * sizeof(float)
            },
            new nk_draw_vertex_layout_element
            {
                attribute = NK_VERTEX_ATTRIBUTE_COUNT
            }
        };


        private static unsafe void ClipBoardCopy(nk_handle handle, char* c, int length) =>
            Gl.Utf8ToManaged((IntPtr) c);

        private static unsafe void ClipboardPaste(nk_handle usr, nk_text_edit edit)
        {
            var text = Gl.Utf8ToNative(SDL.SDL_GetClipboardText());
            var ptr = Marshal.AllocHGlobal(text.Length * sizeof(byte));
            Marshal.Copy(text, 0, ptr, text.Length);
            nk_textedit_paste(edit, (char*) ptr, nk_strlen((char*) ptr));
            Marshal.FreeHGlobal(ptr);
        }

        public unsafe void HandleEvent(ref SDL.SDL_Event evt)
        {
            switch (evt.type)
            {
                case SDL.SDL_EventType.SDL_KEYUP:
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    HandleKeyDownEvent(evt);
                    return;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                {
                    /* mouse button */
                    var down = evt.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN;
                    int x = evt.button.x, y = evt.button.y;
                    if (evt.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        if (evt.button.clicks > 1)
                            InputButton(NK_BUTTON_DOUBLE, x, y, down);
                        InputButton(NK_BUTTON_LEFT, x, y, down);
                    }
                    else if (evt.button.button == SDL.SDL_BUTTON_MIDDLE)
                        InputButton(NK_BUTTON_MIDDLE, x, y, down);
                    else if (evt.button.button == SDL.SDL_BUTTON_RIGHT)
                        InputButton(NK_BUTTON_RIGHT, x, y, down);

                    return;
                }
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    /* mouse motion */
                    if (Ctx.input.mouse.grabbed != 0)
                    {
                        int x = (int) Ctx.input.mouse.prev.x, y = (int) Ctx.input.mouse.prev.y;
                        InputMotion(x + evt.motion.xrel, y + evt.motion.yrel);
                    }
                    else InputMotion(evt.motion.x, evt.motion.y);

                    return;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    fixed (byte* ptr = evt.text.text)
                        InputGlyph(Gl.Utf8ToManaged((IntPtr) ptr));
                    return;
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    /* mouse wheel */
                    nk_vec2 scroll;
                    scroll.x = evt.wheel.x;
                    scroll.y = evt.wheel.y;
                    InputScroll(scroll);
                    return;
            }
        }

        private unsafe void HandleKeyDownEvent(SDL.SDL_Event evt)
        {
            /* key events */
            bool down = evt.type == SDL.SDL_EventType.SDL_KEYDOWN;
            var state = (byte*) SDL.SDL_GetKeyboardState(out var dummy);
            if (state == null) return;
            switch (evt.key.keysym.sym)
            {
                case SDL.SDL_Keycode.SDLK_RSHIFT:
                case SDL.SDL_Keycode.SDLK_LSHIFT:
                    InputKey(NK_KEY_SHIFT, down);
                    break;
                case SDL.SDL_Keycode.SDLK_DELETE:
                    InputKey(NK_KEY_DEL, down);
                    break;
                case SDL.SDL_Keycode.SDLK_RETURN:
                    InputKey(NK_KEY_ENTER, down);
                    break;
                case SDL.SDL_Keycode.SDLK_TAB:
                    InputKey(NK_KEY_TAB, down);
                    break;
                case SDL.SDL_Keycode.SDLK_BACKSPACE:
                    InputKey(NK_KEY_BACKSPACE, down);
                    break;
                case SDL.SDL_Keycode.SDLK_HOME:
                    InputKey(NK_KEY_TEXT_START, down);
                    InputKey(NK_KEY_SCROLL_START, down);
                    break;
                case SDL.SDL_Keycode.SDLK_END:
                    InputKey(NK_KEY_TEXT_END, down);
                    InputKey(NK_KEY_SCROLL_END, down);
                    break;
                case SDL.SDL_Keycode.SDLK_PAGEDOWN:
                    InputKey(NK_KEY_SCROLL_DOWN, down);
                    break;
                case SDL.SDL_Keycode.SDLK_PAGEUP:
                    InputKey(NK_KEY_SCROLL_UP, down);
                    break;
                case SDL.SDL_Keycode.SDLK_z:
                    InputKey(NK_KEY_TEXT_UNDO, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_r:
                    InputKey(NK_KEY_TEXT_REDO, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_c:
                    InputKey(NK_KEY_COPY, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_v:
                    InputKey(NK_KEY_PASTE, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_x:
                    InputKey(NK_KEY_CUT, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_b:
                    InputKey(NK_KEY_TEXT_LINE_START, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_e:
                    InputKey(NK_KEY_TEXT_LINE_END, down & state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0);
                    break;
                case SDL.SDL_Keycode.SDLK_UP:
                    InputKey(NK_KEY_UP, down);
                    break;
                case SDL.SDL_Keycode.SDLK_DOWN:
                    InputKey(NK_KEY_DOWN, down);
                    break;
                case SDL.SDL_Keycode.SDLK_LEFT when state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0:
                    InputKey(NK_KEY_TEXT_WORD_LEFT, down);
                    break;
                case SDL.SDL_Keycode.SDLK_LEFT:
                    InputKey(NK_KEY_LEFT, down);
                    break;
                case SDL.SDL_Keycode.SDLK_RIGHT when state[(int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL] != 0:
                    InputKey(NK_KEY_TEXT_WORD_RIGHT, down);
                    break;
                case SDL.SDL_Keycode.SDLK_RIGHT:
                    InputKey(NK_KEY_RIGHT, down);
                    break;
                default:
                    return;
            }
        }

        public override int CreateTexture(int width, int height, byte[] data)
        {
            var id = _textures.Count;
            var newTex = new Texture(1, PixelInternalFormats.Rgba8, new Vec2<int>(width, height));
            newTex.Image(0, new Rect<int>(0, 0, width, height), PixelTypes.Rgba, PixelDataFormats.UnsignedByte, data);
            _textures.Add(newTex);
            return id;
        }

        protected override void BeginDraw()
        {
            SDL.SDL_GetWindowSize(_win, out var width, out _height);
            SDL.SDL_GL_GetDrawableSize(_win, out var displayWidth, out var displayHeight);
            var othro = Mat4F.Ortho(0, width, 0, _height, 0, 1000);

            _scale.x = displayWidth / (float) width;
            _scale.y = displayHeight / (float) _height;

            /* setup global state */
            Gl.Viewport(0, 0, displayWidth, displayHeight);
            Gl.Enable(Gl.Blend);
            Gl.BlendEquation(Gl.FuncAdd);
            Gl.BlendFunc(Gl.SrcAlpha, Gl.OneMinusSrcAlpha);
            Gl.Disable(Gl.CullFace);
            Gl.Disable(Gl.DepthTest);
            Gl.Enable(Gl.ScissorTest);

            /* setup program */
            _prog.Use();
            _prog.Uniform(1, 0);
            _ubo.DataSection(0, othro.Data);
        }

        protected override void SetBuffers(byte[] vertices, ushort[] indices, int indicesCount, int vertexCount)
        {
            _verts = new DataBuffer();
            _element = new DataBuffer();
            _vao.Use();
            _verts.AllocateWith(vertices);
            _element.AllocateWith(indices);
            _vao.BindBuffer(0, _verts, 0, vertices.Length / vertexCount);
            _ubo.BindBase(Gl.UniformBuffer, 0);
            _element.Bind(Gl.ElementArrayBuffer);
        }

        protected override void Draw(int x, int y, int w, int h, int textureId, int startIndex, int primitiveCount)
        {
            _textures[textureId].Use(0);
            Gl.Scissor((int) (x * _scale.x), (int) ((_height - (y + h)) * _scale.y),
                (int) (w * _scale.x), (int) (h * _scale.y));
            Gl.DrawElements(Gl.Triangles, primitiveCount, Gl.UnsignedShort, (IntPtr) startIndex);
        }

        protected override void EndDraw()
        {
            _verts.Dispose();
            _element.Dispose();
            Gl.Disable(Gl.Blend);
            Gl.Disable(Gl.ScissorTest);
        }

        private readonly DataBuffer _ubo;
        private readonly VertexArray _vao;
        private readonly Program _prog;
        private readonly IntPtr _win;
        private DataBuffer _verts, _element;
        private nk_vec2 _scale;
        private int _height;
        private readonly List<Texture> _textures;

        public void Dispose()
        {
            _ubo?.Dispose();
            _vao?.Dispose();
            _prog?.Dispose();
            _verts?.Dispose();
            _element?.Dispose();
            if (_textures!=null)
                foreach (var texture in _textures)
                    texture.Dispose();
        }
    }
}