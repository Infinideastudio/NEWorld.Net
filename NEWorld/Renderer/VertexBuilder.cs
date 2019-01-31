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
        private readonly IntPtr data;
        private int count;

        private int size;

        public VertexBuilder(int size)
        {
            data = Marshal.AllocHGlobal(size * sizeof(float));
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void AddPrimitive(int verts, params float[] data)
        {
            count += verts;
            Marshal.Copy(data, 0, this.data + size * sizeof(float), data.Length);
            size += data.Length;
        }

        ~VertexBuilder()
        {
            ReleaseUnmanagedResources();
        }

        public void Rect(Int3 position, int face, Int2 tex, int rotation)
        {
            throw new NotImplementedException();
        }

        public Mesh Dump()
        {
            return count > 0
                ? new Mesh
                {
                    Draw = new MeshDraw
                    {
                        DrawCount = count / 2 * 3,
                        IndexBuffer = Context.IndexBuffer,
                        PrimitiveType = PrimitiveType.TriangleList,
                        StartLocation = 0,
                        VertexBuffers = new[]
                        {
                            new VertexBufferBinding(
                                Buffer.Vertex.New(Context.Game.GraphicsDevice,
                                    new DataPointer(data, size * sizeof(float))),
                                Context.VertexLayout, count)
                        }
                    },
                    MaterialIndex = 0
                }
                : null;
        }

        private void ReleaseUnmanagedResources()
        {
            Marshal.FreeHGlobal(data);
        }
    }
}