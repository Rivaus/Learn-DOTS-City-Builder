using quentin.tran.authoring.grid;
using quentin.tran.common;
using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation.component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace quentin.tran.simulation.system.grid
{
    /// <summary>
    /// System which handles <see cref="IBuildingCellCommand"/> commands : creates or deletes entities.
    /// </summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    partial struct SpawnBuildingSystem : ISystem, ISystemStartStop
    {
        private Random random;
        public NativeArray<Entity> simpleHouse01Prefabs;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RoadPrefab>();
            state.RequireForUpdate<HouseBuildingPrefabs>();
            state.RequireForUpdate<JobBuildingPrefabs>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            RefRW<HouseBuildingPrefabs> housePrefabs = SystemAPI.GetSingletonRW<HouseBuildingPrefabs>();
            this.simpleHouse01Prefabs = new(9, Allocator.Persistent);

            this.simpleHouse01Prefabs[0] = housePrefabs.ValueRO.simpleHouse01_01;
            this.simpleHouse01Prefabs[1] = housePrefabs.ValueRO.simpleHouse01_02;
            this.simpleHouse01Prefabs[2] = housePrefabs.ValueRO.simpleHouse01_03;
            this.simpleHouse01Prefabs[3] = housePrefabs.ValueRO.simpleHouse01_04;
            this.simpleHouse01Prefabs[4] = housePrefabs.ValueRO.simpleHouse01_05;
            this.simpleHouse01Prefabs[5] = housePrefabs.ValueRO.simpleHouse01_06;
            this.simpleHouse01Prefabs[6] = housePrefabs.ValueRO.simpleHouse01_07;
            this.simpleHouse01Prefabs[7] = housePrefabs.ValueRO.simpleHouse01_08;
            this.simpleHouse01Prefabs[8] = housePrefabs.ValueRO.simpleHouse01_09;

            var now = System.DateTime.Now;

            this.random = Random.CreateFromIndex((uint)(now.Second + now.Minute + now.Hour));
        }

        public void OnStopRunning(ref SystemState state)
        {
            this.simpleHouse01Prefabs.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            RoadPrefab roadPrefabs = SystemAPI.GetSingleton<RoadPrefab>();
            HouseBuildingPrefabs housePrefabs = SystemAPI.GetSingleton<HouseBuildingPrefabs>();
            JobBuildingPrefabs jobBuildingPrefabs = SystemAPI.GetSingleton<JobBuildingPrefabs>();

            EntityCommandBuffer entityCmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            while (BuilderController.Instance.commands.Count > 0)
            {
                IBuildingCellCommand cmd = BuilderController.Instance.commands.Dequeue();

                switch (cmd)
                {
                    case CreateBuildCellCommand createCmd:

                        Entity entity = createCmd.cellKey switch
                        {
                            0 => Entity.Null,
                            GridCellKeys.ROAD_2x2 => roadPrefabs.road2x2Prefab,
                            GridCellKeys.ROAD_2x2_ONE_NEIGHBOUR => roadPrefabs.road2x2OneNeighbourPrefab,
                            GridCellKeys.ROAD_2x2_NO_NEIGHBOUR => roadPrefabs.road2x2NoNeighbourPrefab,
                            GridCellKeys.ROAD_2x2_TURN => roadPrefabs.road2x2TurnPrefab,
                            GridCellKeys.ROAD_2x2_CROSSROAD => roadPrefabs.road2x2CrossRoadPrefab,
                            GridCellKeys.ROAD_2x2_T_TURN => roadPrefabs.road2x2TTurnPrefab,

                            GridCellKeys.SIMPLE_HOUSE_01 => this.simpleHouse01Prefabs[this.random.NextInt(0, this.simpleHouse01Prefabs.Length)],

                            GridCellKeys.SIMPLE_JOB_OFFICE_01 => jobBuildingPrefabs.simpleOffice01,

                            _ => Entity.Null
                        };

                        if (entity == Entity.Null)
                            continue;

                        float4x4 matrix = SystemAPI.GetComponentRO<LocalToWorld>(entity).ValueRO.Value;

                        Entity createdBuilding = entityCmdBuffer.Instantiate(entity);
                        entityCmdBuffer.SetComponent(createdBuilding, new LocalTransform()
                        {
                            Position = createCmd.position,
                            Rotation = math.mul(createCmd.rotation, matrix.Rotation()),
                            Scale = matrix.Scale().x
                        });
                        entityCmdBuffer.AddComponent(createdBuilding, new GridCellComponent()
                        {
                            index = createCmd.index,
                        });

                        if (SystemAPI.HasComponent<HouseBuilding>(entity))
                        {
                            RefRO<HouseBuilding> building = SystemAPI.GetComponentRO<HouseBuilding>(entity);

                            for (int i = 0; i < building.ValueRO.nbOfHouses; i++)
                            {
                                Entity house = entityCmdBuffer.CreateEntity();
                                entityCmdBuffer.AppendToBuffer<LinkedEntityBuffer>(createdBuilding, new() { entity = house });

                                entityCmdBuffer.AddComponent(house, new House()
                                {
                                    building = createdBuilding,
                                    nbOfResidents = 0,
                                    capacity = building.ValueRO.houseCapacity
                                });
                                entityCmdBuffer.AddBuffer<LinkedEntityBuffer>(house); // A buffer to store all inhabitants
                            }
                        }

                        break;
                    case DeleteBuildCellCommand deleteCmd:

                        Delete(deleteCmd, ref state, ref entityCmdBuffer);

                        break;
                    default:
                        UnityEngine.Debug.LogError("SpawnBuildingSystem : Command unknown ");
                        break;
                }
            }

            entityCmdBuffer.Playback(state.EntityManager);
            entityCmdBuffer.Dispose();
        }

        [BurstCompile]
        private void Delete(DeleteBuildCellCommand deleteCmd, ref SystemState _, ref EntityCommandBuffer cmd)
        {
            // Delete specific data
            switch (deleteCmd.buildingType)
            {
                case models.grid.GridCellType.House:

                    foreach ((RefRO<GridCellComponent> cell, DynamicBuffer<LinkedEntityBuffer> houses) in SystemAPI.Query<RefRO<GridCellComponent>, DynamicBuffer<LinkedEntityBuffer>>().WithAll<HouseBuilding>())
                    {
                        if (!cell.ValueRO.index.Equals(deleteCmd.index))
                            continue;


                        for (int i = 0; i < houses.Length; i++)
                        {
                            // Delete house inhabitants
                            DynamicBuffer<LinkedEntityBuffer> inhabitants = SystemAPI.GetBuffer<LinkedEntityBuffer>(houses[i].entity);

                            for (int j = 0; j < inhabitants.Length; j++)
                            {
                                Entity inhabitant = inhabitants[j].entity;

                                // Remove every worker from their jobs
                                if (SystemAPI.HasComponent<CitizenJob>(inhabitant))
                                {
                                    foreach ((RefRW<OfficeBuilding> office, DynamicBuffer<LinkedEntityBuffer> workers) in SystemAPI.Query<RefRW<OfficeBuilding>, DynamicBuffer<LinkedEntityBuffer>>())
                                    {
                                        int workerFound = -1;

                                        for (int k = 0; k < workers.Length; k++)
                                        {
                                            if (workers[k].entity == inhabitant)
                                            {
                                                office.ValueRW.nbOfAvailableJob = math.clamp(office.ValueRO.nbOfAvailableJob + 1, 0, office.ValueRO.nbJobs); // Free a job

                                                workerFound = k;
                                                break;
                                            }
                                        }

                                        if (workerFound >= 0)
                                        {
                                            workers.RemoveAt(workerFound);
                                            break;
                                        }
                                    }
                                }

                                // Remove every child from school and student from university
                                UnityEngine.Debug.Log("TODO Remove from school");

                                // Destroy inhabitant
                                cmd.DestroyEntity(inhabitant);
                            }

                            // Delete houses
                            cmd.DestroyEntity(houses[i].entity);
                        }
                    }

                    break;
                case models.grid.GridCellType.Office:

                    // 1. Delete building
                    // 2. Remove job for every citizen who were working here
                    foreach ((RefRO<GridCellComponent> cell, DynamicBuffer<LinkedEntityBuffer> workers) in SystemAPI.Query<RefRO<GridCellComponent>, DynamicBuffer<LinkedEntityBuffer>>().WithAll<OfficeBuilding>())
                    {
                        if (!cell.ValueRO.index.Equals(deleteCmd.index))
                            continue;

                        for (int i = 0; i < workers.Length; i++)
                        {
                            cmd.RemoveComponent<CitizenJob>(workers[i].entity);
                        }
                    }

                    break;
                case models.grid.GridCellType.School:
                    break;
                default:
                    break;
            }

            // Delete global entity
            foreach ((var gridCell, var e) in SystemAPI.Query<RefRO<GridCellComponent>>().WithEntityAccess())
            {
                if (gridCell.ValueRO.index.Equals(deleteCmd.index))
                {
                    DynamicBuffer<LinkedEntityGroup> group = SystemAPI.GetBuffer<LinkedEntityGroup>(e);

                    foreach (LinkedEntityGroup child in group)
                    {
                        cmd.DestroyEntity(child.Value);
                    }

                    break;
                }
            }
        }
    }

    public struct GridCellComponent : IComponentData
    {
        public int2 index;
    }
}


