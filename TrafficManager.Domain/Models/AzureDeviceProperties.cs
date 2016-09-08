namespace TrafficManager.Domain.Models
{
    public class AzureDeviceProperties
    {
        public string DeviceID { get; set; }
        public bool HubStateEnabled { get; set; }
        public string DeviceState { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string Platform { get; set; }
        public string Processor { get; set; }
        public string InstalledRam { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}