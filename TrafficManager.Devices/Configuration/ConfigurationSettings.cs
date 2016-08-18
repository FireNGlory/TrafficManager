using System;

namespace TrafficManager.Devices.Configuration
{
    public class ConfigurationSettings
    {
        public string AzureIoTHubUri { get; set; }
        public string AzureIoTDeviceId { get; set; }
        public string AzureIoTDeviceKey { get; set; }


        public Guid IntersectionId { get; set; }

        public Guid NorthSouthRouteId { get; set; }
        public Guid NorthLampSetId { get; set; }
        public Guid NorthLampId { get; set; }
        public Guid NorthRedBulbId { get; set; }
        public Guid NorthRedBulbSensorId { get; set; }
        public int NorthRedBulbPinId { get; set; }
        public Guid NorthYellowBulbId { get; set; }
        public Guid NorthYellowBulbSensorId { get; set; }
        public int NorthYellowBulbPinId { get; set; }
        public Guid NorthGreenBulbId { get; set; }
        public Guid NorthGreenBulbSensorId { get; set; }
        public int NorthGreenBulbPinId { get; set; }

        public Guid SouthLampSetId { get; set; }
        public Guid SouthLampId { get; set; }
        public Guid SouthRedBulbId { get; set; }
        public Guid SouthRedBulbSensorId { get; set; }
        public int SouthRedBulbPinId { get; set; }
        public Guid SouthYellowBulbId { get; set; }
        public Guid SouthYellowBulbSensorId { get; set; }
        public int SouthYellowBulbPinId { get; set; }
        public Guid SouthGreenBulbId { get; set; }
        public Guid SouthGreenBulbSensorId { get; set; }
        public int SouthGreenBulbPinId { get; set; }

        public Guid EastWestRouteId { get; set; }

        public Guid EastLampSetId { get; set; }
        public Guid EastLampId { get; set; }
        public Guid EastRedBulbId { get; set; }
        public Guid EastRedBulbSensorId { get; set; }
        public int EastRedBulbPinId { get; set; }
        public Guid EastYellowBulbId { get; set; }
        public Guid EastYellowBulbSensorId { get; set; }
        public int EastYellowBulbPinId { get; set; }
        public Guid EastGreenBulbId { get; set; }
        public Guid EastGreenBulbSensorId { get; set; }
        public int EastGreenBulbPinId { get; set; }

        public Guid WestLampSetId { get; set; }
        public Guid WestLampId { get; set; }
        public Guid WestRedBulbId { get; set; }
        public Guid WestRedBulbSensorId { get; set; }
        public int WestRedBulbPinId { get; set; }
        public Guid WestYellowBulbId { get; set; }
        public Guid WestYellowBulbSensorId { get; set; }
        public int WestYellowBulbPinId { get; set; }
        public Guid WestGreenBulbId { get; set; }
        public Guid WestGreenBulbSensorId { get; set; }
        public int WestGreenBulbPinId { get; set; }

    }
}