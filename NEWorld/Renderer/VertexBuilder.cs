using System;
using System.Runtime.InteropServices;
using Game.Terrain;
using Xenko.Graphics;
using Xenko.Rendering;
using Buffer = Xenko.Graphics.Buffer;

namespace NEWorld.Renderer
{
    public class VertexBuilder : IVertexBuilder
    {
        public VertexBuilder(int size) => data = Marshal.AllocHGlobal(size * sizeof(float));

        ~VertexBuilder() => Marshal.FreeHGlobal(data);

        public void AddPrimitive(int verts, params float[] data)
        {
            count += verts;
            Marshal.Copy(data, 0, this.data + size * sizeof(float), data.Length);
            size += data.Length;
        }

        public Mesh Dump()
        {
            return count > 0
                ? new Mesh
                {
                    Draw = new MeshDraw()
                    {
                        DrawCount = count / 2 * 3,
                        IndexBuffer = Context.IndexBuffer,
                        PrimitiveType = PrimitiveType.TriangleList,
                        StartLocation = 0,
                        VertexBuffers = new[]
                        {
                            new VertexBufferBinding(Buffer.Vertex.New(Context.Game.GraphicsDevice, new DataPointer(data, size * sizeof(float))),
                                Context.VertexLayout, count)
                        }
                    },
                    MaterialIndex = 0
                }
                : null;
        }

        private int size;
        private int count;
        private readonly IntPtr data;
    }

}