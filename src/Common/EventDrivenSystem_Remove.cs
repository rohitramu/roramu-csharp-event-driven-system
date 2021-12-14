using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
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
        private void DecrementSnapshotsBefore(LinkedListNode<Snapshot<T, S>> snapshotNode)
        {
            if (snapshotNode == null)
            {
                throw new ArgumentNullException(nameof(snapshotNode));
            }

            LinkedListNode<Snapshot<T, S>> currentSnapshotNode = snapshotNode;
            while (currentSnapshotNode != this.Snapshots.First) // don't retreat the initial (creation event) snapshot.
            {
                Snapshot<T, S> snapshot = currentSnapshotNode.Value;

                // The previous event node should never be null since it is guaranteed to be after the initial creation event.
                snapshot.MovePrevious();

                currentSnapshotNode = currentSnapshotNode.Previous;
            }
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

                // Remove the first snapshot.
                LinkedListNode<Snapshot<T, S>> nodeToRemove = this.Snapshots.First.Next;
                if (nodeToRemove != null && nodeToRemove.Value.EventNode == this.Events.First)
                {
                    this.Snapshots.Remove(nodeToRemove);
                }
            }
        }
    }
}