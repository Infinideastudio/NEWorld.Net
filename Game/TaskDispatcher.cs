// 
// Game: TaskDispatcher.cs
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

using System.Collections.Generic;
using System.Threading;
using Core;
using Core.Utilities;

namespace Game
{
    // TODO: we can add a `finished` flag in DEBUG mode
    //       to verify that all tasks are indeed processed.
    /**
     * \brief This type of tasks will be executed concurrently.
     *        Note that "ReadOnly" here is with respect to chunks
     *        data specifically. However please be aware of
     *        thread safety when you write something other than
     *        chunks.
     */
    public interface IReadOnlyTask
    {
        void Task(ChunkService srv);
        IReadOnlyTask Clone();
    }

    /**
     * \brief This type of tasks will be executed in one thread.
     *        Thus, it is safe to do write opeartions inside
     *        without the need to worry thread safety.
     */
    public interface IReadWriteTask
    {
        void Task(ChunkService srv);
        IReadWriteTask Clone();
    }

    /**
     * \brief This type of tasks will be executed in main thread.
     *        Thus, it is safe to call OpenGL function inside.
     */
    public interface IRenderTask
    {
        void Task(ChunkService srv);
        IRenderTask Clone();
    }

    public class TaskDispatcher
    {
        /**
         * \brief Initialize the dispatcher and start threads.
         * \param threadNumber The number of threads in the thread pool
         * \param chunkService the chunk service that the dispatcher binds to
         */
        public TaskDispatcher(int threadNumber, ChunkService chunkService)
        {
            this.threadNumber = threadNumber;
            this.chunkService = chunkService;
            TimeUsed = new int[threadNumber];
            mutex = new object();
            readOnlyTasks = new List<IReadOnlyTask>();
            nextReadOnlyTasks = new List<IReadOnlyTask>();
            regularReadOnlyTasks = new List<IReadOnlyTask>();
            readWriteTasks = new List<IReadWriteTask>();
            nextReadWriteTasks = new List<IReadWriteTask>();
            regularReadWriteTasks = new List<IReadWriteTask>();
            renderTasks = new List<IRenderTask>();
            nextRenderTasks = new List<IRenderTask>();
        }

        ~TaskDispatcher()
        {
            if (!shouldExit) Stop();
        }

        public void Start()
        {
            shouldExit = false;
            roundNumber = 0;
            numberOfUnfinishedThreads = threadNumber;
            threads = new List<Thread>(threadNumber);
            for (var i = 0; i < threadNumber; ++i)
            {
                var i1 = i;
                var trd = new Thread(() => { Worker(i1); });
                trd.Start();
                threads.Add(trd);
            }
        }

        public void Add(IReadOnlyTask task)
        {
            lock (mutex)
                nextReadOnlyTasks.Add(task);
        }

        public void Add(IReadWriteTask task)
        {
            lock (mutex)
                nextReadWriteTasks.Add(task);
        }

        public void Add(IRenderTask task)
        {
            lock (mutex)
                nextRenderTasks.Add(task);
        }

        public void AddRegular(IReadOnlyTask task)
        {
            lock (mutex)
                regularReadOnlyTasks.Add(task);
        }

        public void AddRegular(IReadWriteTask task)
        {
            lock (mutex)
                regularReadWriteTasks.Add(task);
        }

        public int GetRegularReadOnlyTaskCount() => regularReadOnlyTasks.Count;

        public int GetRegularReadWriteTaskCount() => regularReadWriteTasks.Count;

        /**
         * \brief Process render tasks.
         *        This function should be called from the main thread.
         */
        public void ProcessRenderTasks()
        {
            lock (mutex)
            {
                foreach (var task in renderTasks)
                    task.Task(chunkService);
                renderTasks.Clear();
                Generic.Swap(ref renderTasks, ref nextRenderTasks);
            }
        }

        public int[] TimeUsed { get; }

        public void Stop()
        {
            shouldExit = true;
            foreach (var thread in threads)
                thread.Join();
        }

        private void Worker(int threadId)
        {
            var meter = new RateController(30);
            while (!shouldExit)
            {
                // A tick starts
                var currentRoundNumber = roundNumber;
                // Process read-only work.
                for (var i = threadId; i < readOnlyTasks.Count; i += threadNumber)
                {
                    readOnlyTasks[i].Task(chunkService);
                }

                // Finish the tick
                TimeUsed[threadId] = meter.GetDeltaTimeMs();

                // The last finished thread is responsible to do writing jobs
                if (Interlocked.Decrement(ref numberOfUnfinishedThreads) == 0)
                {
                    // All other threads have finished?
                    foreach (var task in readWriteTasks)
                    {
                        task.Task(chunkService);
                    }

                    // ...and finish up!
                    readOnlyTasks.Clear();
                    readWriteTasks.Clear();
                    foreach (var task in regularReadOnlyTasks)
                        nextReadOnlyTasks.Add(task.Clone());
                    foreach (var task in regularReadWriteTasks)
                        nextReadWriteTasks.Add(task.Clone());
                    Generic.Swap(ref readOnlyTasks, ref nextReadOnlyTasks);
                    Generic.Swap(ref readWriteTasks, ref nextReadWriteTasks);

                    // Limit UPS
                    meter.Yield();

                    // Time to move to next tick!
                    // Notify other threads that we are good to go
                    numberOfUnfinishedThreads = threadNumber;
                    Interlocked.Increment(ref roundNumber);
                }
                else
                {
                    meter.Yield();
                    // Wait for other threads...
                    while (roundNumber == currentRoundNumber)
                        Thread.Yield();
                }
            }
        }

        // TODO: replace it with lock-free structure.
        private readonly object mutex;
        private List<IReadOnlyTask> readOnlyTasks, nextReadOnlyTasks;
        private readonly List<IReadOnlyTask> regularReadOnlyTasks;
        private List<IReadWriteTask> readWriteTasks, nextReadWriteTasks;
        private readonly List<IReadWriteTask> regularReadWriteTasks;
        private List<IRenderTask> renderTasks, nextRenderTasks;
        private List<Thread> threads;
        private readonly int threadNumber;
        private int roundNumber;
        private int numberOfUnfinishedThreads;
        private bool shouldExit;

        private readonly ChunkService chunkService;
    }
}