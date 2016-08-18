namespace TrafficManager.Domain.Reference
{
    public enum SystemCommandEnum
    {
        None = 0,
        BringOnline = 10010,
        RequestStatus = 10020,
        UpdateRoutePreference = 10030,
        ReplaceBulb = 10040,
        ReplaceSensor = 10045,
        SimulateBulbFailure = 10810,
        SimulateSensorFailure = 10820,
        TakeOffline = 10999,
        Shutdown = 19999
    }
}