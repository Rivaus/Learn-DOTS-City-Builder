using quentin.tran.authoring.citizen;
using quentin.tran.authoring;
using Unity.Burst;
using Unity.Entities;
using quentin.tran.authoring.building;
using Unity.Mathematics;
using Unity.Collections;
using quentin.tran.simulation.grid;
using quentin.tran.simulation.system.grid;
using quentin.tran.models.grid;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Unity.Jobs;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// System to set citizen destinations.
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    public partial struct SetCitizenTargetSystem : ISystem
    {
        private EntityQuery buildingQuery;

        private Random random;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManager>();
            state.RequireForUpdate<Citizen>();

            this.buildingQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly(typeof(Building)), ComponentType.ReadOnly(typeof(GridCellComponent)));
            this.random = Random.CreateFromIndex(123);
        }

        public void OnUpdate(ref SystemState state)
        {
            using NativeArray<GridCellComponent> buildings = this.buildingQuery.ToComponentDataArray<GridCellComponent>(Allocator.Temp);

            EntityCommandBuffer cmd = new(Allocator.Temp);

            NativeList<JobHandle> handles = new(Allocator.TempJob);

            foreach ((RefRO<Citizen> citizen, var entity) in SystemAPI.Query<RefRO<Citizen>>().WithAbsent<Target>().WithEntityAccess())
            {
                // 1. Find citizen building
                if (citizen.ValueRO.house == Entity.Null)
                    continue;

                RefRO<House> house = SystemAPI.GetComponentRO<House>(citizen.ValueRO.house);

                if (!house.IsValid)
                    continue;

                RefRO<GridCellComponent> cell = SystemAPI.GetComponentRO<GridCellComponent>(house.ValueRO.building);

                if (!cell.IsValid)
                    continue;

                List<GridCellModel> roadNeighbours = new();
                GridUtils.GetNeighboursOfType(cell.ValueRO.index.x, cell.ValueRO.index.y, GridCellType.Road, roadNeighbours);
                Debug.Assert(roadNeighbours.Count > 0);

                int2 start = roadNeighbours[0].Index;

                //2. Found a random target road;
                GridCellComponent randomBuilding = buildings[random.NextInt(0, buildings.Length)];
                roadNeighbours.Clear();
                GridUtils.GetNeighboursOfType(randomBuilding.index.x, randomBuilding.index.y, GridCellType.Road, roadNeighbours);
                Debug.Assert(roadNeighbours.Count > 0);

                int2 end = roadNeighbours[0].Index;

                //2. Let's do pathfinding
                DynamicBuffer<Waypoint> waypoints = cmd.AddBuffer<Waypoint>(entity);
                //FindPath(start, end, waypoints).Complete();
                cmd.AddComponent<Target>(entity);
            }

            handles.Add(state.Dependency);

            state.Dependency = JobHandle.CombineDependencies(handles.AsArray());
            handles.Dispose();

            cmd.Playback(state.EntityManager);
            cmd.Dispose();
        }

        private JobHandle FindPath(int2 startIndex, int2 endIndex, DynamicBuffer<Waypoint> waypoints)
        {
            RoadGridManager manager = RoadGridManager.Instance;

            NativeArray<PathFindingNode> nodes = new(manager.GetGraphSize(), Allocator.TempJob);
            NativeArray<RoadCell>.ReadOnly roadsArray = manager.RoadGridArray;

            manager.GetGraphNode(nodes);

            RoadPathfinderJob job = new()
            {
                startIndex = startIndex,
                endIndex = endIndex,
                roads = roadsArray,
                gridWidth = manager.GetGridSize().x,
                directions = manager.MovementDirections,
                nodes = nodes,
                result = waypoints
            };

            JobHandle handle = job.Schedule();
            handle.Complete();

            return default;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            this.buildingQuery.Dispose();
        }
    }

    public struct Target : IComponentData
    {
    }
}
