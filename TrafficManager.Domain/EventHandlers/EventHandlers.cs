using TrafficManager.Domain.Reference.Args;

namespace TrafficManager.Domain.EventHandlers
{
    public delegate void BulbCycledEvent(object sender, BulbCycledEventArgs args);
    public delegate void StateChangedEvent(object sender, StateChangedEventArgs args);
    public delegate void InternalAnomalyEvent(object sender, InternalAnomalyEventArgs args);
    public delegate void RightOfWayChangedEvent(object sender, RightOfWayChangedEventArgs args);

    public delegate void CommandReceivedEvent(object sender, CommandReceivedEventArgs args);
}
