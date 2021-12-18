using System;

namespace RoRamu.EventDrivenSystem
{
    /// <summary>
    /// Represents an event which can change the state of a system.
    /// </summary>
    /// <remarks>
    /// Implementations should be immutable.
    /// </remarks>
    /// <typeparam name="T">
    /// A type which represents the ordering of events, e.g. <see cref="DateTime"/>.
    /// </typeparam>
    /// <typeparam name="S">
    /// A type which represents the state of a system that this event can be applied to.
    /// </typeparam>
    public interface IEvent<T, S> : IEquatable<IEvent<T, S>>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        /// <summary>
        /// The time that this event occured.
        /// </summary>
        T Timestamp { get; }

        /// <summary>
        /// Applies the event to the given state snapshot.
        /// </summary>
        /// <remarks>
        /// Implementations should be as fast as possible - this method may be called many times
        /// in implementations of <see cref="IEventDrivenSystem{T,S}"/>.
        /// </remarks>
        /// <param name="snapshot">The snapshot to apply this event to.</param>
        /// <returns>
        /// The next state if the event was successfully applied, otherwise null.
        /// </returns>
        S Apply(S snapshot);
    }
}