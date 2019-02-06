using System;

namespace Game.World
{
    public partial class Chunk: IDisposable
    {
        public void Dispose()
        {
            ReleaseCriticalResources();
            GC.SuppressFinalize(this);
        }

        partial void ReleaseCriticalResources();

        ~Chunk()
        {
            ReleaseCriticalResources();
        }
    }
}