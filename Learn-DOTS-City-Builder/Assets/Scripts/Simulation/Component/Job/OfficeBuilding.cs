using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.component
{
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