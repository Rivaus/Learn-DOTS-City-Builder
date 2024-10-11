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
                AddComponent(e, new ShopBuildingPrefabs()
                {
                    restaurant01 = GetEntity(authoring.simpleOffice01Prefabs[0], TransformUsageFlags.Dynamic),
                    restaurant02 = GetEntity(authoring.simpleOffice01Prefabs[1], TransformUsageFlags.Dynamic),
                    restaurant03 = GetEntity(authoring.simpleOffice01Prefabs[2], TransformUsageFlags.Dynamic),
                    floristAndBakery1 = GetEntity(authoring.simpleOffice01Prefabs[3], TransformUsageFlags.Dynamic),
                    floristAndBakery2 = GetEntity(authoring.simpleOffice01Prefabs[4], TransformUsageFlags.Dynamic),
                    floristAndBakery3 = GetEntity(authoring.simpleOffice01Prefabs[5], TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct ShopBuildingPrefabs : IComponentData
    {
        public Entity restaurant01;
        public Entity restaurant02;
        public Entity restaurant03;
        public Entity floristAndBakery1;
        public Entity floristAndBakery2;
        public Entity floristAndBakery3;
    }
}

