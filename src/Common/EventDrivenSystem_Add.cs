using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
{
    public partial class EventDrivenSystem<T, S>
    {
        /// <summary>
        /// Inserts an event to maintain chronological order, and returns the snapshot node just
        /// before the inserted event.
        /// </summary>
        /// <param name="toAdd">The event to insert.</param>
        /// <param name="snapshotNode">The snapshot node just before the inserted event.</param>
        /// <returns>True if the event was successfully inserted, otherwise false.</returns>
        private bool TryInsertEvent(IEvent<T, S> toAdd, out LinkedListNode<Snapshot<T, S>> snapshotNode)
        {
            // Find the event just before the insertion point.
            LinkedListNode<IEvent<T, S>> currentEventNode = this.FindEventBefore(
                toAdd.Timestamp,
                equalIsGreater: false,
                out LinkedListNode<Snapshot<T, S>> currentSnapshotNode);

            // If the event already exists, don't add it again.
            if (toAdd.Equals(currentEventNode.Value))
            {
                snapshotNode = null;
                return false;
            }

            // Insert the event in the appropriate position to maintain chronological order.
            this.Events.AddAfter(currentEventNode, toAdd);

            snapshotNode = currentSnapshotNode;
            return true;
        }

        /// <summary>
        /// Advances all snapshots up to and including the given snapshot.
        /// </summary>
        /// <param name="snapshotNode">The latest snapshot to update.</param>
        private void IncrementSnapshotsBefore(LinkedListNode<Snapshot<T, S>> snapshotNode)
        {
            if (snapshotNode == null)
            {
                throw new ArgumentNullException(nameof(snapshotNode));
            }

            LinkedListNode<Snapshot<T, S>> currentSnapshotNode = snapshotNode;
            while (currentSnapshotNode != this.Snapshots.First) // don't update the initial (creation event) snapshot.
            {
                Snapshot<T, S> snapshot = currentSnapshotNode.Value;

                // The next event node should never be null since it is guaranteed to be before the newly inserted event.
                snapshot.MoveNext();

                currentSnapshotNode = currentSnapshotNode.Previous;
            }
        }

        private void UpdateStateAfterAddingEvent()
        {
            // Update counters and add new snapshot to the beginning if required.
            this.CurrentGap++;
            bool shouldAddSnapshot = false;
            if (this.NextGap == 0)
            {
                // Need to add the snapshot which will track the current state.
                shouldAddSnapshot = true;

                this.CurrentGap = 0;
                this.NextGap = 1;
            }
            else if (this.CurrentGap == this.NextGap)
            {
                // We need to add another snapshot to maintain the correct distribution.
                shouldAddSnapshot = true;

                // Update counters so we wait for twice as many events before adding another snapshot.
                this.CurrentGap = 0;
                this.NextGap <<= 1;
            }

            if (shouldAddSnapshot)
            {
                // Add a snapshot after the initial snapshot.
                Snapshot<T, S> newSnapshot = this.CloneSnapshot(this.Snapshots.First.Value);
                newSnapshot.MoveNext();
                this.Snapshots.AddAfter(this.Snapshots.First, newSnapshot);
            }
        }
    }
}