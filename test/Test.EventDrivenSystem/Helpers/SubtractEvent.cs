namespace RoRamu.EventSourcing.Test
{
    public class SubtractEvent : IRevertibleEvent<int, decimal>
    {
        public int Timestamp { get; }

        public decimal ToSubtract { get; }

        public SubtractEvent(int timestamp, decimal toSubtract)
        {
            this.Timestamp = timestamp;
            this.ToSubtract = toSubtract;
        }

        public decimal Apply(decimal snapshot)
        {
            return snapshot - this.ToSubtract;
        }

        public decimal Undo(decimal snapshot)
        {
            return snapshot + this.ToSubtract;
        }

        public bool Equals(IEvent<int, decimal> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is not SubtractEvent otherEvent)
            {
                return false;
            }

            if (otherEvent.Timestamp != this.Timestamp)
            {
                return false;
            }

            if (otherEvent.ToSubtract != this.ToSubtract)
            {
                return false;
            }

            return true;
        }
    }
}