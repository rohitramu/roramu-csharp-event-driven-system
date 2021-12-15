using System;

namespace RoRamu.EventSourcing
{
    /// <summary>
    /// Represents an event which can change the state of a system, and can also be reverted.
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
    public interface IRevertibleEvent<T, S> : IEvent<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        /// <summary>
        /// Reverts the event from the given state snapshot.  <see cref="Undo(S)"/> should only be
        /// called if this event was the last event that was applied to the given state snapshot.
        /// Calling <see cref="Undo(S)"/> at other times may result in a corrupted state.
        /// </summary>
        /// <remarks>
        /// Implementations should be as fast as possible - this method may be called many times
        /// in implementations of <see cref="IEventDrivenSystem{T,S}"/>.
        /// </remarks>
        /// <param name="snapshot">The snapshot to revert this event from.</param>
        /// <returns>
        /// The previous state if the event was successfully reverted, otherwise null.
        /// </returns>
        S Undo(S snapshot);
    }
}