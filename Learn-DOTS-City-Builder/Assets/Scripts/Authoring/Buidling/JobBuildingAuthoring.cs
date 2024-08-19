using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.authoring.building
{
    public class JobBuildingAuthoring : MonoBehaviour
    {
        public int nbJob;

        public int startHour, endHour;

        public Vector2 salaryRangePerDay;

        private class Baker : Baker<JobBuildingAuthoring>
        {
            public override void Bake(JobBuildingAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new OfficeBuilding()
                {
                    nbJob = authoring.nbJob,
                    numberOfFreeJob = 0,
                    startHour = authoring.startHour,
                    endHour = authoring.endHour,
                    salaryRangePerDay = new float2(authoring.salaryRangePerDay),
                    officeBuilding = e
                });
            }
        }
    }

    public struct OfficeBuilding : IComponentData
    {
        public int nbJob;

        public int numberOfFreeJob;

        public int startHour, endHour;

        public float2 salaryRangePerDay;

        public Entity officeBuilding;
    }
}

