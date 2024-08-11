using quentin.tran.authoring.building;
using quentin.tran.authoring.grid;
using quentin.tran.common;
using quentin.tran.gameplay.buildingTool;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VectorGraphics;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
            state.RequireForUpdate<HousePrefab>();
        }

        public void OnUpdate(ref SystemState state)
        {
            RoadPrefab roadPrefabs = SystemAPI.GetSingleton<RoadPrefab>();
            HousePrefab housePrefabs = SystemAPI.GetSingleton<HousePrefab>();
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

                        if (SystemAPI.HasComponent<Building>(entity))
                        {
                            RefRO<Building> building = SystemAPI.GetComponentRO<Building>(entity);
                            for (int i = 0; i < building.ValueRO.nbOfHouses; i++)
                            {
                                Entity house = entityCmdBuffer.CreateEntity();
                                entityCmdBuffer.AddComponent(house, new House()
                                {
                                    building = createdBuilding,
                                    nbOfResidents = 0,
                                    capacity = building.ValueRO.houseCapacity
                                });
                            }
                        }

                        break;
                    case DeleteBuildCellCommand deleteCmd:

                        foreach((var gridCell, var e) in SystemAPI.Query<RefRO<GridCellComponent>>().WithEntityAccess())
                        {
                            if (gridCell.ValueRO.index.Equals(deleteCmd.index))
                            {
                                DynamicBuffer<LinkedEntityGroup> group = SystemAPI.GetBuffer<LinkedEntityGroup>(e);

                                foreach (LinkedEntityGroup child in group)
                                {
                                    entityCmdBuffer.DestroyEntity(child.Value);
                                }

                                break;
                            }
                        }
                        
                        break;
                    default:
                        Debug.LogError("SpawnBuildingSystem : Command unknown ");
                        break;
                }
            }

            entityCmdBuffer.Playback(state.EntityManager);
            entityCmdBuffer.Dispose();
        }
    }

    public struct GridCellComponent : IComponentData
    {
        public int2 index;
    }
}


