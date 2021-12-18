using System;
using System.Collections.Generic;

namespace RoRamu.EventDrivenSystem
{
    public partial class EventDrivenSystem<T, S>
    {
        private bool TryFindEventNodeToRemove(IEvent<T, S> toRemove, out LinkedListNode<IEvent<T, S>> eventNode, out LinkedListNode<Snapshot<T, S>> snapshotNode)
        {
            if (toRemove == null)
            {
                throw new ArgumentNullException(nameof(toRemove));
            }

            LinkedListNode<IEvent<T, S>> currentEventNode;
            if (toRemove.Timestamp.Equals(this.CreatedTimestamp))
            {
                // Don't try to delete the events which sets the initial state.
                if (toRemove.Equals(this.Events.First.Value))
                {
                    eventNode = null;
                    snapshotNode = null;
                    return false;
                }

                // If the timestamp of the event is equal to the initial state's timestamp, start searching at the beginning.
                currentEventNode = this.Events.First;
                snapshotNode = this.Snapshots.First;
            }
            else
            {
                // Find the node just before the node from which we should start searching.
                // This would be the node that has a timestamp just less than the target node's timestamp.
                currentEventNode = this.FindEventBefore(toRemove.Timestamp, equalIsGreater: true, out snapshotNode);
            }

            // The first node to check will be 1 after the node we just selected
            currentEventNode = currentEventNode.Next;

            // Look through all nodes with the same timestamp until we get to the last one
            while (currentEventNode != null && toRemove.Timestamp.Equals(currentEventNode.Value.Timestamp))
            {
                // If we find a snapshot which points to a later event node (still with the same timestamp) move our snapshot pointer to it.
                if (snapshotNode.Next.Value.EventNode == currentEventNode)
                {
                    snapshotNode = snapshotNode.Next;
                }

                // Return if we find the one we're looking for
                if (currentEventNode.Value.Equals(toRemove))
                {
                    eventNode = currentEventNode;
                    return true;
                }

                currentEventNode = currentEventNode.Next;
            }

            eventNode = null;
            return false;
        }

        /// <summary>
        /// Retreats all snapshots up to and including the given snapshot.
        /// </summary>
        /// <param name="snapshotNode">The latest snapshot to retreat.</param>
        /// <returns>
        /// The last snapshot which was successfully reverted.  All subsequent snapshots should be
        /// regenerated.
        /// </returns>
        private LinkedListNode<Snapshot<T, S>> DecrementSnapshotsBefore(LinkedListNode<Snapshot<T, S>> snapshotNode)
        {
            if (snapshotNode == null)
            {
                throw new ArgumentNullException(nameof(snapshotNode));
            }

            // Revert all snapshots before (and including) the chosen snapshot.
            LinkedListNode<Snapshot<T, S>> result = null;
            LinkedListNode<Snapshot<T, S>> currentSnapshotNode = snapshotNode;
            while (currentSnapshotNode != this.Snapshots.First) // don't retreat the initial (creation event) snapshot.
            {
                if (currentSnapshotNode.Value is not RevertibleSnapshot<T, S> revertibleSnapshot)
                {
                    Snapshot<T, S> snapshot = currentSnapshotNode.Value;
                    snapshot.EventNode = snapshot.EventNode.Previous;

                    // We were not able to revert this node, so reset the result.
                    result = null;
                }
                else
                {
                    // The previous event node should never be null since it is guaranteed to be
                    // after the initial creation event.
                    revertibleSnapshot.MovePrevious();

                    // Since we successfully reverted the snapshot, remember this if it's the first
                    // one since a snapshot we could not revert.
                    result ??= currentSnapshotNode;
                }

                currentSnapshotNode = currentSnapshotNode.Previous;
            }

            // If we couldn't revert any snapshots, we need to regenerate them all from the beginning.
            if (result == null)
            {
                // If we just moved a snapshot onto the first event (i.e. initial state), remove it
                if (this.Snapshots.First.Value.EventNode == this.Snapshots.First.Next?.Value?.EventNode)
                {
                    this.Snapshots.Remove(this.Snapshots.First.Next);
                }
                result = this.Snapshots.First;
            }

            return result;
        }

        private void UpdateStateAfterRemovingEvent()
        {
            // Update counters and remove the first snapshot (skipping the initial creation snapshot).
            if (this.CurrentGap > 0)
            {
                this.CurrentGap--;
            }
            else
            {
                this.NextGap >>= 1;
                this.CurrentGap = this.NextGap == 0
                    ? 0
                    : this.NextGap - 1;
            }
        }
    }
}