using quentin.tran.authoring;
using quentin.tran.authoring.citizen;
using quentin.tran.common;
using quentin.tran.simulation.component;
using quentin.tran.simulation.grid;
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

        public static NativeArray<FixedString32Bytes> maleNames, femaleNames, lastNames;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CitizenSpawner>();
            state.RequireForUpdate<TimeManager>();
            state.RequireForUpdate<House>();

            maleNames = new NativeArray<FixedString32Bytes>(CitizenNames.MALE_NAMES, Allocator.Persistent);
            femaleNames = new NativeArray<FixedString32Bytes>(CitizenNames.FEMALE_NAMES, Allocator.Persistent);
            lastNames = new NativeArray<FixedString32Bytes>(CitizenNames.LAST_NAMES, Allocator.Persistent);
        }

        private void OnDestroy(ref SystemState _)
        {
            maleNames.Dispose();
            femaleNames.Dispose();
            lastNames.Dispose();
        }

        //[BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            DateTime dateTime = SystemAPI.GetSingleton<TimeManager>().dateTime;

            this.currentDay = dateTime.Day;
            this.currentHour = dateTime.Hour - 2;

            this.random = Random.CreateFromIndex((uint)(this.currentHour + this.currentDay + dateTime.Year + dateTime.Second));
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

                foreach ((RefRW<House> house, DynamicBuffer<LinkedEntityBuffer> inhabitants, Entity houseEntity) in SystemAPI.Query<RefRW<House>, DynamicBuffer<LinkedEntityBuffer>>().WithEntityAccess())
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

                            FixedString32Bytes lastName = lastNames[random.NextInt(0, lastNames.Length)];

                            CreateCitizen(lastName, ageA, ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);

                            CreateCitizen(lastName, ageB, ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);

                            nbResidents++;
                            nbResidents++;

                            int nChildren = random.NextInt(0, 3);

                            for (int i = 0; i < nChildren; i++)
                            {
                                nbResidents++;

                                CreateCitizen(lastName, random.NextInt(1, 20), ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                    citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);
                            }
                        }
                        else // it's a collocation or a single person
                        {
                            int nbCitizens = random.NextInt(1, house.ValueRO.capacity + 1);

                            for (int i = 0; i < nbCitizens; i++)
                            {
                                CreateCitizen(lastNames[random.NextInt(0, lastNames.Length)], random.NextInt(18, 77), ref cmd, citizenSpawner.citizenPrefab, houseEntity, buildingTransform.ValueRO.Position,
                                    citizenPrefabTransform.ValueRO.Rotation, citizenPrefabTransform.ValueRO.Scale);
                            }

                            nbResidents++;
                        }

                        house.ValueRW.nbOfResidents = nbResidents;

                        //break;
                    }
                }

                cmd.Playback(state.EntityManager);
                cmd.Dispose();
            }
        }

        private void CreateCitizen(FixedString32Bytes lastName, int age, ref EntityCommandBuffer cmd, Entity prefab, Entity house, float3 pos, quaternion rot, float scale)
        {
            Entity citizen = cmd.Instantiate(prefab);

            CitizenGender gender = random.NextBool() ? CitizenGender.Male : CitizenGender.Female;
            FixedString32Bytes name = (gender == CitizenGender.Male) ? maleNames[random.NextInt(0, maleNames.Length)] : femaleNames[random.NextInt(0, maleNames.Length)];

            cmd.AddComponent(citizen, new Citizen()
            {
                age = age,
                name = name + " " + lastName,
                gender = gender,
                house = house,
                happiness = 50
            });
            cmd.SetComponent(citizen, new LocalTransform()
            {
                Position = pos + math.rotate(quaternion.Euler(0, math.radians(random.NextInt(0, 360)), 0), math.forward()),
                Rotation = rot,
                Scale = scale
            });

            switch (age)
            {
                case < CitizenConsts.MIN_AGE_CHILD:
                    cmd.AddComponent<CitizenBaby>(citizen);
                    break;
                case >= CitizenConsts.MIN_AGE_CHILD and < CitizenConsts.MIN_AGE_TEENAGER:
                    cmd.AddComponent<CitizenBaby>(citizen);
                    break;
                case >= CitizenConsts.MIN_AGE_TEENAGER and < CitizenConsts.MIN_AGE_ADULT:
                    cmd.AddComponent<CitizenTeenager>(citizen);
                    break;
                case >= CitizenConsts.MIN_AGE_ADULT and < CitizenConsts.MIN_AGE_RETIRED:
                    cmd.AddComponent<CitizenAdult>(citizen);
                    break;
                case >= CitizenConsts.MIN_AGE_RETIRED:
                    cmd.AddComponent<CitizenSenior>(citizen);
                    break;
                default:
            }

            cmd.AddBuffer<Waypoint>(citizen);
            cmd.AppendToBuffer<LinkedEntityBuffer>(house, new LinkedEntityBuffer() { entity = citizen });

            cmd.AddComponent<PathFindingRequest>(citizen);
            cmd.SetComponentEnabled<PathFindingRequest>(citizen, false);
            cmd.AddComponent<HasPathFindingPath>(citizen);
            cmd.SetComponentEnabled<HasPathFindingPath>(citizen, false);
        }
    }
}

