using System;
using System.Collections.Generic;

namespace RoRamu.EventDrivenSystem
{
    /// <summary>
    /// Represents an event-driven system which can be updated.
    /// </summary>
    /// <typeparam name="T">
    /// The type which represents event ordering in this system.  For example, <see cref="int"/> to
    /// represent sequence numbers, or <see cref="DateTime"/> to represent time.
    /// <br/>
    /// Objects of this type should be immutable.
    /// </typeparam>
    /// <typeparam name="S">
    /// The type which represents the state of the system.
    /// <br/>
    /// Implementations will likely be more performant if objects of this type are mutable.
    /// </typeparam>
    public interface IEventDrivenSystem<T, S> : IReadOnlyEventDrivenSystem<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        /// <summary>
        /// Adds the event to the system's history, and updates the current state if necessary.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when the given event has a timestamp that comes before
        /// <see cref="IReadOnlyEventDrivenSystem{T, S}.CreatedTimestamp"/>.
        /// </exception>
        /// <param name="toAdd">The event to add.</param>
        /// <returns>True if the event was successfully added, otherwise false.</returns>
        bool AddEvent(IEvent<T, S> toAdd);

        /// <summary>
        /// Removes the event from the system's history, and updates the current state if necessary.
        /// </summary>
        /// <param name="toRemove">The event to remove.</param>
        /// <returns>True if the event was successfully removed, otherwise false.</returns>
        bool RemoveEvent(IEvent<T, S> toRemove);
    }
}