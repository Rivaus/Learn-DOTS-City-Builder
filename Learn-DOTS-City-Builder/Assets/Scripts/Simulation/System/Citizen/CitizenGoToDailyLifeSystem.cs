using quentin.tran.authoring;
using quentin.tran.common;
using quentin.tran.simulation.grid;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Citizen Go To DailyLife System : in the morning, set a citizen target if their are students or employed
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(ApplyToJobSystem))]
    partial struct CitizenGoToDailyLifeSystem : ISystem
    {
        NativeArray<int2> directions;

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
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            DateTime now = SystemAPI.GetSingleton<TimeManager>().dateTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter cmd = ecb.AsParallelWriter();

            SetJobTargetJob job = new()
            {
                day = now.Day,
                hour = now.Hour,
                roads = RoadGridManager.Instance.RoadGridArray,
                directions = directions,
                cmd = cmd
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

        public EntityCommandBuffer.ParallelWriter cmd;

        [BurstCompile]
        public void Execute(Entity citizenEntity, in CitizenJob job, [ChunkIndexInQuery] int sortKey)
        {
            if (hour > job.startHour)
            {
                int2 officeIndex = job.officeBuildingIndex;

                int tmp;
                int2 res = new(-1, -1);

                foreach (int2 direction in directions)
                {
                    tmp = IndexToArrayIndex(officeIndex + direction);

                    if (tmp < roads.Length && roads[tmp].Type != RoadType.None)
                    {
                        res = tmp;

                        break;
                    }
                }

                if (res.x == -1)
                {
                    UnityEngine.Debug.LogError("SetJobTargetJob : office without road around");
                    return;
                }

                UnityEngine.Debug.Log("GO JOB");
                cmd.AddComponent(sortKey, citizenEntity, new PathFindingRequest() { roadTarget = res, target = officeIndex });
            }
        }

        [BurstCompile]
        private int IndexToArrayIndex(int2 index) => index.x + index.y * GridProperties.GRID_SIZE;
    }

    public struct PathFindingRequest : IComponentData
    {
        public int2 roadTarget;

        public int2 target;
    }
}