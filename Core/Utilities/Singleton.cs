// 
// Core: Singleton.cs
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
using System.Reflection;

// ReSharper disable InconsistentlySynchronizedField

namespace Core.Utilities
{
    /// <summary>
    ///     Represents errors that occur while creating a singleton.
    /// </summary>
    /// <remarks>
    ///     http://msdn.microsoft.com/en-us/library/ms229064(VS.80).aspx
    /// </remarks>
    [Serializable]
    public class SingletonException : Exception
    {
        /// <summary>
        ///     Initializes a new instance with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SingletonException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance with a reference to the inner
        ///     exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception,
        ///     or a null reference if no inner exception is specified.
        /// </param>
        public SingletonException(Exception innerException)
            : base(null, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance with a specified error message and a
        ///     reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception,
        ///     or a null reference if no inner exception is specified.
        /// </param>
        public SingletonException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Manages the single instance of a class.
    /// </summary>
    /// <remarks>
    ///     Generic variant of the strategy presented here : http://geekswithblogs.net/akraus1/articles/90803.aspx.
    ///     Prefered to http://www.yoda.arachsys.com/csharp/singleton.html, where static initialization doesn't allow
    ///     proper handling of exceptions, and doesn't allow retrying type initializers initialization later
    ///     (once a type initializer fails to initialize in .NET, it can't be re-initialized again).
    /// </remarks>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    public static class Singleton<T>
        where T : class
    {
        #region Constructors

        /// <summary>
        ///     Type-initializer to prevent type to be marked with beforefieldinit.
        /// </summary>
        /// <remarks>
        ///     This simply makes sure that static fields initialization occurs
        ///     when Instance is called the first time and not before.
        /// </remarks>
        static Singleton()
        {
        }

        #endregion Constructors

        #region Fields

        /// <summary>
        ///     The single instance of the target class.
        /// </summary>
        /// <remarks>
        ///     The volatile keyword makes sure to remove any compiler optimization that could make concurrent
        ///     threads reach a race condition with the double-checked lock pattern used in the Instance property.
        ///     See http://www.bluebytesoftware.com/blog/PermaLink,guid,543d89ad-8d57-4a51-b7c9-a821e3992bf6.aspx
        /// </remarks>
        private static volatile T _instance;

        /// <summary>
        ///     The dummy object used for locking.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Lock = new object();

        #endregion Fields


        #region Properties

        /// <summary>
        ///     Gets the single instance of the class.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    if (_instance == null) _instance = ConstructInstance();
                }

                return _instance;
            }
        }

        private static T ConstructInstance()
        {
            ConstructorInfo constructor;
            try
            {
                // Binding flags exclude public constructors.
                constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new Type[0], null);
            }
            catch (Exception exception)
            {
                throw new SingletonException(exception);
            }

            if (constructor == null || constructor.IsAssembly) // Also exclude internal constructors.
                throw new SingletonException($"A private or protected constructor is missing for '{typeof(T).Name}'.");

            return (T) constructor.Invoke(null);
        }

        #endregion Properties
    }
}