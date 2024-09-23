using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.component
{
    public struct PathFindingRequest : IComponentData
    {
        /// <summary>
        /// Road next to target
        /// </summary>
        public int2 roadTarget;

        /// <summary>
        /// Target
        /// </summary>
        public int2 target;
    }

    public struct HasPathFindingPath : IComponentData
    {
        public int currentWaypointIndex;
    }
}