using quentin.tran.authoring;
using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

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

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            DateTime dateTime = SystemAPI.GetSingleton<TimeManager>().dateTime;

            this.currentDay = dateTime.Day;
            this.currentHour = dateTime.Hour - 2;

            this.random = Random.CreateFromIndex((uint)(this.currentHour + this.currentDay + dateTime.Year));
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            DateTime dateTime = SystemAPI.GetSingleton<TimeManager>().dateTime;

            CitizenSpawner citizenSpawner = SystemAPI.GetSingleton<CitizenSpawner>();

            RefRO<LocalTransform> citizenPrefabTransform = SystemAPI.GetComponentRO<LocalTransform>(citizenSpawner.citizenPrefab);

            if (dateTime.Day != this.currentDay || dateTime.Hour >= this.currentHour + 2)
            {
                this.currentDay = dateTime.Day;
                this.currentHour = dateTime.Hour;

                EntityCommandBuffer cmd = new EntityCommandBuffer(Allocator.Temp);

                foreach ((RefRW<House> house, Entity houseEntity) in SystemAPI.Query<RefRW<House>>().WithEntityAccess())
                {
                    if (house.ValueRO.nbOfResidents == 0) // Find an empty house
                    {
                        var buildingTransform = SystemAPI.GetComponentRO<LocalTransform>(house.ValueRO.building);

                        int nbResidents = 0;
                        
                        if (random.NextInt(0, 100) < 65) // chance to be a couple
                        {
                            // First 
                            int ageA = random.NextInt(25, 60);
                            int ageB = math.clamp(random.NextInt(ageA - 10, ageA + 10), 20, 60);

                            CreateCitizen("Jean", "Dupont", ageA, ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);

                            CreateCitizen("Jean", "Dupont", ageB, ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);

                            nbResidents++;
                            nbResidents++;

                            int nChildren = random.NextInt(0, 3);

                            for (int i = 0; i < nChildren; i++)
                            {
                                nbResidents++;

                                CreateCitizen("Jean", "Dupont", random.NextInt(1, 20), ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                    citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);
                            }
                        }
                        else // it's a collocation or a single person
                        {
                            int nbCitizens = random.NextInt(1, house.ValueRO.capacity + 1);

                            for (int i = 0; i < nbCitizens; i++)
                            {
                                CreateCitizen("Jean", "Dupont", random.NextInt(22, 70), ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                    citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);
                            }

                            nbResidents++;
                        }

                        house.ValueRW.nbOfResidents = nbResidents;

                        break;
                    }
                }

                cmd.Playback(state.EntityManager);
                cmd.Dispose();
            }
        }

        private void CreateCitizen(string firstName, string lastName, int age, ref EntityCommandBuffer cmd, Entity prefab, Entity house, float3 pos, quaternion rot, float scale)
        {
            Entity citizen = cmd.Instantiate(prefab);
            cmd.AddComponent(citizen, new Citizen()
            {
                age = age,
                name = firstName + " " + lastName,
                gender = random.NextBool() ? CitizenGender.Male : CitizenGender.Female,
                house = house,
                happiness = 50
            });
            cmd.SetComponent(citizen, new LocalTransform()
            {
                Position = pos + math.rotate(quaternion.Euler(0, math.radians(random.NextInt(0, 360)), 0), math.forward()),
                Rotation = rot,
                Scale = scale
            });
        }
    }
}

