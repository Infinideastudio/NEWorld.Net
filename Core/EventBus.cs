// 
// NEWorld/Core: EventBus.cs
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
using System.Collections.Generic;
using System.Threading;

namespace Core
{
    public class DeclareBusEventHandlerAttribute : Attribute
    {
    }

    public static class EventBus
    {
        public delegate void EventHandler<in T>(object sender, T payload);

        private static readonly Dictionary<Type, object> EventHandlers = new Dictionary<Type, object>();

        public static void Add<T>(EventHandler<T> handler)
        {
            var slot = GetOrCreateSlot<T>();
            slot.Rwl.EnterWriteLock();
            slot.Handlers += handler;
            slot.Rwl.ExitWriteLock();
        }

        private static Slot<T> GetOrCreateSlot<T>()
        {
            Slot<T> slot;
            lock (EventHandlers)
            {
                if (EventHandlers.TryGetValue(typeof(T), out var value))
                {
                    slot = (Slot<T>) value;
                }
                else
                {
                    slot = new Slot<T>();
                    EventHandlers.Add(typeof(T), slot);
                }
            }

            return slot;
        }

        private static ISlot GetOrCreateSlot(Type type)
        {
            ISlot slot;
            lock (EventHandlers)
            {
                if (EventHandlers.TryGetValue(type, out var value))
                {
                    slot = (ISlot) value;
                }
                else
                {
                    slot = (ISlot) Activator.CreateInstance(typeof(Slot<>).MakeGenericType(type));
                    EventHandlers.Add(type, slot);
                }
            }

            return slot;
        }

        public static void Remove<T>(EventHandler<T> handler)
        {
            Slot<T> slot;
            lock (EventHandlers)
            {
                if (EventHandlers.TryGetValue(typeof(T), out var value))
                    slot = (Slot<T>) value;
                else
                    return;
            }

            slot.Rwl.EnterWriteLock();
            slot.Handlers -= handler;
            slot.Rwl.ExitWriteLock();
        }

        public static void AddCollection(object obj)
        {
            ProcessCollection(obj, true);
        }

        public static void RemoveCollection(object obj)
        {
            ProcessCollection(obj, false);
        }

        private static void ProcessCollection(object obj, bool add)
        {
            foreach (var method in obj.GetType().GetMethods())
                if (method.IsDefined(typeof(DeclareBusEventHandlerAttribute), true))
                {
                    var payload = method.GetParameters();
                    if (payload.Length == 2)
                    {
                        var payloadType = payload[payload.Length - 1].ParameterType;
                        var handlerType = typeof(EventHandler<>).MakeGenericType(payloadType);
                        var del = method.IsStatic
                            ? Delegate.CreateDelegate(handlerType, method)
                            : Delegate.CreateDelegate(handlerType, obj, method);
                        if (add)
                            GetOrCreateSlot(payloadType).Add(del);
                        else
                            GetOrCreateSlot(payloadType).Remove(del);
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Excepting Arguments (System.Object, T) But Got {payload.Length} at Handler {method}" +
                            ", Stopping. Note that Previously Added Handlers will NOT be Removed");
                    }
                }
        }

        public static void Broadcast<T>(object sender, T payload)
        {
            Slot<T> slot = null;
            lock (EventHandlers)
            {
                if (EventHandlers.TryGetValue(typeof(T), out var value))
                    slot = (Slot<T>) value;
            }

            slot?.Invoke(sender, payload);
        }

        private interface ISlot
        {
            void Add(Delegate handler);
            void Remove(Delegate handler);
        }

        private class Slot<T> : ISlot
        {
            public readonly ReaderWriterLockSlim Rwl = new ReaderWriterLockSlim();

            public void Add(Delegate handler)
            {
                Rwl.EnterWriteLock();
                typeof(Slot<T>).GetEvents()[0].AddMethod.Invoke(this, new object[] {handler});
                Rwl.ExitWriteLock();
            }

            public void Remove(Delegate handler)
            {
                Rwl.EnterWriteLock();
                typeof(Slot<T>).GetEvents()[0].RemoveMethod.Invoke(this, new object[] {handler});
                Rwl.ExitWriteLock();
            }

            public event EventHandler<T> Handlers;

            public void Invoke(object sender, T payload)
            {
                Rwl.EnterReadLock();
                Handlers?.Invoke(sender, payload);
                Rwl.ExitReadLock();
            }
        }
    }
}