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
                    nbJobs = authoring.nbJob,
                    nbOfAvailableJob = authoring.nbJob,
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
        /// <summary>
        /// Total of jobs (available + employed)
        /// </summary>
        public int nbJobs;

        public int nbOfAvailableJob;

        public int startHour, endHour;

        public float2 salaryRangePerDay;

        public Entity officeBuilding;
    }
}

