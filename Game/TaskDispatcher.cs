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
            _threadNumber = threadNumber;
            _chunkService = chunkService;
            TimeUsed = new int[threadNumber];
            _mutex = new object();
            _readOnlyTasks = new List<IReadOnlyTask>();
            _nextReadOnlyTasks = new List<IReadOnlyTask>();
            _regularReadOnlyTasks = new List<IReadOnlyTask>();
            _readWriteTasks = new List<IReadWriteTask>();
            _nextReadWriteTasks = new List<IReadWriteTask>();
            _regularReadWriteTasks = new List<IReadWriteTask>();
            _renderTasks = new List<IRenderTask>();
            _nextRenderTasks = new List<IRenderTask>();
        }

        ~TaskDispatcher()
        {
            if (!_shouldExit) Stop();
        }

        public void Start()
        {
            _shouldExit = false;
            _roundNumber = 0;
            _numberOfUnfinishedThreads = _threadNumber;
            _threads = new List<Thread>(_threadNumber);
            for (var i = 0; i < _threadNumber; ++i)
            {
                var trd = new Thread(() => { Worker(i); });
                trd.Start();
                _threads.Add(trd);
            }
        }

        public void AddReadOnlyTask(IReadOnlyTask task)
        {
            lock (_mutex)
                _nextReadOnlyTasks.Add(task);
        }

        public void AddReadWriteTask(IReadWriteTask task)
        {
            lock (_mutex)
                _nextReadWriteTasks.Add(task);
        }

        public void AddRenderTask(IRenderTask task)
        {
            lock (_mutex)
                _nextRenderTasks.Add(task);
        }

        public void AddRegularReadOnlyTask(IReadOnlyTask task)
        {
            lock (_mutex)
                _regularReadOnlyTasks.Add(task);
        }

        public void AddRegularReadWriteTask(IReadWriteTask task)
        {
            lock (_mutex)
                _regularReadWriteTasks.Add(task);
        }

        public int GetRegularReadOnlyTaskCount() => _regularReadOnlyTasks.Count;

        public int GetRegularReadWriteTaskCount() => _regularReadWriteTasks.Count;

        /**
         * \brief Process render tasks.
         *        This function should be called from the main thread.
         */
        public void ProcessRenderTasks()
        {
            lock (_mutex)
            {
                foreach (var task in _renderTasks)
                    task.Task(_chunkService);
                _renderTasks.Clear();
                Generic.Swap(ref _renderTasks, ref _nextRenderTasks);
            }
        }

        public int[] TimeUsed { get; }

        public void Stop()
        {
            _shouldExit = true;
            foreach (var thread in _threads)
                thread.Join();
        }

        private void Worker(int threadId)
        {
            var meter = new RateController(30);
            while (!_shouldExit)
            {
                // A tick starts
                var currentRoundNumber = _roundNumber;
                // Process read-only work.
                for (var i = threadId; i < _readOnlyTasks.Count; i += _threadNumber)
                {
                    _readOnlyTasks[i].Task(_chunkService);
                }

                // Finish the tick
                TimeUsed[threadId] = meter.GetDeltaTimeMs();

                // The last finished thread is responsible to do writing jobs
                if (Interlocked.Decrement(ref _numberOfUnfinishedThreads) == 0)
                {
                    // All other threads have finished?
                    foreach (var task in _readWriteTasks)
                    {
                        task.Task(_chunkService);
                    }

                    // ...and finish up!
                    _readOnlyTasks.Clear();
                    _readWriteTasks.Clear();
                    foreach (var task in _regularReadOnlyTasks)
                        _nextReadOnlyTasks.Add(task.Clone());
                    foreach (var task in _regularReadWriteTasks)
                        _nextReadWriteTasks.Add(task.Clone());
                    Generic.Swap(ref _readOnlyTasks, ref _nextReadOnlyTasks);
                    Generic.Swap(ref _readWriteTasks, ref _nextReadWriteTasks);

                    // Limit UPS
                    meter.Yield();

                    // Time to move to next tick!
                    // Notify other threads that we are good to go
                    _numberOfUnfinishedThreads = _threadNumber;
                    Interlocked.Increment(ref _roundNumber);
                }
                else
                {
                    meter.Yield();
                    // Wait for other threads...
                    while (_roundNumber == currentRoundNumber)
                        Thread.Yield();
                }
            }
        }

        // TODO: replace it with lock-free structure.
        private readonly object _mutex;
        private List<IReadOnlyTask> _readOnlyTasks, _nextReadOnlyTasks, _regularReadOnlyTasks;
        private List<IReadWriteTask> _readWriteTasks, _nextReadWriteTasks, _regularReadWriteTasks;
        private List<IRenderTask> _renderTasks, _nextRenderTasks;
        private List<Thread> _threads;
        private readonly int _threadNumber;
        private int _roundNumber;
        private int _numberOfUnfinishedThreads;
        private bool _shouldExit;

        private ChunkService _chunkService;
    }
}