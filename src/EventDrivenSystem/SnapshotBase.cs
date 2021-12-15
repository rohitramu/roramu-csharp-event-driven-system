using System;
using System.Collections.Generic;

namespace RoRamu.EventDrivenSystem
{
    internal abstract class SnapshotBase<E, T, S>
        where E : IEvent<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        public T Timestamp => this.EventNode.Value.Timestamp;

        public S State { get; set; }

        public LinkedListNode<E> EventNode { get; set; }
    }
}