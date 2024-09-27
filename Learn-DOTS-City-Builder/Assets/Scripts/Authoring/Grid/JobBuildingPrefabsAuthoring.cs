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
                AddComponent(e, new JobBuildingPrefabs()
                {
                    simpleOffice01 = GetEntity(authoring.simpleOffice01Prefabs[0], TransformUsageFlags.Dynamic),
                    simpleOffice02 = GetEntity(authoring.simpleOffice01Prefabs[1], TransformUsageFlags.Dynamic),
                    simpleOffice03 = GetEntity(authoring.simpleOffice01Prefabs[2], TransformUsageFlags.Dynamic),
                    simpleOffice04 = GetEntity(authoring.simpleOffice01Prefabs[3], TransformUsageFlags.Dynamic),
                    simpleOffice05 = GetEntity(authoring.simpleOffice01Prefabs[4], TransformUsageFlags.Dynamic),
                    simpleOffice06 = GetEntity(authoring.simpleOffice01Prefabs[5], TransformUsageFlags.Dynamic),
                    restaurant01 = GetEntity(authoring.simpleOffice01Prefabs[6], TransformUsageFlags.Dynamic),
                    restaurant02 = GetEntity(authoring.simpleOffice01Prefabs[7], TransformUsageFlags.Dynamic),
                    restaurant03 = GetEntity(authoring.simpleOffice01Prefabs[8], TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct JobBuildingPrefabs : IComponentData
    {
        public Entity simpleOffice01;
        public Entity simpleOffice02;
        public Entity simpleOffice03;
        public Entity simpleOffice04;
        public Entity simpleOffice05;
        public Entity simpleOffice06;
        public Entity restaurant01;
        public Entity restaurant02;
        public Entity restaurant03;
    }
}

