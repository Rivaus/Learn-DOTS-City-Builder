using Unity.Burst;
using Unity.Entities;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Go To Entertainment System : at evening or weekends, inhabitants can go to the theatre, concert, etc if they are close enough to.
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(CitizenGoBackHomeSystem))]
    partial struct CitizenGoToEntertainmentSystem : ISystem
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