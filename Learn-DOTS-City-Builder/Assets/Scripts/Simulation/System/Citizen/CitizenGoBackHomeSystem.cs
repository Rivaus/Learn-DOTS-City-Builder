using Unity.Burst;
using Unity.Entities;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Go Back Home System : in the afternoon, make citizens go back to their home
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(CitizenGoToDailyLifeSystem))]
    partial struct CitizenGoBackHomeSystem : ISystem
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
