using System;
using System.Threading;

namespace LP.Test.Framework.Core
{
    /// <summary>
    /// Implements a lazy-initialized object that can be disposed and recreated.
    /// </summary>
    /// <typeparam name="T">The type of the object that this <see cref="RecyclableLazy{T}"/> contains.</typeparam>
    public class RecyclableLazy<T>
        where T : IDisposable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RecyclableLazy{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory delegate used to instantiate the lazy object.</param>
        public RecyclableLazy(Func<T> factory)
        {
            this.factory = factory;

            lock (syncRoot)
            {
                container = new Lazy<T>(this.factory, LazyThreadSafetyMode.ExecutionAndPublication);
            }
        }

        /// <summary>
        /// Holds the factory delegate for future usage.
        /// </summary>
        private readonly Func<T> factory;

        /// <summary>
        /// This synchronization root is used to avoid problems with concurrency while recycling the inner lazy container.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// The lazy container used to hold the value internally.
        /// </summary>
        private Lazy<T> container;

        /// <summary>
        /// Gets the value of the stored object.
        /// </summary>
        /// <remarks>If the object is not created, the <see cref="factory"/> delegate is invoked.</remarks>
        public T Value => container.Value;

        /// <summary>
        /// Checks whether the value of this lazy object has been created.
        /// </summary>
        public bool IsValueCreated => container.IsValueCreated;

        /// <summary>
        /// Disposes the lazy object value and resets the lazy container.
        /// </summary>
        /// <remarks>The inner value will only be recreated the next time the value is accessed.</remarks>
        public void Reset()
        {
            if (IsValueCreated)
            {
                Value.Dispose();
            }
            lock (syncRoot)
            {
                container = null;
                container = new Lazy<T>(factory, LazyThreadSafetyMode.ExecutionAndPublication);
            }
        }

    }
}