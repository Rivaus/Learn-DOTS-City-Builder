using Unity.Entities;

namespace quentin.tran.simulation.component
{
    public struct House : IComponentData
    {
        public Entity building;

        public int capacity;

        public int nbOfResidents;
    }
}
