using quentin.tran.authoring.building;
using quentin.tran.authoring.grid;
using quentin.tran.common;
using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation.component;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace quentin.tran.simulation.system.grid
{
    /// <summary>
    /// System which handles <see cref="IBuildingCellCommand"/> commands : creates or deletes entities.
    /// </summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    partial struct SpawnBuildingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RoadPrefab>();
            state.RequireForUpdate<HouseBuildingPrefabs>();
            state.RequireForUpdate<JobBuildingPrefabs>();
        }

        public void OnUpdate(ref SystemState state)
        {
            RoadPrefab roadPrefabs = SystemAPI.GetSingleton<RoadPrefab>();
            HouseBuildingPrefabs housePrefabs = SystemAPI.GetSingleton<HouseBuildingPrefabs>();
            JobBuildingPrefabs jobBuildingPrefabs = SystemAPI.GetSingleton<JobBuildingPrefabs>();


            EntityCommandBuffer entityCmdBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

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

                            GridCellKeys.SIMPLE_HOUSE_01 => housePrefabs.simpleHouse01,

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
                        Debug.LogError("SpawnBuildingSystem : Command unknown ");
                        break;
                }
            }

            entityCmdBuffer.Playback(state.EntityManager);
            entityCmdBuffer.Dispose();
        }

        //[BurstCompile]
        private void Delete(DeleteBuildCellCommand deleteCmd, ref SystemState state, ref EntityCommandBuffer cmd)
        {
            switch (deleteCmd.buildingType)
            {
                case models.grid.GridCellType.House:
                    // 2. Delete house
                    // 2.1 Remove every worker from their jobs
                    // 2.2 Remove every child from school and student from university
                    // 2.3 Delete citizens
                    break;
                case models.grid.GridCellType.Office:

                    // 1. Delete building
                    // 1.1 Remove job for every citizen who were working here
                    foreach((RefRO<GridCellComponent> cell, DynamicBuffer<LinkedEntityBuffer> workers) in SystemAPI.Query<RefRO<GridCellComponent>, DynamicBuffer<LinkedEntityBuffer>>().WithAll<OfficeBuilding>())
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


