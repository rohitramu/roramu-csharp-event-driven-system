using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
{
    public partial class EventDrivenSystem<T, S>
    {
        private readonly LinkedList<IEvent<T, S>> Events = new();

        private readonly LinkedList<Snapshot<T, S>> Snapshots = new();

        private ulong NextGap = 0;

        private ulong CurrentGap = 0;

        private readonly ulong MaxGap;

        private readonly Func<S, S> CloneState;

        private Snapshot<T, S> CloneSnapshot(Snapshot<T, S> toClone)
        {
            return new()
            {
                State = this.CloneState(toClone.State),
                EventNode = toClone.EventNode,
            };
        }

        private void ThrowIfTimestampIsBeforeCreatedTime(T timestamp, bool equalIsGreater)
        {
            if (timestamp == null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            int compare = timestamp.CompareTo(this.CreatedTimestamp);
            if (equalIsGreater && compare <= 0)
            {
                throw new ArgumentException("Given timestamp is less than the created time, and equalIsGreater is true.", nameof(timestamp));
            }
            if (!equalIsGreater && compare < 0)
            {
                throw new ArgumentException("Given timestamp is less than or equal to the created time, and equalIsGreater is false.", nameof(timestamp));
            }
        }
    }
}