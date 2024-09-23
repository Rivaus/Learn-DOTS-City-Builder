using quentin.tran.simulation.component;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.building
{
    public class BuildingAuthoring : MonoBehaviour
    {
        /// <summary>
        /// Max number of residents per house.
        /// </summary>
        public int capacity = 0;

        /// <summary>
        /// Number of homes inside the building.
        /// </summary>
        public int nbOfHouses = 1;

        private class Baker : Baker<BuildingAuthoring>
        {
            public override void Bake(BuildingAuthoring authoring)
            {
                Entity house = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(house, new HouseBuilding()
                {
                    nbOfHouses = authoring.nbOfHouses,
                    houseCapacity = authoring.capacity
                });
                AddBuffer<LinkedEntityBuffer>(house); // store all House Component related to.
            }
        }
    }
}

