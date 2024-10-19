using quentin.tran.authoring;
using quentin.tran.common;
using quentin.tran.simulation.component;
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
        private ComponentLookup<House> housesLookup;
        private ComponentLookup<GridCellComponent> gridLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CitizenJob>();
            state.RequireForUpdate<TimeManager>();

            this.housesLookup = state.GetComponentLookup<House>();
            this.gridLookup = state.GetComponentLookup<GridCellComponent>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            TimeManager timeManager = SystemAPI.GetSingleton<TimeManager>();
            if (timeManager.timeScale == 0)
                return;

            this.housesLookup.Update(ref state);
            this.gridLookup.Update(ref state);

            DateTime now = timeManager.dateTime;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter cmd = ecb.AsParallelWriter();

            SetJobTargetJob job = new()
            {
                day = now.Day,
                hour = now.Hour,
                cmd = cmd,
                housesLookup = this.housesLookup,
                gridLookup = this.gridLookup,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    [WithDisabled(typeof(PathFindingRequest))]
    [WithDisabled(typeof(HasPathFindingPath))]
    public partial struct SetJobTargetJob : IJobEntity
    {
        [ReadOnly]
        public int day;

        [ReadOnly]
        public int hour;

        [WriteOnly]
        public EntityCommandBuffer.ParallelWriter cmd;

        [ReadOnly]
        public ComponentLookup<House> housesLookup;

        [ReadOnly]
        public ComponentLookup<GridCellComponent> gridLookup;

        [BurstCompile]
        public void Execute(Entity citizenEntity, ref CitizenJob job, ref Citizen citizen, [ChunkIndexInQuery] int sortKey)
        {
            if (hour >= job.startHour && hour < job.endHour && citizen.activity != CitizenActivity.AtOffice)
            {
                citizen.activity = CitizenActivity.AtOffice;
                cmd.SetComponent(sortKey, citizenEntity, new PathFindingRequest() { target = CellIndexToPosition(job.officeBuildingIndex) });
                cmd.SetComponentEnabled<PathFindingRequest>(sortKey, citizenEntity, true);
            }
            else if (hour >= job.endHour && citizen.activity == CitizenActivity.AtOffice)
            {

                citizen.activity = CitizenActivity.AtHome;

                RefRO<House> house = this.housesLookup.GetRefRO(citizen.house);
                RefRO<GridCellComponent> gridIndex = this.gridLookup.GetRefRO(house.ValueRO.building);

                cmd.SetComponent(sortKey, citizenEntity, new PathFindingRequest() { target = CellIndexToPosition(gridIndex.ValueRO.index) });
                cmd.SetComponentEnabled<PathFindingRequest>(sortKey, citizenEntity, true);
            }
        }

        [BurstCompile]
        private float3 CellIndexToPosition(int2 index) => (new float3(index.x, 0, index.y) + new float3(.5f, 0, .5f)) * GridProperties.GRID_CELL_SIZE;
    }
}