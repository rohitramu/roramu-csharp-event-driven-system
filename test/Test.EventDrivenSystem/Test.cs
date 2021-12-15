using FluentAssertions;
using Xunit;

namespace RoRamu.EventDrivenSystem.Test
{
    public class Test
    {
        [Fact]
        public void EndToEnd()
        {
            IEventDrivenSystem<int, decimal> system = new EventDrivenSystem<int, decimal>(0, 0, x => x);

            system.AddEvent(new AddEvent(10, 20)).Should().BeTrue("this is a new (distinct) event");
            system.CurrentState.Should().Be(20, "we just did '0 + 20'");

            system.AddEvent(new MultiplyEvent(20, 5)).Should().BeTrue("this is a new (distinct) event");
            system.CurrentState.Should().Be(100, "we just did '20 * 5'");

            system.AddEvent(new DivideEvent(30, 2)).Should().BeTrue("this is a new (distinct) event");
            system.CurrentState.Should().Be(50, "we just did '100 / 2'");

            system.AddEvent(new SubtractEvent(20, 10)).Should().BeTrue("this is a new (distinct) event");
            system.CurrentState.Should().Be(45, "we just changed the sequence of events to '((0 + 20) * 5 - 10) / 2'");

            system.AddEvent(new SetValueEvent(25, 60)).Should().BeTrue("this is a new (distinct) event");
            system.CurrentState.Should().Be(30, "we just changed the sequence of events to '60 / 2'");

            system.AddEvent(new AddEvent(10, 20)).Should().BeFalse("the duplicate event should not be added");
            system.CurrentState.Should().Be(30, "we attempted to add a duplicate event");

            system.GetEvents().Should().HaveCount(6, "we have added 5 distinct values, and we also have to include the initial state");

            system.GetStateAtTimestamp(15).Should().Be(20, "after the first operation at time 10, the events are equivalent to '0 + 20'");
            system.GetStateAtTimestamp(0).Should().Be(0, "the initial state is 0");
            system.GetStateAtTimestamp(30).Should().Be(30, "this is the final state before removing events");

            system.RemoveEvent(new MultiplyEvent(20, 5)).Should().BeTrue("this event was previously added");
            system.CurrentState.Should().Be(30, "we just removed an event before the 'SetValue(60)' event, so the effective operations are '60 / 2'");

            system.RemoveEvent(new DivideEvent(30, 2)).Should().BeTrue("this event was previously added");
            system.CurrentState.Should().Be(60, "we just changed the effective sequence of events to '60'");

            system.RemoveEvent(new DivideEvent(30, 2)).Should().BeFalse("this event was already removed");
            system.CurrentState.Should().Be(60, "we just changed the sequence of events to '0 + 20 - 10'");

            system.AddEvent(new DivideEvent(30, 2)).Should().BeTrue("this is a new (distinct) event");
            system.CurrentState.Should().Be(30, "we just did '60 / 2'");

            system.RemoveEvent(new SetValueEvent(25, 60)).Should().BeTrue("we previously added this event");
            system.CurrentState.Should().Be(5, "we just changed the effective sequence of events to '((0 + 20 - 10) / 2'");
        }
    }
}