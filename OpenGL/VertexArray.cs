// 
// OpenGL: VertexArray.cs
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
using Core.Utilities;

namespace OpenGL
{
    public static partial class Gl
    {
        internal unsafe delegate void CreateVertexArraysProc(int n, uint* arrays);

        internal unsafe delegate void DeleteVertexArraysProc(int n, uint* arrays);

        internal delegate void BindVertexArrayProc(uint array);

        internal delegate void EnableVertexArrayAttribProc(uint vaobj, uint index);

        internal delegate void DisableVertexArrayAttribProc(uint vaobj, uint index);

        internal delegate void VertexArrayAttribFormatProc(uint vaobj, uint attribindex, int size, uint type,
            byte normalized, uint relativeoffset);

        internal delegate void VertexArrayAttribBindingProc(uint vaobj, uint attribindex, uint bindingindex);

        internal delegate void VertexArrayVertexBufferProc(uint vaobj, uint bindingindex, uint buffer,
            UIntPtr offset, int stride);

        internal static CreateVertexArraysProc CreateVertexArrays;
        internal static DeleteVertexArraysProc DeleteVertexArrays;
        internal static BindVertexArrayProc BindVertexArray;
        internal static EnableVertexArrayAttribProc EnableVertexArrayAttrib;
        internal static DisableVertexArrayAttribProc DisableVertexArrayAttrib;
        internal static VertexArrayAttribFormatProc VertexArrayAttribFormat;
        internal static VertexArrayAttribBindingProc VertexArrayAttribBinding;
        internal static VertexArrayVertexBufferProc VertexArrayVertexBuffer;

        static partial void InitVertexArray()
        {
            DeleteVertexArrays = Get<DeleteVertexArraysProc>("glDeleteVertexArrays");
            BindVertexArray = Get<BindVertexArrayProc>("glBindVertexArray");
            EnableVertexArrayAttrib = Get<EnableVertexArrayAttribProc>("glEnableVertexArrayAttrib");
            DisableVertexArrayAttrib = Get<DisableVertexArrayAttribProc>("glDisableVertexArrayAttrib");
            VertexArrayAttribFormat =
                Get<VertexArrayAttribFormatProc>("glVertexArrayAttribFormat");
            VertexArrayAttribBinding =
                Get<VertexArrayAttribBindingProc>("glVertexArrayAttribBinding");
            VertexArrayVertexBuffer = Get<VertexArrayVertexBufferProc>("glVertexArrayVertexBuffer");
            CreateVertexArrays = Get<CreateVertexArraysProc>("glCreateVertexArrays");
        }
    }

    public class VertexArray : StrictDispose
    {
        public unsafe VertexArray()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.CreateVertexArrays(1, addr);
            }
        }

        protected override unsafe void Release()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.DeleteVertexArrays(1, addr);
            }
        }

        public void Use() => Gl.BindVertexArray(_hdc);

        public void EnableAttrib(uint index) => Gl.EnableVertexArrayAttrib(_hdc, index);

        public void DisableAttrib(uint index) => Gl.DisableVertexArrayAttrib(_hdc, index);

        public void AttribFormat(uint index, int size, uint type, bool normalized, uint relativeOffset)
        {
            byte norm = 0;
            if (normalized) norm = 1;
            Gl.VertexArrayAttribFormat(_hdc, index, size, type, norm, relativeOffset);
        }

        public void AttribBinding(uint attribIndex, uint bufferIndex) =>
            Gl.VertexArrayAttribBinding(_hdc, attribIndex, bufferIndex);

        public void BindBuffer(uint index, DataBuffer buffer, uint offset, int stride) =>
            Gl.VertexArrayVertexBuffer(_hdc, index, buffer.Raw(), (UIntPtr) offset, stride);

        public void BindBuffer(uint index, ConstDataBuffer buffer, uint offset, int stride) =>
            Gl.VertexArrayVertexBuffer(_hdc, index, buffer.Raw(), (UIntPtr) offset, stride);

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}