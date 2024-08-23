using Unity.Burst;
using Unity.Entities;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Move City Entities : move cars and inhabitants if they have a target
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(CitizenGoBackHomeSystem))]

    partial struct MoveCityEntitiesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}