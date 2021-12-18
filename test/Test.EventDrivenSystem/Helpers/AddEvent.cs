namespace RoRamu.EventDrivenSystem.Test
{
    public class AddEvent : IRevertibleEvent<int, decimal>
    {
        public int Timestamp { get; }

        public decimal ToAdd { get; }

        public AddEvent(int timestamp, decimal toAdd)
        {
            this.Timestamp = timestamp;
            this.ToAdd = toAdd;
        }

        public decimal Apply(decimal snapshot)
        {
            return snapshot + this.ToAdd;
        }

        public decimal Revert(decimal snapshot)
        {
            return snapshot - this.ToAdd;
        }

        public bool Equals(IEvent<int, decimal> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is not AddEvent otherEvent)
            {
                return false;
            }

            if (otherEvent.Timestamp != this.Timestamp)
            {
                return false;
            }

            if (otherEvent.ToAdd != this.ToAdd)
            {
                return false;
            }

            return true;
        }
    }
}