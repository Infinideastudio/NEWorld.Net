using System;
using System.Runtime.InteropServices;
using Game.Terrain;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
using Buffer = Xenko.Graphics.Buffer;

namespace NEWorld.Renderer
{
    public class VertexBuilder : IVertexBuilder, IDisposable
    {
        private static readonly uint[,] Rotation =
        {
            {0x0000, 0x0001, 0x0101, 0x0100}, {0x0001, 0x0101, 0x0100, 0x0000},
            {0x0101, 0x0100, 0x0000, 0x0001}, {0x0100, 0x0000, 0x0001, 0x0101}
        };

        private static readonly uint[,] Faces =
        {
            {0x010101, 0x010001, 0x010000, 0x010100}, {0x000100, 0x000000, 0x000001, 0x000101},
            {0x000100, 0x000101, 0x010101, 0x010100}, {0x000001, 0x000000, 0x010000, 0x010001},
            {0x000101, 0x000001, 0x010001, 0x010101}, {0x010100, 0x010000, 0x000000, 0x000100}
        };

        private readonly IntPtr data;
        private int count;
        private unsafe uint* view;

        public unsafe VertexBuilder(int size)
        {
            data = Marshal.AllocHGlobal(size * 8);
            view = (uint*) data.ToPointer();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public unsafe void Rect(Int3 position, Int2 tex, uint face, int rotation, uint shade)
        {
            var high = (shade << 24) | (uint) ((position.X << 16) | (position.Y << 8) | position.Z);
            var low = (face << 16) | (uint) ((tex.X << 8) | tex.Y);
            for (var i = 0; i < 4; ++i)
            {
                *view++ = high + Faces[face, i];
                *view++ = low + Rotation[rotation, i];
            }

            count += 4;
        }

        ~VertexBuilder()
        {
            ReleaseUnmanagedResources();
        }

        public Mesh Dump()
        {
            return count > 0 ? CreateMesh() : null;
        }

        private Mesh CreateMesh()
        {
            return new Mesh
            {
                Draw = new MeshDraw
                {
                    DrawCount = count / 2 * 3,
                    IndexBuffer = Context.IndexBuffer,
                    PrimitiveType = PrimitiveType.TriangleList,
                    StartLocation = 0,
                    VertexBuffers = new[]
                    {
                        CreateVertexBuffer()
                    }
                },
                MaterialIndex = 0
            };
        }

        private VertexBufferBinding CreateVertexBuffer()
        {
            return new VertexBufferBinding(
                Buffer.Vertex.New(Context.Game.GraphicsDevice, new DataPointer(data, count * 32)), Context.VertexLayout,
                count);
        }

        private void ReleaseUnmanagedResources()
        {
            Marshal.FreeHGlobal(data);
        }
    }
}