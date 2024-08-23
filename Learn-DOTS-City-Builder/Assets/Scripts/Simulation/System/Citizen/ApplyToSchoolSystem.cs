using Unity.Burst;
using Unity.Entities;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Apply To School System : kids from 3 to 24 year old can apply to school
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(ApplyToJobSystem))]
    partial struct ApplyToSchoolSystem : ISystem
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

