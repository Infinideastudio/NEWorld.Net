using System;
using System.Threading;

namespace Game.World
{
    public partial class Chunk
    {
        private long mLastRequestTime;

        public bool CheckReleaseable()
        {
            return DateTime.Now - new DateTime(Interlocked.Read(ref mLastRequestTime)) > TimeSpan.FromSeconds(10);
        }

        public void MarkRequest()
        {
            mLastRequestTime = DateTime.Now.Ticks;
        }
    }
}