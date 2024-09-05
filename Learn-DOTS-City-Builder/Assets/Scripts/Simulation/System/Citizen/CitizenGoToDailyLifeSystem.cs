using quentin.tran.authoring;
using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using quentin.tran.common;
using quentin.tran.simulation.grid;
using quentin.tran.simulation.system.grid;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Citizen Go To Job DailyLife System : in the morning, set employed citizens their office as a target. In the evening, make them go back home.
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(ApplyToJobSystem))]
    partial struct CitizenGoToJobDailyLifeSystem : ISystem
    {
        NativeArray<int2> directions;

        private ComponentLookup<House> housesLookup;
        private ComponentLookup<GridCellComponent> gridLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CitizenJob>();
            state.RequireForUpdate<TimeManager>();

            directions = new(4, Allocator.Persistent);
            directions[0] = new int2(0, 1);
            directions[1] = new int2(1, 0);
            directions[2] = new int2(0, -1);
            directions[3] = new int2(-1, 0);


            this.housesLookup = state.GetComponentLookup<House>();
            this.gridLookup = state.GetComponentLookup<GridCellComponent>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.housesLookup.Update(ref state);
            this.gridLookup.Update(ref state);

            DateTime now = SystemAPI.GetSingleton<TimeManager>().dateTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter cmd = ecb.AsParallelWriter();

            SetJobTargetJob job = new()
            {
                day = now.Day,
                hour = now.Hour,
                roads = RoadGridManager.Instance.RoadGridArray,
                directions = directions,
                cmd = cmd,
                housesLookup = this.housesLookup,
                gridLookup = this.gridLookup,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            directions.Dispose();
        }
    }

    [BurstCompile]
    [WithAbsent(typeof(PathFindingRequest))]
    [WithAbsent(typeof(HasPathFindingPath))]
    public partial struct SetJobTargetJob : IJobEntity
    {
        [ReadOnly]
        public int day;

        [ReadOnly]
        public int hour;

        [ReadOnly]
        public NativeArray<RoadCell>.ReadOnly roads;

        [ReadOnly]
        public NativeArray<int2> directions;

        [WriteOnly]
        public EntityCommandBuffer.ParallelWriter cmd;

        [ReadOnly]
        public ComponentLookup<House> housesLookup;

        [ReadOnly]
        public ComponentLookup<GridCellComponent> gridLookup;

        [BurstCompile]
        public void Execute(Entity citizenEntity, ref CitizenJob job, ref Citizen citizen, [ChunkIndexInQuery] int sortKey)
        {
            if (hour >= job.startHour && hour < job.endHour && citizen.activty != CitizenActivity.AtOffice)
            {
                citizen.activty = CitizenActivity.AtOffice;
                cmd.AddComponent(sortKey, citizenEntity, new PathFindingRequest() { roadTarget = GetClosestRoad(job.officeBuildingIndex), target = job.officeBuildingIndex });
            }
            else if (hour >= job.endHour && citizen.activty == CitizenActivity.AtOffice)
            {

                citizen.activty = CitizenActivity.AtHome;

                RefRO<House> house = this.housesLookup.GetRefRO(citizen.house);
                RefRO<GridCellComponent> gridIndex = this.gridLookup.GetRefRO(house.ValueRO.building);

                cmd.AddComponent(sortKey, citizenEntity, new PathFindingRequest() { roadTarget = GetClosestRoad(gridIndex.ValueRO.index), target = gridIndex.ValueRO.index });
            }
        }

        [BurstCompile] private int2 GetClosestRoad(int2 target)
        {
            int tmp;
            int res = -1;

            foreach (int2 direction in directions)
            {
                tmp = IndexToArrayIndex(target + direction);

                if (tmp < roads.Length && roads[tmp].Type != RoadType.None)
                {
                    res = tmp;

                    break;
                }
            }

            if (res == -1)
            {
                UnityEngine.Debug.LogError("SetJobTargetJob : office without road around");
                return int2.zero;
            }

            return ArrayIndexToIndex(res);
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
}