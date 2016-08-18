using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface IBulb : IDomainDevice
    {
        event StateChangedEvent StateChanged;
        event BulbCycledEvent BulbCycled;
        
        BulbTypeEnum BulbType { get; }
        ICurrentSensor MyCurrentSensor { get; }

        Task<BulbStateEnum> GetState();
        Task<bool> TransitionToState(BulbStateEnum state);

        void MarkInOp(bool broken);
    }
}