using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    public class JobBuildingPrefabsAuthoring : MonoBehaviour
    {
        public List<GameObject> simpleOffice01Prefabs = new();

        private class Baker : Baker<JobBuildingPrefabsAuthoring>
        {
            public override void Bake(JobBuildingPrefabsAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent<ShopBuildingPrefabs>(e);

                DynamicBuffer<LowDensityShopCollection> lowDensityShops = AddBuffer<LowDensityShopCollection>(e);

                for (int i = 0; i < authoring.simpleOffice01Prefabs.Count; i++)
                    lowDensityShops.Add(new() { entity = GetEntity(authoring.simpleOffice01Prefabs[i], TransformUsageFlags.Renderable) });
            }
        }
    }

    public struct ShopBuildingPrefabs : IComponentData
    {
    }

    public struct LowDensityShopCollection : IBufferElementData
    {
        public Entity entity;
    }
}

