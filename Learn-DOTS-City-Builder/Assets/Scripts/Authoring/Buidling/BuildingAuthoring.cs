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
                AddComponent(house, new Building()
                {
                    nbOfHouses = authoring.nbOfHouses,
                    houseCapacity = authoring.capacity
                });
            }
        }
    }

    public struct Building : IComponentData
    {
        public int nbOfHouses;

        public int houseCapacity;
    }

    public struct House : IComponentData
    {
        public Entity building;

        public int capacity;

        public int nbOfResidents;
    }
}

