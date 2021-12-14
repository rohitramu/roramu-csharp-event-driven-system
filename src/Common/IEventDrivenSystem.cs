using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
{
    /// <summary>
    /// Represents an event-driven system.
    /// </summary>
    /// <typeparam name="T">
    /// The type which represents event ordering in this system.  For example, <see cref="int"/> or
    /// <see cref="long"/> to represent sequence numbers, or <see cref="DateTime"/> to represent
    /// time.
    /// <br/>
    /// Objects of this type should be immutable.
    /// </typeparam>
    /// <typeparam name="S">
    /// The type which represents the state of the system.
    /// <br/>
    /// Implementations will likely be more performant if objects of this type are not immutable.
    /// </typeparam>
    public interface IEventDrivenSystem<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        /// <summary>
        /// The current state of this system.
        /// </summary>
        S CurrentState { get; }

        /// <summary>
        /// The state of the system when it was first created.
        /// </summary>
        S InitialState { get; }

        /// <summary>
        /// The time that the last event took place.
        /// </summary>
        T LastModifiedTimestamp { get; }

        /// <summary>
        /// The time that this system was created.  No events can be added before this time.
        /// </summary>
        T CreatedTimestamp { get; }

        /// <summary>
        /// Gets all events in chronological order.
        /// </summary>
        /// <remarks>
        /// There will be 1 more event than the total number of events added - this is because the
        /// first event represents the creation of the system.
        /// </remarks>
        /// <returns>All events.</returns>
        IEnumerable<IEvent<T, S>> GetEvents();

        /// <summary>
        /// Gets events in chronological order from the given time.
        /// </summary>
        /// <remarks>
        /// If <see cref="CreatedTimestamp"/> is used for the 'from' parameter, there will be 1 more
        /// event than the total number of events added - this is because the first event represents
        /// the creation of the system.
        /// </remarks>
        /// <param name="from">The earliest time to start searching for events.</param>
        /// <returns>All events from the specified time.</returns>
        IEnumerable<IEvent<T, S>> GetEvents(T from);

        /// <summary>
        /// Gets events in chronological order in the given time window.
        /// </summary>
        /// <remarks>
        /// If <see cref="CreatedTimestamp"/> is used for the 'from' parameter, there will be 1 more
        /// event than the total number of events added - this is because the first event represents
        /// the creation of the system.
        /// </remarks>
        /// <param name="from">The start of the time window (inclusive).</param>
        /// <param name="to">The end of the time window (exclusive).</param>
        /// <returns>All events in the specified time window.</returns>
        IEnumerable<IEvent<T, S>> GetEvents(T from, T to);

        /// <summary>
        /// Adds the event to the system's history, and updates the current state if necessary.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when the given event has a timestamp that comes before <see cref="CreatedTimestamp"/>.
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
        // TODO: Make a separate interface for systems which will never have events removed from history.
        // The implementation can be optimized by allowing event implementations that model state
        // transitions that cannot be undone.  Remember to make a separate interface for undoable events.

        /// <summary>
        /// Gets the state of the system at the given time, based on the history of events.
        /// </summary>
        /// <param name="timestamp">The time at which to get the state of the system.</param>
        /// <returns>
        /// The state of the system at the given time, otherwise null if the given time preceeds
        /// the existence of this system (i.e. the system has not yet been created).
        /// <br/>
        /// If the given time is later than the last modified time of this system, the current state
        /// will be returned.
        /// </returns>
        S GetStateAtTimestamp(T timestamp);
    }
}