using quentin.tran.simulation.component;
using quentin.tran.simulation.system.grid;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer cmd = new(Unity.Collections.Allocator.Temp);

            foreach ((RefRO<LocalTransform> citizenTransform, Entity citizenEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAbsent<CitizenJob>().WithAll<CitizenAdult>().WithEntityAccess())
            {
                RefRW<OfficeBuilding> officeWithEmploy = default;
                Entity officeBuilding = Entity.Null;
                DynamicBuffer<LinkedEntityBuffer> workers = default;

                float sqDistanceToOffice = float.MaxValue;

                // Find closest office with an available job
                foreach ((RefRW<OfficeBuilding> office, RefRO<LocalToWorld> transform, DynamicBuffer<LinkedEntityBuffer> w, Entity officeEntity) in
                    SystemAPI.Query<RefRW<OfficeBuilding>, RefRO<LocalToWorld>, DynamicBuffer<LinkedEntityBuffer>>().WithEntityAccess())
                {
                    float currentDistance = math.lengthsq(transform.ValueRO.Position - citizenTransform.ValueRO.Position);

                    if (office.ValueRO.nbOfAvailableJob > 0 && currentDistance < sqDistanceToOffice)
                    {
                        sqDistanceToOffice = currentDistance;

                        officeWithEmploy = office;
                        officeBuilding = officeEntity;
                        workers = w;
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
                officeWithEmploy.ValueRW.nbOfAvailableJob--;
                workers.Add(new LinkedEntityBuffer { entity = citizenEntity });
            }

            cmd.Playback(state.EntityManager);
            cmd.Dispose();
        }
    }
}

