using System;

namespace RoRamu.EventDrivenSystem
{
    internal class SystemCreationEvent<T, S> : IEvent<T, S>
        where T : IEquatable<T>, IComparable<T>
        where S : IEquatable<S>
    {
        public T Timestamp { get; }
        public S State { get; }

        public SystemCreationEvent(T timestamp, S state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            this.Timestamp = timestamp;
            this.State = state;
        }

        public S Apply(S snapshot)
        {
            throw new InvalidOperationException("A system creation even cannot be applied to any state, as it defines the initial state.");
        }

        public bool Equals(IEvent<T, S> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is not SystemCreationEvent<T, S> otherEvent)
            {
                return false;
            }

            if (!this.Timestamp.Equals(otherEvent.Timestamp))
            {
                return false;
            }

            if (!this.State.Equals(otherEvent.State))
            {
                return false;
            }

            return true;
        }
    }
}