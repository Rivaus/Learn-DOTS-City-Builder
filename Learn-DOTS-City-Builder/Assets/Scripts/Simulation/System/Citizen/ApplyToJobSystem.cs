using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using quentin.tran.simulation.component;
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
        private Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Citizen>();
            state.RequireForUpdate<CitizenAdult>();
            state.RequireForUpdate<OfficeBuilding>();

            this.random = new Random(123);

            UnityEngine.Debug.LogError("Prevent student from getting a job");
            UnityEngine.Debug.LogError("Find closest job");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer cmd = new(Unity.Collections.Allocator.Temp);

            foreach ((RefRO<CitizenAdult> _, Entity citizenEntity) in SystemAPI.Query<RefRO<CitizenAdult>>().WithAbsent<CitizenJob>().WithEntityAccess())
            {
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
}

