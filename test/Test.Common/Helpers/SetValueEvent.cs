namespace RoRamu.EventSourcing.Test
{
    public class SetValueEvent : IEvent<int, decimal>
    {
        public int Timestamp { get; }

        public decimal NewValue { get; }

        public SetValueEvent(int timestamp, decimal newValue)
        {
            this.Timestamp = timestamp;
            this.NewValue = newValue;
        }

        public decimal Apply(decimal snapshot)
        {
            return this.NewValue;
        }

        public bool Equals(IEvent<int, decimal> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is not SetValueEvent otherEvent)
            {
                return false;
            }

            if (otherEvent.Timestamp != this.Timestamp)
            {
                return false;
            }

            if (otherEvent.NewValue != this.NewValue)
            {
                return false;
            }

            return true;
        }
    }
}