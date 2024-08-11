using quentin.tran.authoring.citizen;
using quentin.tran.authoring;
using Unity.Burst;
using Unity.Entities;
using quentin.tran.authoring.building;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// System to set citizen destinations.
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(SpawnCitizenSystem))]
    public partial struct SetCitizenTargetSystem : ISystem
    {
        private EntityQuery buildingQuery;

        private Random random;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManager>();
            state.RequireForUpdate<Citizen>();

            this.buildingQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly(typeof(Building)), ComponentType.ReadOnly(typeof(LocalTransform)));
            this.random = Random.CreateFromIndex(123);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using NativeArray<LocalTransform> buildings = this.buildingQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            EntityCommandBuffer cmd = new(Allocator.Temp);

            foreach ((var citizen, var entity) in SystemAPI.Query<Citizen>().WithAbsent<Target>().WithEntityAccess())
            {
                LocalTransform randomBuildingTransform = buildings[this.random.NextInt(0, buildings.Length)];
                cmd.AddComponent(entity, new Target { target = randomBuildingTransform.Position });
            }

            cmd.Playback(state.EntityManager);
            cmd.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.buildingQuery.Dispose();
        }
    }

    public struct Target : IComponentData
    {
        public float3 target;
    }
}
