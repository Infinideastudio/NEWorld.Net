// 
// NEWorld/Core: RateController.cs
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
using System.Threading;

namespace Core.Utilities
{
    /**
     * \brief Rate Control Helper. Used to controll task execution rate
     */
    public struct RateController
    {
        /**
         * \brief Construct an instance with a given execution rate
         * \param rate Exectution Rate
         */
        public RateController(int rate = 0)
        {
            this.rate = rate;
            last = due = DateTime.Now;
        }

        /**
         * \brief Synchronize the internal timer with system clock. For cases that the timer doesn't keep up or forced resets
         */
        public void Sync()
        {
            last = due = DateTime.Now;
        }

        /**
         * \brief Get elapsed time from the start of the tick, in milliseconds
         * \return Elapsed time from the start of the tick, in milliseconds
         */
        public int GetDeltaTimeMs()
        {
            return (DateTime.Now - last).Milliseconds;
        }

        /**
         * \brief Check if the deadline of the current tick has pased
         * \return true if the deadline is passed, false otherwise
         */
        public bool IsDue()
        {
            return rate <= 0 || DateTime.Now >= due;
        }

        /**
         * \brief Increase the internal timer by one tick. Sets the current due time as the starting time of the next tick
         */
        public void IncreaseTimer()
        {
            if (rate <= 0) return;
            last = due;
            due += TimeSpan.FromMilliseconds(1000 / rate);
        }

        /**
         * \brief End the current tick and wait until the next tick starts
         */
        public void Yield()
        {
            if (!IsDue())
                Thread.Sleep(due - DateTime.Now);
            else
                Sync();
            IncreaseTimer();
        }

        private readonly int rate;
        private DateTime due, last;
    }
}