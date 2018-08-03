// 
// OpenGL: DataBuffer.cs
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
        public const uint StreamDraw = 0x88E0;
        public const uint StreamRead = 0x88E1;
        public const uint StreamCopy = 0x88E2;
        public const uint StaticDraw = 0x88E4;
        public const uint StaticRead = 0x88E5;
        public const uint StaticCopy = 0x88E6;
        public const uint DynamicDraw = 0x88E8;
        public const uint DynamicRead = 0x88E9;
        public const uint DynamicCopy = 0x88EA;
        public const uint UniformBuffer = 0x8A11;
        public const uint ElementArrayBuffer = 0x8893;

        internal unsafe delegate void CreateBuffersProc(int n, uint* buffers);

        internal unsafe delegate void DeleteBuffersProc(int n, uint* buffers);

        internal unsafe delegate void NamedBufferStorageProc(uint buffer, int size, void* data, uint flags);

        internal delegate void BindBufferBaseProc(uint target, uint index, uint buffer);

        internal delegate void BindBufferProc(uint target, uint buffer);

        internal unsafe delegate void NamedBufferSubDataProc(uint buffer, UIntPtr offset, UIntPtr size,
            void* data);

        internal static CreateBuffersProc CreateBuffers;
        internal static DeleteBuffersProc DeleteBuffers;
        internal static NamedBufferStorageProc NamedBufferStorage;
        internal static BindBufferBaseProc BindBufferBase;
        internal static BindBufferProc BindBuffer;
        internal static NamedBufferSubDataProc NamedBufferSubData;

        static partial void InitDataBuffer()
        {
            CreateBuffers = Get<CreateBuffersProc>("glCreateBuffers");
            DeleteBuffers = Get<DeleteBuffersProc>("glDeleteBuffers");
            NamedBufferStorage = Get<NamedBufferStorageProc>("glNamedBufferStorage");
            BindBufferBase = Get<BindBufferBaseProc>("glBindBufferBase");
            BindBuffer = Get<BindBufferProc>("glBindBuffer");
            NamedBufferSubData = Get<NamedBufferSubDataProc>("glNamedBufferSubData");
        }
    }

    public class DataBuffer : StrictDispose
    {
        public unsafe DataBuffer()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.CreateBuffers(1, addr);
            }
        }

        public DataBuffer(int size) : this() => Allocate(size);

        protected override unsafe void Release()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.DeleteBuffers(1, addr);
            }
        }

        public void AllocateWith(byte[] data) => AllocateWith(data, data.Length);

        public void AllocateWith(ushort[] data) => AllocateWith(data, data.Length);

        public void AllocateWith(float[] data) => AllocateWith(data, data.Length);

        public unsafe void AllocateWith(byte[] data, int length)
        {
            fixed (byte* ptr = &data[0])
            {
                AllocateRaw(length * sizeof(byte), ptr);
            }
        }

        public unsafe void AllocateWith(ushort[] data, int length)
        {
            fixed (ushort* ptr = &data[0])
            {
                AllocateRaw(length * sizeof(ushort), ptr);
            }
        }

        public unsafe void AllocateWith(float[] data, int length)
        {
            fixed (float* ptr = &data[0])
            {
                AllocateRaw(length * sizeof(float), ptr);
            }
        }

        public unsafe void DataSection(uint offset, float[] data)
        {
            fixed (float* ptr = &data[0])
            {
                DataSectionRaw(offset, data.Length * sizeof(float), ptr);
            }
        }

        public void Bind(uint usage) => Gl.BindBuffer(usage, _hdc);

        public void BindBase(uint usage, uint index) => Gl.BindBufferBase(usage, index, _hdc);

        public uint Raw() => _hdc;

        private unsafe void AllocateRaw(int size, void* data) =>
            Gl.NamedBufferStorage(_hdc, size, data, 0x0100);

        private unsafe void DataSectionRaw(uint offset, int size, void* data) =>
            Gl.NamedBufferSubData(_hdc, (UIntPtr) offset, (UIntPtr) size, data);

        private unsafe void Allocate(int size) => AllocateRaw(size, null);

        private uint _hdc;
    }

    public class ConstDataBuffer : StrictDispose
    {
        public unsafe ConstDataBuffer(int size, IntPtr data)
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.CreateBuffers(1, addr);
            }

            Gl.NamedBufferStorage(_hdc, size, (void*) data, 0);
        }

        protected override unsafe void Release()
        {
            fixed (uint* addr = &_hdc)
            {
                Gl.DeleteBuffers(1, addr);
            }
        }

        public void BindBase(uint usage, uint index) => Gl.BindBufferBase(usage, index, _hdc);

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}