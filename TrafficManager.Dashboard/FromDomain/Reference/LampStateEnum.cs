namespace TrafficManager.Domain.Reference
{
    public enum LampStateEnum
    {
        Unknown = 0,
        Go = 1210,
        Caution = 1220,
        Stop = 1230,
        Transitioning = 5000,
        CriticalMalfunction = 1240,
        InOperable = 9999
    }
}