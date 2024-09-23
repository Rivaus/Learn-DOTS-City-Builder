using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.component
{
    public struct CitizenJob : IComponentData
    {
        public int salaryPerDay;

        public int startHour, endHour;

        public int2 officeBuildingIndex;
    }
}