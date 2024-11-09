using Castrimaris.Player;

namespace Castrimaris.Vehicles {
    public interface IVehicle {
        public void AttachEntity(IEntity Entity);
        public void DetachEntity(IEntity Entity);
    }
}
