namespace TrafficManager.Domain.Reference
{
    public enum BulbStateEnum
    {
        Unknown = 0,
        On = 1110,
        Off = 1120,
        Transitioning = 5000,
        AssumedOff = 1130,
        AssumedOn = 1140,
        InOperable = 9999
    }
}