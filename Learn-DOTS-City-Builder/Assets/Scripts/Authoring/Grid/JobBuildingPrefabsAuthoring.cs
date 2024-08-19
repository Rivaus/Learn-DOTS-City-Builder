using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    public class JobBuildingPrefabsAuthoring : MonoBehaviour
    {
        public GameObject simpleOffice01 = null;

        private class Baker : Baker<JobBuildingPrefabsAuthoring>
        {
            public override void Bake(JobBuildingPrefabsAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new JobBuildingPrefabs()
                {
                    simpleOffice01 = GetEntity(authoring.simpleOffice01, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct JobBuildingPrefabs : IComponentData
    {
        public Entity simpleOffice01;
    }
}

