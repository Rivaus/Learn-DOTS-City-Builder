using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using quentin.tran.simulation.system.grid;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Apply To Job System : unemployed citizens, not student, in range 18 -> 65 years old try to get a job
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup))]
    [UpdateAfter(typeof(SpawnCitizenSystem))]
    partial struct ApplyToJobSystem : ISystem
    {
        public const int MIN_AGE_TO_WORK = 18;
        public const int MAX_AGE_TO_WORK = 65;

        private Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Citizen>();
            state.RequireForUpdate<OfficeBuilding>();

            this.random = new Random(123);

            UnityEngine.Debug.LogError("Prevent student from getting a job");
            UnityEngine.Debug.LogError("Prevent citizens too young or old to loop through update");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer cmd = new(Unity.Collections.Allocator.Temp);

            foreach((RefRO<Citizen> citizen, Entity citizenEntity) in SystemAPI.Query<RefRO<Citizen>>().WithAbsent<CitizenJob>().WithEntityAccess())
            {
                if (citizen.ValueRO.age < MIN_AGE_TO_WORK || citizen.ValueRO.age > MAX_AGE_TO_WORK)
                {
                    continue;
                }

                RefRW<OfficeBuilding> officeWithEmploy = default;
                Entity officeBuilding = Entity.Null;

                foreach ((RefRW<OfficeBuilding> office, Entity officeEntity) in SystemAPI.Query<RefRW<OfficeBuilding>>().WithEntityAccess())
                {
                    if (office.ValueRO.nbOfAvailableJob > 0)
                    {
                        office.ValueRW.nbOfAvailableJob--;
                        officeWithEmploy = office;
                        officeBuilding = officeEntity;
                    }
                }

                if (!officeWithEmploy.IsValid || officeBuilding == Entity.Null)
                {
                    break;
                }

                if (!SystemAPI.HasComponent<GridCellComponent>(officeBuilding))
                {
                    UnityEngine.Debug.LogError("ApplyToJobSystem.OnUpdate : internal error");
                }
                int2 officeIndex = SystemAPI.GetComponentRO<GridCellComponent>(officeBuilding).ValueRO.index;
                CitizenJob job = new CitizenJob()
                {
                    salaryPerDay = this.random.NextInt((int)officeWithEmploy.ValueRO.salaryRangePerDay.x, (int)officeWithEmploy.ValueRO.salaryRangePerDay.y),
                    startHour = officeWithEmploy.ValueRO.startHour,
                    endHour = officeWithEmploy.ValueRO.endHour,
                    officeBuildingIndex = officeIndex,
                };

                cmd.AddComponent(citizenEntity, job);
            }

            cmd.Playback(state.EntityManager);
            cmd.Dispose();
        }
    }

    public struct CitizenJob : IComponentData
    {
        public int salaryPerDay;

        public int startHour, endHour;

        public int2 officeBuildingIndex;
    }
}

