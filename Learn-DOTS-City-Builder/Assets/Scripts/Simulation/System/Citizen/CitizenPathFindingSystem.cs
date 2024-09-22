using quentin.tran.authoring.citizen;
using quentin.tran.common;
using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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

            RoadGridManager manager = RoadGridManager.Instance;

            EntityCommandBuffer cmd = new(Allocator.TempJob);

            NativeArray<PathFindingNode> nodes = new(manager.GetGraphSize(), Allocator.TempJob);
            manager.GetGraphNode(nodes);
            NativeArray<RoadCell>.ReadOnly roadsArray = manager.RoadGridArray;
            NativeList<JobHandle> jobs = new(Allocator.TempJob);

            foreach ((var pathFindingRequest, var transform, var waypoints, Entity e) in
                SystemAPI.Query<RefRO<PathFindingRequest>, RefRO<LocalTransform>, DynamicBuffer<Waypoint>>()
                .WithAbsent<HasPathFindingPath>()
                .WithEntityAccess())
            {
                waypoints.Clear();

                cmd.RemoveComponent<PathFindingRequest>(e);
                cmd.AddComponent<HasPathFindingPath>(e);

                int2 start = FindStartRoad(transform, roadsArray, manager.MovementDirections);

                RoadPathFindingJob pathFinder = new()
                {
                    startIndex = start,
                    endIndex = pathFindingRequest.ValueRO.roadTarget,
                    roads = roadsArray,
                    gridWidth = manager.GetGridSize().x,
                    directions = manager.MovementDirections,
                    baseNodes = nodes,
                    result = waypoints,
                    addExtraWaypoint = true,
                    extraWaypoint = pathFindingRequest.ValueRO.target
                };
                
                jobs.Add(pathFinder.Schedule());
            }

            NativeArray<JobHandle> jobsArray = jobs.AsArray();
            JobHandle.CompleteAll(jobsArray);
            jobsArray.Dispose(); // required ?
            jobs.Dispose();
            nodes.Dispose();

            cmd.Playback(state.EntityManager);
            cmd.Dispose();
        }

        /// <summary>
        /// Find closest road tile next to <paramref name="transform"/>.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="roads"></param>
        /// <param name="directions"></param>
        /// <returns></returns>
        [BurstCompile]
        private int2 FindStartRoad(RefRO<LocalTransform> transform, NativeArray<RoadCell>.ReadOnly roads, NativeArray<int2>.ReadOnly directions)
        {
            int2 start = new int2((int)(transform.ValueRO.Position.x / GridProperties.GRID_CELL_SIZE), (int)(transform.ValueRO.Position.z / GridProperties.GRID_CELL_SIZE));
            int tmp;

            int res = -1;

            foreach (int2 direction in directions)
            {
                tmp = IndexToArrayIndex(start + direction);

                if (tmp < roads.Length && roads[tmp].Type != RoadType.None)
                {
                    res = tmp;

                    break;
                }
            }

            if (res >= 0)
                return ArrayIndexToIndex(res);
            else
                return int2.zero;
        }

        [BurstCompile]
        private int IndexToArrayIndex(int2 index) => index.x + index.y * GridProperties.GRID_SIZE;

        [BurstCompile]
        private int2 ArrayIndexToIndex(int index)
        {
            int y = index / GridProperties.GRID_SIZE;
            int x = index - (y * GridProperties.GRID_SIZE);

            return new int2(x, y);
        }
    }

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