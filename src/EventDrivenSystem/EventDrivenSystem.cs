using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
{
    /// <inheritdoc/>
    public sealed partial class EventDrivenSystem<T, S> : IEventDrivenSystem<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        /// <inheritdoc/>
        public S CurrentState => this.Snapshots.Last.Value.State;

        /// <inheritdoc/>
        public S InitialState => this.Snapshots.First.Value.State;

        /// <inheritdoc/>
        public T LastModifiedTimestamp => this.Snapshots.Last.Value.Timestamp;

        /// <inheritdoc/>
        public T CreatedTimestamp => this.Snapshots.First.Value.Timestamp;

        /// <summary>
        /// Creates a new <see cref="EventDrivenSystem{T, S}"/>.
        /// </summary>
        /// <param name="createdTimestamp">When this system was created.</param>
        /// <param name="initialState">The initial state of the system.  No events can happen before this initial state.</param>
        /// <param name="deepCloneFunc">A method which creates a deep clone of a state object.</param>
        /// <param name="maxEventsBetweenSnapshots">
        /// A tuning variable which impacts how many state snapshots will be stored.  Use large
        /// values (or null) in scenarios where events are mostly added in sequence.  Use small
        /// values in scenarios where events are added in an unpredictable order.
        /// <br/>
        /// Large values will ensure that this type uses less memory than small values.
        /// </param>
        public EventDrivenSystem(
            T createdTimestamp,
            S initialState,
            Func<S, S> deepCloneFunc,
            ulong maxEventsBetweenSnapshots = ulong.MaxValue)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            this.CloneState = deepCloneFunc ?? throw new ArgumentNullException(nameof(deepCloneFunc));

            S state = this.CloneState(initialState);
            this.Events.AddFirst(new SystemCreationEvent<T, S>(createdTimestamp, state));
            this.Snapshots.AddFirst(new Snapshot<T, S>() { State = state, EventNode = this.Events.First });

            this.MaxGap = maxEventsBetweenSnapshots;
        }

        /// <inheritdoc/>
        public IEnumerable<IEvent<T, S>> GetEvents()
        {
            return this.GetEvents(this.CreatedTimestamp);
        }

        /// <inheritdoc/>
        public IEnumerable<IEvent<T, S>> GetEvents(T from)
        {
            foreach (IEvent<T, S> e in this.GetEvents(from, this.Events.Last.Value.Timestamp))
            {
                yield return e;
            }

            // Since "to" is not included in the time window, we need to also return the final event.
            yield return this.Events.Last.Value;
        }

        /// <inheritdoc/>
        public IEnumerable<IEvent<T, S>> GetEvents(T from, T to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }
            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }
            if (from.CompareTo(to) >= 0)
            {
                throw new ArgumentException($"The value for 'from' must come before the value for 'to'.", nameof(from));
            }

            if (to.CompareTo(this.CreatedTimestamp) <= 0 || from.CompareTo(this.LastModifiedTimestamp) > 0)
            {
                // Outside range of events so return an empty list.
                yield break;
            }

            // Get the first event in the window.
            LinkedListNode<IEvent<T, S>> currentEventNode;
            if (from.CompareTo(this.CreatedTimestamp) <= 0)
            {
                // Need to start looking from the very first event.
                currentEventNode = this.Events.First;
            }
            else
            {
                // Search for the first event in the time window.
                currentEventNode = FindEventBefore(from, equalIsGreater: true);
                currentEventNode = currentEventNode.Next;
            }

            // Return events until we hit the end of the time window.
            while (currentEventNode != null && currentEventNode.Value.Timestamp.CompareTo(to) < 0)
            {
                yield return currentEventNode.Value;
                currentEventNode = currentEventNode.Next;
            }
        }

        /// <inheritdoc/>
        public bool AddEvent(IEvent<T, S> toAdd)
        {
            if (toAdd == null)
            {
                throw new ArgumentNullException(nameof(toAdd));
            }
            if (toAdd.Timestamp.CompareTo(this.CreatedTimestamp) < 0)
            {
                // We cannot add an event before the creation time.
                return false;
            }

            // Insert the event and get the snapshot just before it.
            if (!this.TryInsertEvent(toAdd, out LinkedListNode<Snapshot<T, S>> snapshotNode))
            {
                // Failed to insert the event - most likely due to a duplicate.
                return false;
            }

            // Update all snapshots before the inserted event.
            this.IncrementSnapshotsBefore(snapshotNode);

            // Recalculate all snapshots after the inserted event.
            this.RegenerateSnapshotsAfter(snapshotNode);

            // Update counters and clean up snapshots.
            this.UpdateStateAfterAddingEvent();

            return true;
        }

        /// <inheritdoc/>
        public bool RemoveEvent(IEvent<T, S> toRemove)
        {
            if (toRemove == null)
            {
                throw new ArgumentNullException(nameof(toRemove));
            }
            if (toRemove.Timestamp.CompareTo(this.CreatedTimestamp) < 0)
            {
                // There are no events before the creation time.
                return false;
            }
            if (toRemove.Timestamp.CompareTo(this.Events.Last.Value.Timestamp) > 0)
            {
                // There are no events after the last event.
                return false;
            }

            // Find the node to remove.
            if (!this.TryFindEventNodeToRemove(
                toRemove,
                out LinkedListNode<IEvent<T, S>> eventNode,
                out LinkedListNode<Snapshot<T, S>> snapshotNode))
            {
                // Couldn't find the node.
                return false;
            }

            // Update all snapshots before the event to remove.
            // The result will tell us from which snapshot we need to regenerate snapshots.
            LinkedListNode<Snapshot<T, S>> regenSnapshotsAfter = this.DecrementSnapshotsBefore(snapshotNode);

            // Remove the event
            this.Events.Remove(eventNode);

            // Recalculate all snapshots after the removed event.
            this.RegenerateSnapshotsAfter(regenSnapshotsAfter);

            // Update counters and clean up snapshots.
            this.UpdateStateAfterRemovingEvent();

            return true;
        }

        /// <inheritdoc/>
        public S GetStateAtTimestamp(T timestamp)
        {
            if (timestamp == null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }
            if (timestamp.CompareTo(this.CreatedTimestamp) < 0)
            {
                throw new ArgumentException("Given timestamp is before creation time.");
            }

            //Find the snapshot just befor the given timestamp.
            LinkedListNode<Snapshot<T, S>> snapshotNode = this.FindSnapshotBefore(timestamp, equalIsGreater: false);

            // Build up the state from the snapshot to the event.
            Snapshot<T, S> result = this.CloneSnapshot(snapshotNode.Value);
            while (result.EventNode.Next != null
                && timestamp.CompareTo(result.EventNode.Next.Value.Timestamp) >= 0)
            {
                result.MoveNext();
            }

            return result.State;
        }
    }
}