// 
// NEWorld/Game: TaskDispatcher.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2019 NEWorld Team
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Core;
using Core.Utilities;

namespace Game
{
    public interface IInstancedTask
    {
        void Task(int instance, int instanceCount);
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


    public interface IRegularReadOnlyTask : IInstancedTask
    {
    }

    [DeclareService("Game.TaskDispatcher")]
    public class TaskDispatcher : IDisposable
    {
        private readonly Barrier barrier;

        // TODO: replace it with lock-free structure.
        private readonly object mutex;
        private readonly ConcurrentBag<Action> readOnlyTasks;
        private readonly List<IRegularReadOnlyTask> regularReadOnlyTasks;
        private readonly List<IReadWriteTask> regularReadWriteTasks;
        private readonly List<Thread> threads;

        private RateController meter = new RateController(30);
        private List<IReadWriteTask> readWriteTasks, nextReadWriteTasks;
        private List<Action> renderTasks, nextRenderTasks;
        private bool shouldExit;

        public struct NextReadOnlyChanceS
        {
            public struct Awaiter : INotifyCompletion
            {
                public bool IsCompleted => false;

                public void GetResult()
                {
                }

                public void OnCompleted(Action continuation)
                {
                    ChunkService.TaskDispatcher.AddReadOnlyTask(continuation);
                }
            }

            public Awaiter GetAwaiter() { return new Awaiter();}
        }

        /**
         * \brief This type of tasks will be executed concurrently.
         *        Note that "ReadOnly" here is with respect to chunks
         *        data specifically. However please be aware of
         *        thread safety when you write something other than
         *        chunks.
         */
        public NextReadOnlyChanceS NextReadOnlyChance() { return new NextReadOnlyChanceS(); }

        public struct NextRenderChanceS
        {
            public struct Awaiter : INotifyCompletion
            {
                public bool IsCompleted => false;

                public void GetResult()
                {
                }

                public void OnCompleted(Action continuation)
                {
                    ChunkService.TaskDispatcher.AddRenderTask(continuation);
                }
            }

            public Awaiter GetAwaiter() { return new Awaiter();}
        }
        
        /**
         * \brief This type of tasks will be executed in main thread.
         *        Thus, it is safe to call OpenGL function inside.
         */
        public NextRenderChanceS NextRenderChance() { return new NextRenderChanceS(); }

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
            readOnlyTasks = new ConcurrentBag<Action>();
            regularReadOnlyTasks = new List<IRegularReadOnlyTask>();
            readWriteTasks = new List<IReadWriteTask>();
            nextReadWriteTasks = new List<IReadWriteTask>();
            regularReadWriteTasks = new List<IReadWriteTask>();
            renderTasks = new List<Action>();
            nextRenderTasks = new List<Action>();
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

        private void AddReadOnlyTask(Action task)
        {
            readOnlyTasks.Add(task);
        }

        public void Add(IReadWriteTask task)
        {
            lock (mutex)
            {
                nextReadWriteTasks.Add(task);
            }
        }

        private void AddRenderTask(Action task)
        {
            lock (mutex)
            {
                nextRenderTasks.Add(task);
            }
        }

        public void AddRegular(IRegularReadOnlyTask task)
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

        /**
         * \brief Process render tasks.
         *        This function should be called from the main thread.
         */
        public void ProcessRenderTasks()
        {
            lock (mutex)
            {
                foreach (var task in renderTasks)
                    task();
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
            for (var cnt = 0; cnt < regularReadOnlyTasks.Count; ++cnt)
                regularReadOnlyTasks[cnt].Task(i, TimeUsed.Length);

            // TODO: Make sure the no further tasks will be added before exiting this function
            // TODO: AddReadOnlyTask a timeout support for this to ensure the updation rate
            while (readOnlyTasks.TryTake(out var task))
                task();
        }

        private void ProcessReadWriteTasks()
        {
            foreach (var task in regularReadWriteTasks) task.Task();
            foreach (var task in readWriteTasks) task.Task();
            readWriteTasks.Clear();
        }

        private void QueueSwap()
        {
            Generic.Swap(ref readWriteTasks, ref nextReadWriteTasks);
        }

        private void ReleaseUnmanagedResources()
        {
            Reset();
            barrier?.Dispose();
        }
    }
}