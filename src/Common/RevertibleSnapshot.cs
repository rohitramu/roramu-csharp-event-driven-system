using System;

namespace RoRamu.EventSourcing
{
    internal class RevertibleSnapshot<T, S> : SnapshotBase<IRevertibleEvent<T, S>, T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        public void MovePrevious()
        {
            if (this.EventNode.Previous == null)
            {
                throw new InvalidOperationException("Cannot move to previous state if there are no previous events.");
            }

            this.State = this.EventNode.Value.Undo(this.State);
            this.EventNode = this.EventNode.Previous;
        }
    }
}