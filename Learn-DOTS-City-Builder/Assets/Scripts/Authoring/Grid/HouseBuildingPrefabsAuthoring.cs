using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    /// <summary>
    /// Creates an entity which has all roads prefabs.
    /// </summary>
    public class HouseBuildingPrefabsAuthoring : MonoBehaviour
    {
        public List<GameObject> simpleHouse01Prefabs = new();

        private class Baker : Baker<HouseBuildingPrefabsAuthoring>
        {
            public override void Bake(HouseBuildingPrefabsAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);

                AddComponent<HouseBuildingPrefabs>(e);

                DynamicBuffer<LowDensityHouseCollection> lowDensityHouses = AddBuffer<LowDensityHouseCollection>(e);

                for (int i = 0; i < authoring.simpleHouse01Prefabs.Count; i++)
                    lowDensityHouses.Add(new() { entity = GetEntity(authoring.simpleHouse01Prefabs[i], TransformUsageFlags.Renderable) });
            }
        }
    }

    public struct HouseBuildingPrefabs : IComponentData
    {
    }

    public struct LowDensityHouseCollection : IBufferElementData
    {
        public Entity entity;
    }
}