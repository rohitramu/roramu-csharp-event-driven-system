using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
{
    public partial class EventDrivenSystem<T, S>
    {
        private void RegenerateSnapshotsAfter(LinkedListNode<Snapshot<T, S>> snapshotNode)
        {
            if (snapshotNode == null)
            {
                throw new ArgumentNullException(nameof(snapshotNode));
            }

            // No need to update anything if the given node is the final snapshot.
            if (snapshotNode == this.Snapshots.Last)
            {
                return;
            }

            S currentState = this.CloneState(snapshotNode.Value.State);
            LinkedListNode<Snapshot<T, S>> nextSnapshotNode = snapshotNode.Next;
            LinkedListNode<IEvent<T, S>> currentEventNode = snapshotNode.Value.EventNode.Next;
            while (currentEventNode != null)
            {
                currentState = currentEventNode.Value.Apply(currentState);
                if (nextSnapshotNode.Value.EventNode == currentEventNode)
                {
                    // Update the state in this snapshot.
                    nextSnapshotNode.Value.State = currentState;

                    // Move to the next snapshot.
                    nextSnapshotNode = nextSnapshotNode.Next;

                    // No more snapshots to update, to break out of this loop.
                    if (nextSnapshotNode == null)
                    {
                        // If this happens, we should also be at the end of the event nodes since
                        // the last snapshot should point to the last event node.
                        break;
                    }

                    // Make a copy of the state so we don't overwrite the state object we just put in the previous snapshot.
                    currentState = this.CloneState(currentState);
                }

                currentEventNode = currentEventNode.Next;
            }
        }
    }
}