namespace RoRamu.EventSourcing.Test
{
    public class DivideEvent : IRevertibleEvent<int, decimal>
    {
        public int Timestamp { get; }

        public decimal ToDivide { get; }

        public DivideEvent(int timestamp, decimal toDivide)
        {
            this.Timestamp = timestamp;
            this.ToDivide = toDivide;
        }

        public decimal Apply(decimal snapshot)
        {
            return snapshot / this.ToDivide;
        }

        public decimal Undo(decimal snapshot)
        {
            return snapshot * this.ToDivide;
        }

        public bool Equals(IEvent<int, decimal> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is not DivideEvent otherEvent)
            {
                return false;
            }

            if (otherEvent.Timestamp != this.Timestamp)
            {
                return false;
            }

            if (otherEvent.ToDivide != this.ToDivide)
            {
                return false;
            }

            return true;
        }
    }
}