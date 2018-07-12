using System;
using System.Threading.Tasks;

namespace TrafficManager.Devices.Configuration
{
    public class InMemoryConfigService : IConfigService
    {
        private ConfigurationSettings _myConfig;

        public InMemoryConfigService()
        {
            //Default Configuration
            _myConfig = new ConfigurationSettings
            {
                AzureIoTHubUri = "TrafficManager.azure-devices.net",
                AzureIoTDeviceId = "PeiceOfPiDevice",
                AzureIoTDeviceKey = "aTcFbRvVs5CoyOHNQbucVUbNwjJO0zQVH5TuI1pYCaM=", // "HSL5jVUOhrLwd0Mf0fCdqA==",

                IntersectionId = new Guid("f6f9747d-f68f-4e1c-a4b9-d9e85c41ba97"),

                EastLampId = new Guid("5555111f-d639-4b35-8de9-14b58b3e420c"),
                EastLampSetId = new Guid("5581584e-3dae-4aa6-b690-f9d43bbf2b08"),
                EastRedBulbId = new Guid("3e48ed9c-813d-4cef-93d2-216f4d32fd1d"),
                EastRedBulbPinId = 23,
                EastRedBulbSensorId = new Guid("735752fa-2928-405b-b50f-995231d3b792"),
                EastYellowBulbId = new Guid("0bd1fbe0-ded5-410d-8cb5-887111c01630"),
                EastYellowBulbPinId = 24,
                EastYellowBulbSensorId = new Guid("029ae0ae-2171-4362-91a3-a814765ee8a4"),
                EastGreenBulbId = new Guid("c5ef56ca-a594-41bc-8c24-2655944a752b"),
                EastGreenBulbPinId = 25,
                EastGreenBulbSensorId = new Guid("0035ee67-d89f-4a9e-be41-91bdac838a78"),

                EastWestRouteId = new Guid("bfbe2082-4be5-427b-9f2c-fb5e4692ceec"),
                NorthSouthRouteId = new Guid("47365541-2882-4814-b5f1-94361c09d58c"),
                NorthLampId = new Guid("d22c0c30-35a3-439d-b505-2b395b87104e"),
                NorthLampSetId = new Guid("0fc50ef3-ad24-4d63-b152-b712903e8607"),
                NorthRedBulbId = new Guid("bdd4c02b-11d3-4ed5-8d34-6ef2e85c804b"),
                NorthRedBulbPinId = 13,
                NorthRedBulbSensorId = new Guid("26ebd133-7fa6-46b0-8a7f-5734622cc70c"),
                NorthYellowBulbId = new Guid("01478f65-93c7-46d9-be64-a55f383296f9"),
                NorthYellowBulbPinId = 19,
                NorthYellowBulbSensorId = new Guid("b2f1e17d-52e7-480d-862a-639e98066f29"),
                NorthGreenBulbId = new Guid("8b6bd075-7576-43a2-adc8-4ec36f914297"),
                NorthGreenBulbPinId = 26,
                NorthGreenBulbSensorId = new Guid("145b1009-099c-4456-93d8-34f4eb670ebb"),

                SouthLampId = new Guid("a2742869-6852-4e03-8c1a-32673ea1c13f"),
                SouthLampSetId = new Guid("edf14820-c265-44af-a8b3-b00be4ea4106"),
                SouthRedBulbId = new Guid("16a97bd3-0661-47d4-a314-e7ab62162fca"),
                SouthRedBulbPinId = 16,
                SouthRedBulbSensorId = new Guid("3b47fc0c-9d01-4cdb-9162-8cf4508f3909"),
                SouthYellowBulbId = new Guid("a8fbf9ca-aab6-4f84-95f4-f8c1134cacb5"),
                SouthYellowBulbPinId = 20,
                SouthYellowBulbSensorId = new Guid("352243bb-837d-41f0-be4f-0e210d4ce4a5"),
                SouthGreenBulbId = new Guid("249f351b-b43d-4864-bad7-d9f19cd7c161"),
                SouthGreenBulbPinId = 21,
                SouthGreenBulbSensorId = new Guid("6191baaf-bb76-4664-b293-1040b223151c"),

                WestLampId = new Guid("3c8c9db3-cdc6-4a76-8b0a-06ba80a03932"),
                WestLampSetId = new Guid("be894b4e-454b-4a2e-b729-8d2c422e9dff"),
                WestRedBulbId = new Guid("dcce1d40-45c3-4edf-9023-73b5d9261886"),
                WestRedBulbPinId = 4,
                WestRedBulbSensorId = new Guid("13dad334-0db5-4f55-94a0-4e34c20859f1"),
                WestYellowBulbId = new Guid("1b743ee7-d375-44a7-924f-9b7b0e070a95"),
                WestYellowBulbPinId = 5,
                WestYellowBulbSensorId = new Guid("33db4064-522f-4068-bd4e-e4d929e48016"),
                WestGreenBulbId = new Guid("03d9e044-6aee-46df-994b-0fe3fe5fefc5"),
                WestGreenBulbPinId = 6,
                WestGreenBulbSensorId = new Guid("00f26339-b3de-4630-bca2-4b9f24cc132c")
            };

        }
        public Task<ConfigurationSettings> ReadConfig()
        {
            return Task.FromResult(_myConfig);
        }

        public Task WriteConfig(ConfigurationSettings updatedConfig)
        {
            return Task.FromResult(_myConfig = updatedConfig);
        }
    }
}
