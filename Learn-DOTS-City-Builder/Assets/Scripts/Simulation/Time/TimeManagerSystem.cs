using quentin.tran.authoring;
using System;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(UpdateTimeSystemGroup), OrderFirst = true)]
public partial struct TimeManagerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TimeManager>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        RefRW<TimeManager> manager = SystemAPI.GetSingletonRW<TimeManager>();

        manager.ValueRW.dateTime = manager.ValueRO.dateTime.AddMinutes(SystemAPI.Time.DeltaTime * manager.ValueRO.timeScale);
    }

}
