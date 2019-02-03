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

using System;
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
        void Task();
    }

    /**
     * \brief This type of tasks will be executed in one thread.
     *        Thus, it is safe to do write opeartions inside
     *        without the need to worry thread safety.
     */
    public interface IReadWriteTask
    {
        void Task();
    }

    /**
     * \brief This type of tasks will be executed in main thread.
     *        Thus, it is safe to call OpenGL function inside.
     */
    public interface IRenderTask
    {
        void Task();
    }

    [DeclareService("Game.TaskDispatcher")]
    public class TaskDispatcher : IDisposable
    {
        private readonly Barrier barrier;

        // TODO: replace it with lock-free structure.
        private readonly object mutex;
        private readonly List<IReadOnlyTask> regularReadOnlyTasks;
        private readonly List<IReadWriteTask> regularReadWriteTasks;
        private readonly List<Thread> threads;
        
        private RateController meter = new RateController(30);
        private List<IReadOnlyTask> readOnlyTasks, nextReadOnlyTasks;
        private List<IReadWriteTask> readWriteTasks, nextReadWriteTasks;
        private List<IRenderTask> renderTasks, nextRenderTasks;
        private bool shouldExit;

        // Automatic Creation. We reserve one virtual processor for main thread
        public TaskDispatcher() : this(Environment.ProcessorCount - 1)
        {
        }

        /**
         * \brief Initialize the dispatcher and start threads.
         * \param threads.Count The number of threads in the thread pool
         * \param chunkService the chunk service that the dispatcher binds to
         */
        private TaskDispatcher(int threadNumber)
        {
            barrier = new Barrier(threadNumber);
            threads = new List<Thread>(threadNumber);
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

        public int[] TimeUsed { get; }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~TaskDispatcher()
        {
            ReleaseUnmanagedResources();
        }

        public void Start()
        {
            shouldExit = false;
            for (var i = 0; i < TimeUsed.Length; ++i)
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
            {
                nextReadOnlyTasks.Add(task);
            }
        }

        public void Add(IReadWriteTask task)
        {
            lock (mutex)
            {
                nextReadWriteTasks.Add(task);
            }
        }

        public void Add(IRenderTask task)
        {
            lock (mutex)
            {
                nextRenderTasks.Add(task);
            }
        }

        public void AddRegular(IReadOnlyTask task)
        {
            lock (mutex)
            {
                regularReadOnlyTasks.Add(task);
            }
        }

        public void AddRegular(IReadWriteTask task)
        {
            lock (mutex)
            {
                regularReadWriteTasks.Add(task);
            }
        }

        public int GetRegularReadOnlyTaskCount()
        {
            return regularReadOnlyTasks.Count;
        }

        public int GetRegularReadWriteTaskCount()
        {
            return regularReadWriteTasks.Count;
        }

        /**
         * \brief Process render tasks.
         *        This function should be called from the main thread.
         */
        public void ProcessRenderTasks()
        {
            lock (mutex)
            {
                foreach (var task in renderTasks)
                    task.Task();
                renderTasks.Clear();
                Generic.Swap(ref renderTasks, ref nextRenderTasks);
            }
        }

        public void Reset()
        {
            if (!shouldExit)
            {
                shouldExit = true;
                foreach (var thread in threads)
                    thread.Join();
                // TODO: Clear the queues
            }
        }

        private void Worker(int threadId)
        {
            while (!shouldExit)
            {
                ProcessReadonlyTasks(threadId);
                // The last finished thread is responsible to do writing jobs
                if (barrier.ParticipantsRemaining == 1)
                {
                    QueueSwap();
                    ProcessReadWriteTasks();
                    meter.Yield(); // Rate Control
                }

                TimeUsed[threadId] = meter.GetDeltaTimeMs();
                barrier.SignalAndWait();
            }
        }

        private void ProcessReadonlyTasks(int i)
        {
            for (; i < regularReadOnlyTasks.Count; i += threads.Count) regularReadOnlyTasks[i].Task();
            for (i -= regularReadOnlyTasks.Count; i < readOnlyTasks.Count; i += threads.Count)
                readOnlyTasks[i].Task();
        }

        private void ProcessReadWriteTasks()
        {
            foreach (var task in regularReadWriteTasks) task.Task();
            foreach (var task in readWriteTasks) task.Task();
            readWriteTasks.Clear();
        }

        private void QueueSwap()
        {
            readOnlyTasks.Clear();
            Generic.Swap(ref readOnlyTasks, ref nextReadOnlyTasks);
            Generic.Swap(ref readWriteTasks, ref nextReadWriteTasks);
        }

        private void ReleaseUnmanagedResources()
        {
            Reset();
            barrier?.Dispose();
        }
    }
}