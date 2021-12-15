using System;

namespace RoRamu.EventSourcing
{
    internal class Snapshot<T, S> : SnapshotBase<IEvent<T, S>, T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        public void MoveNext()
        {
            if (this.EventNode.Next == null)
            {
                throw new InvalidOperationException("Cannot move to next state if there are no more events.");
            }

            this.EventNode = this.EventNode.Next;
            this.State = this.EventNode.Value.Apply(this.State);
        }
    }
}