using quentin.tran.common;
using quentin.tran.models.grid;
using quentin.tran.simulation.component;
using quentin.tran.simulation.grid;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Citizen Path Finder System : compute path from pathfinding requests added from (go to daily life and go back home system)
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(CitizenGoBackHomeSystem))]
    partial struct CitizenPathFindingSystem : ISystem
    {
        private BufferLookup<Waypoint> waypoints;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PathFindingRequest>();
            state.RequireForUpdate<Citizen>();

            waypoints = state.GetBufferLookup<Waypoint>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            waypoints.Update(ref state);

            EntityCommandBuffer cmd = new(Allocator.TempJob);
            NativeList<JobHandle> jobs = new(Allocator.TempJob);

            foreach ((var pathFindingRequest, var transform, var waypoints, Entity e) in
                SystemAPI.Query<RefRO<PathFindingRequest>, RefRO<LocalTransform>, DynamicBuffer<Waypoint>>()
                .WithDisabled<HasPathFindingPath>()
                .WithEntityAccess())
            {
                waypoints.Clear();

                cmd.SetComponentEnabled<PathFindingRequest>(e, false);
                cmd.SetComponent(e, new HasPathFindingPath() { currentWaypointIndex = 0 });
                cmd.SetComponentEnabled<HasPathFindingPath>(e, true);

                PathFindingJob pathFinder = new()
                {
                    startPosition = transform.ValueRO.Position,
                    endPosition = pathFindingRequest.ValueRO.target,
                    baseNodes = WalkingNetworkManager.Instance.Nodes,
                    result = waypoints
                };

                jobs.Add(pathFinder.Schedule());
            }

            NativeArray<JobHandle> jobsArray = jobs.AsArray();
            JobHandle.CompleteAll(jobsArray);
            jobsArray.Dispose(); // required ?
            jobs.Dispose();

            cmd.Playback(state.EntityManager);
            cmd.Dispose();
        }

    }
}