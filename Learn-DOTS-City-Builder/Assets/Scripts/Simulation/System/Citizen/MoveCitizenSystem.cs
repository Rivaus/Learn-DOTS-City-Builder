using quentin.tran.authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// A system reponsible to move all city entities (citizens, cars)
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(SetCitizenTargetSystem))]
    partial struct MoveCityEntitySystem : ISystem
    {
        public const float CITIZEN_SPEED = .3f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManager>();
            state.RequireForUpdate<Target>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter cmd = ecb.AsParallelWriter();

            MoveCityEntitiesJob job = new MoveCityEntitiesJob()
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                timeScale = SystemAPI.GetSingleton<TimeManager>().timeScale,
                cmd = cmd,
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct MoveCityEntitiesJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;

        [ReadOnly] public float timeScale;

        public EntityCommandBuffer.ParallelWriter cmd;

        [BurstCompile]
        public void Execute(ref LocalTransform transform, in Target target, Entity e, [ChunkIndexInQuery] int sortKey)
        {
            /*float3 direction = target.target - transform.Position;
            float3 movement = math.normalizesafe(direction) * MoveCityEntitySystem.CITIZEN_SPEED * deltaTime * timeScale;

            float distanceToTarget = math.length(direction);

            if (math.lengthsq(movement) > math.lengthsq(direction))
                movement = math.normalize(movement) * distanceToTarget;

            transform.Position += movement;

            if (math.distancesq(transform.Position, target.target) < .1 *.1)
            {
                this.cmd.RemoveComponent<Target>(sortKey, e);
            }*/
        }
    }
}

