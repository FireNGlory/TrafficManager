using System.Threading.Tasks;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface ICurrentSensor : IDomainDevice
    {
        Task<CurrentSensorStateEnum> GetState();

        void MarkInOp(bool broken);
    }
}
