namespace TrafficManager.Domain.Reference
{
    public enum EventStreamEnum
    {
        None = 0,
        Directory = 20010,
        StateChange = 20020,
        Anomaly = 20030,
        Summary = 20040,
        Usage = 20050,
        RoWChange = 20060,
        Log = 20070
    }
}