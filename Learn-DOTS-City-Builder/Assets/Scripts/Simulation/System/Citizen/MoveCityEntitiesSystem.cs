using quentin.tran.authoring;
using quentin.tran.common;
using quentin.tran.simulation.grid;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Move City Entities : move cars and inhabitants if they have a target
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(CitizenPathFindingSystem))]

    partial struct MoveCityEntitiesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManager>();
            state.RequireForUpdate<HasPathFindingPath>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            TimeManager time = SystemAPI.GetSingleton<TimeManager>();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter cmd = ecb.AsParallelWriter();

            MoveEntitiesJob moveJob = new MoveEntitiesJob() { deltaTime = SystemAPI.Time.DeltaTime, cmd = cmd, timeScale = time.timeScale };
            moveJob.ScheduleParallel(state.Dependency).Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct MoveEntitiesJob : IJobEntity
    {
        private const float SPEED = .25f;

        [ReadOnly]
        public float deltaTime;

        [ReadOnly]
        public float timeScale;

        [WriteOnly]
        public EntityCommandBuffer.ParallelWriter cmd;

        [BurstCompile]
        public void Execute(ref LocalTransform transform, DynamicBuffer<Waypoint> waypoints, ref HasPathFindingPath path, Entity e, [ChunkIndexInQuery] int sortKey)
        {
            if (waypoints.Length == 0)
            {
                cmd.RemoveComponent<HasPathFindingPath>(sortKey, e); ;
                return;
            }

            float distanceThatCanBeTravelled = this.deltaTime * SPEED * this.timeScale;
            int i = 0;

            while (distanceThatCanBeTravelled >= 0)
            {
                int2 targetIndex = waypoints[^(path.currentWaypointIndex + 1)].cellIndex;
                float3 target = (new float3(targetIndex.x, 0, targetIndex.y) + new float3(0.5f, 0, 0.5f)) * GridProperties.GRID_CELL_SIZE;

                float3 direction = target - transform.Position;
                float speed = math.clamp(distanceThatCanBeTravelled, 0, math.length(direction));
                direction = math.normalizesafe(direction);

                transform = transform.Translate(direction * speed);
                distanceThatCanBeTravelled -= speed;

                if (math.lengthsq(transform.Position - target) < .1)
                {
                    path.currentWaypointIndex++;

                    if (path.currentWaypointIndex >= waypoints.Length)
                    {
                        cmd.RemoveComponent<HasPathFindingPath>(sortKey, e);
                        return;
                    }
                }

                if (i > 10) // security to prevent over simulation
                    return;

                i++;
            }
        }
    }
}