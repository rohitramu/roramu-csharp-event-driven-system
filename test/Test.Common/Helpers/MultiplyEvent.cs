namespace RoRamu.EventSourcing.Test
{
    public class MultiplyEvent : IEvent<int, decimal>
    {
        public int Timestamp { get; }

        public decimal ToMultiply { get; }

        public MultiplyEvent(int timestamp, decimal toMultiply)
        {
            this.Timestamp = timestamp;
            this.ToMultiply = toMultiply;
        }

        public decimal Apply(decimal snapshot)
        {
            return snapshot * this.ToMultiply;
        }

        public decimal Undo(decimal snapshot)
        {
            return snapshot / this.ToMultiply;
        }

        public bool Equals(IEvent<int, decimal> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is not MultiplyEvent otherEvent)
            {
                return false;
            }

            if (otherEvent.Timestamp != this.Timestamp)
            {
                return false;
            }

            if (otherEvent.ToMultiply != this.ToMultiply)
            {
                return false;
            }

            return true;
        }
    }
}