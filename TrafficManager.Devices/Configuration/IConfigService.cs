using System.Threading.Tasks;

namespace TrafficManager.Devices.Configuration
{
    public interface IConfigService
    {
        Task<ConfigurationSettings> ReadConfig();
        Task WriteConfig(ConfigurationSettings updatedConfig);
    }
}