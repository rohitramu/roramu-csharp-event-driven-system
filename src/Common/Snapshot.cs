using System;
using System.Collections.Generic;

namespace RoRamu.EventSourcing
{
    internal class Snapshot<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        public T Timestamp => this.EventNode.Value.Timestamp;

        public S State { get; set; }

        public LinkedListNode<IEvent<T, S>> EventNode { get; set; }

        public Snapshot<T, S> MoveNext()
        {
            if (this.EventNode.Next == null)
            {
                throw new InvalidOperationException("Cannot move to next state if there are no more events.");
            }

            this.EventNode = this.EventNode.Next;
            this.State = this.EventNode.Value.Apply(this.State);

            return this;
        }

        public Snapshot<T, S> MovePrevious()
        {
            if (this.EventNode.Previous == null)
            {
                throw new InvalidOperationException("Cannot move to previous state if there are no previous events.");
            }

            this.State = this.EventNode.Value.Undo(this.State);
            this.EventNode = this.EventNode.Previous;

            return this;
        }
    }
}