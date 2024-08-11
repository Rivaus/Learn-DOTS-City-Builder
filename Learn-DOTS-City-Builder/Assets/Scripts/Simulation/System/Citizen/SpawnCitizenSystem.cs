using quentin.tran.authoring;
using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace quentin.tran.simulation.system.citizen
{
    /// <summary>
    /// Spawns new citizens if city is attracting enough and with free houses.
    /// </summary>
    [UpdateInGroup(typeof(CitizenSystemGroup), OrderFirst = true)]
    partial struct SpawnCitizenSystem : ISystem, ISystemStartStop
    {
        private int currentHour, currentDay;

        private Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CitizenSpawner>();
            state.RequireForUpdate<TimeManager>();
            state.RequireForUpdate<House>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            TimeManager time = SystemAPI.GetSingleton<TimeManager>();

            this.currentDay = time.dateTime.Day;
            this.currentHour = time.dateTime.Hour - 2;

            this.random = Random.CreateFromIndex((uint)(this.currentHour + this.currentDay + time.dateTime.Year));
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            TimeManager time = SystemAPI.GetSingleton<TimeManager>();
            CitizenSpawner citizenSpawner = SystemAPI.GetSingleton<CitizenSpawner>();

            RefRO<LocalTransform> citizenPrefabTransform = SystemAPI.GetComponentRO<LocalTransform>(citizenSpawner.citizenPrefab);

            if (time.dateTime.Day != this.currentDay || time.dateTime.Hour >= this.currentHour + 2)
            {
                this.currentDay = time.dateTime.Day;
                this.currentHour = time.dateTime.Hour;

                EntityCommandBuffer cmd = new EntityCommandBuffer(Allocator.Temp);

                foreach ((RefRW<House> house, Entity houseEntity) in SystemAPI.Query<RefRW<House>>().WithEntityAccess())
                {
                    if (house.ValueRO.nbOfResidents == 0) // Find an empty house
                    {
                        var buildingTransform = SystemAPI.GetComponentRO<LocalTransform>(house.ValueRO.building);
                        int nbCitizens = random.NextInt(1, house.ValueRO.capacity + 1);

                        for (int i = 0; i < nbCitizens; i++) // Simulate a family of 1 to 4 persons
                        {
                            Entity citizen = cmd.Instantiate(citizenSpawner.citizenPrefab);
                            cmd.AddComponent(citizen, new Citizen()
                            {
                                age = random.NextInt(18, 100),
                                name = "Jean Dupont",
                                gender = random.NextBool() ? CitizenGender.Male : CitizenGender.Female,
                                house = houseEntity,
                                happiness = 50
                            });
                            cmd.SetComponent(citizen, new LocalTransform()
                            {
                                Position = buildingTransform.ValueRO.Position + math.rotate(quaternion.Euler(0, math.radians(random.NextInt(0, 360)), 0), math.forward()),
                                Rotation = citizenPrefabTransform.ValueRO.Rotation,
                                Scale = citizenPrefabTransform.ValueRO.Scale
                            });
                        }

                        house.ValueRW.nbOfResidents = nbCitizens;

                        break;
                    }
                }

                cmd.Playback(state.EntityManager);
                cmd.Dispose();
            }
        }
    }
}

