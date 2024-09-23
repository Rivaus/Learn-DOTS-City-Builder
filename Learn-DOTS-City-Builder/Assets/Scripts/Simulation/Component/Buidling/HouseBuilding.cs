using Unity.Entities;

namespace quentin.tran.simulation.component
{
    public struct HouseBuilding : IComponentData
    {
        public int nbOfHouses;

        public int houseCapacity;
    }
}

