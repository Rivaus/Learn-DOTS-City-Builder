using quentin.tran.authoring;
using quentin.tran.authoring.weather;
using quentin.tran.gameplay;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.simulation.weather
{
    public partial struct WeatherSystem : ISystem
    {
        private int lastDay;

        private int lastMonth;

        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Weather>();
            state.RequireForUpdate<TimeManager>();
        }

        //[BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            RefRW<Weather> weather = SystemAPI.GetSingletonRW<Weather>();
            TimeManager time = SystemAPI.GetSingleton<TimeManager>();

            // For now every month, it snows from the 5th to the 9th
            if (this.lastMonth != time.dateTime.Month && this.lastDay == 5)
            {
                this.lastMonth = time.dateTime.Month;
                weather.ValueRW.daysOfRain = 4;
            }

            bool isRaining = weather.ValueRO.daysOfRain > 0;

            if (isRaining)
            {
                weather.ValueRW.snowLevel = math.lerp(weather.ValueRO.snowLevel, 0.5f, SystemAPI.Time.DeltaTime * 0.01f * time.timeScale);

                if (this.lastDay != time.dateTime.Day)
                {
                    weather.ValueRW.daysOfRain = math.clamp(weather.ValueRO.daysOfRain - 1, 0, int.MaxValue);
                }
            }
            else
            {
                weather.ValueRW.snowLevel = math.lerp(weather.ValueRO.snowLevel, 0f, SystemAPI.Time.DeltaTime * 0.01f * time.timeScale);
            }

            VFXManager.Instance.rainVFX.SetActive(isRaining);

            Shader.SetGlobalFloat("_SnowLevel", weather.ValueRO.snowLevel);

            lastDay = time.dateTime.Day;
        }

        private void OnDestroy(ref SystemState state)
        {
            Shader.SetGlobalFloat("_SnowLevel", 0);
        }
    }
}
