using System;
using System.Collections.Generic;

namespace RoRamu.EventDrivenSystem
{
    public partial class EventDrivenSystem<T, S>
    {
        /// <summary>
        /// Finds the snapshot just before the given timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp to search with.</param>
        /// <param name="equalIsGreater">
        /// If true, snapshots with a timestamp equal to the given timestamp will be considered as
        /// greater - thus, the selected snapshot will be the one just before the first snapshot
        /// that has the given timestamp.
        /// If false, snapshots with a timestamp equal to the given timestamp will be considered as
        /// smaller - thus, the selected snapshot will be the last one with the given timestamp.
        /// </param>
        /// <returns>The snapshot just before the given timestamp.</returns>
        private LinkedListNode<Snapshot<T, S>> FindSnapshotBefore(T timestamp, bool equalIsGreater)
        {
            if (timestamp == null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            ThrowIfTimestampIsBeforeCreatedTime(timestamp, equalIsGreater);

            LinkedListNode<Snapshot<T, S>> currentSnapshotNode = this.Snapshots.Last;
            int compare = timestamp.CompareTo(currentSnapshotNode.Value.Timestamp);
            while ((equalIsGreater && compare <= 0) || (!equalIsGreater && compare < 0))
            {
                currentSnapshotNode = currentSnapshotNode.Previous;
                compare = timestamp.CompareTo(currentSnapshotNode.Value.Timestamp);
            }

            return currentSnapshotNode;
        }

        /// <summary>
        /// Find the event just before the given timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp to search with.</param>
        /// <param name="snapshot">The snapshot to start the search from.</param>
        /// <param name="equalIsGreater">
        /// If true, events with a timestamp equal to the given timestamp will be considered as
        /// greater - thus, the selected event will be the one just before the first event
        /// that has the given timestamp.
        /// If false, events with a timestamp equal to the given timestamp will be considered as
        /// smaller - thus, the selected event will be the last one with the equal timestamp.
        /// </param>
        /// <returns>The event just before the given timestamp.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822:Can be marked as static.", Justification = "Could potentially need member references in future.")]
        private LinkedListNode<IEvent<T, S>> FindEventBefore(T timestamp, Snapshot<T, S> snapshot, bool equalIsGreater)
        {
            if (timestamp == null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            ThrowIfTimestampIsBeforeCreatedTime(timestamp, equalIsGreater);

            LinkedListNode<IEvent<T, S>> currentEventNode = snapshot.EventNode;
            if (currentEventNode.Next == null)
            {
                return currentEventNode;
            }

            int compare = timestamp.CompareTo(currentEventNode.Next.Value.Timestamp);
            while (currentEventNode.Next != null
                && ((equalIsGreater && compare > 0) || (!equalIsGreater && compare >= 0)))
            {
                currentEventNode = currentEventNode.Next;

                if (currentEventNode.Next != null)
                {
                    compare = timestamp.CompareTo(currentEventNode.Next.Value.Timestamp);
                }
            }

            return currentEventNode;
        }

        /// <summary>
        /// Find the event just before the given timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp to search with.</param>
        /// <param name="equalIsGreater">
        /// If true, events with a timestamp equal to the given timestamp will be considered as
        /// greater - thus, the selected event will be the one just before the first event
        /// that has the given timestamp.
        /// If false, events with a timestamp equal to the given timestamp will be considered as
        /// smaller - thus, the selected event will be the last one with the equal timestamp.
        /// </param>
        /// <param name="snapshotBefore">
        /// The snapshot just before the given timestamp.  This will be at or before the returned
        /// event node.
        /// </param>
        /// <returns>The event just before the given timestamp.</returns>
        private LinkedListNode<IEvent<T, S>> FindEventBefore(T timestamp, bool equalIsGreater, out LinkedListNode<Snapshot<T, S>> snapshotBefore)
        {
            if (timestamp == null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            // Narrow down our search by finding the snapshot just before the insertion point.
            snapshotBefore = this.FindSnapshotBefore(timestamp, equalIsGreater);

            // Find the event just before the insertion point - we know it is between the current snapshot and the next snapshot.
            LinkedListNode<IEvent<T, S>> currentEventNode = this.FindEventBefore(timestamp, snapshotBefore.Value, equalIsGreater);

            return currentEventNode;
        }

        /// <summary>
        /// Find the event just before the given timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp to search with.</param>
        /// <param name="equalIsGreater">
        /// If true, events with a timestamp equal to the given timestamp will be considered as
        /// greater - thus, the selected event will be the one just before the first event
        /// that has the given timestamp.
        /// If false, events with a timestamp equal to the given timestamp will be considered as
        /// smaller - thus, the selected event will be the last one with the equal timestamp.
        /// </param>
        /// <returns>The event just before the given timestamp.</returns>
        private LinkedListNode<IEvent<T, S>> FindEventBefore(T timestamp, bool equalIsGreater)
        {
            if (timestamp == null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            return this.FindEventBefore(timestamp, equalIsGreater, out _);
        }
    }
}