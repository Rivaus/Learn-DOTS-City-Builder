using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.component
{
    public struct PathFindingRequest : IComponentData, IEnableableComponent
    {
        /// <summary>
        /// Target
        /// </summary>
        public float3 target;
    }

    public struct HasPathFindingPath : IComponentData, IEnableableComponent
    {
        public int currentWaypointIndex;
    }

    public struct Waypoint : IBufferElementData
    {
        public float3 position;
    }
}