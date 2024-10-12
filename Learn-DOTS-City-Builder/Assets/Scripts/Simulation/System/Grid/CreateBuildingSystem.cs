using quentin.tran.authoring.grid;
using quentin.tran.common;
using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation.component;
using quentin.tran.simulation.component.map;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace quentin.tran.simulation.system.grid
{
    /// <summary>
    /// System which handles <see cref="IBuildingEntityCommand"/> commands : creates entities.
    /// </summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    partial struct CreateBuildingSystem : ISystem
    {
        private Random random;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RoadPrefab>();
            state.RequireForUpdate<HouseBuildingPrefabs>();
            state.RequireForUpdate<ShopBuildingPrefabs>();

            var now = System.DateTime.Now;
            this.random = Random.CreateFromIndex((uint)(now.Second + now.Minute + now.Hour));
        }

        public void OnUpdate(ref SystemState state)
        {
            RoadPrefab roadPrefabs = SystemAPI.GetSingleton<RoadPrefab>();
            Entity buildingPrefabs = SystemAPI.GetSingletonEntity<HouseBuildingPrefabs>();
            Entity shopPrefabs = SystemAPI.GetSingletonEntity<ShopBuildingPrefabs>();

            DynamicBuffer<LowDensityHouseCollection> lowDensityPrefabs = SystemAPI.GetBuffer<LowDensityHouseCollection>(buildingPrefabs);
            DynamicBuffer<LowDensityShopCollection> lowDensityShops = SystemAPI.GetBuffer<LowDensityShopCollection>(shopPrefabs);

            EntityCommandBuffer entityCmdBuffer = new (Allocator.Temp);

            Queue<CreateBuildingEntityCommand> commands = BuilderController.Instance.createBuildingCommands;

            while (commands.Count > 0)
            {
                CreateBuildingEntityCommand cmd = commands.Dequeue();

                Create(ref state, cmd, ref entityCmdBuffer, ref roadPrefabs, lowDensityPrefabs, lowDensityShops);
                DestroyDecoration(ref state, cmd.index, ref entityCmdBuffer);
            }

            entityCmdBuffer.Playback(state.EntityManager);
            entityCmdBuffer.Dispose();
        }

        [BurstCompile]
        private void Create(ref SystemState _, CreateBuildingEntityCommand createCmd, ref EntityCommandBuffer entityCmdBuffer, ref RoadPrefab roadPrefabs,
            DynamicBuffer<LowDensityHouseCollection> lowDensityHouses,
            DynamicBuffer<LowDensityShopCollection> lowDensityShops)
        {
            // 1. Find entity to spawn
            Entity entity = createCmd.cellKey switch
            {
                0 => Entity.Null,
                GridCellKeys.ROAD_2x2 => roadPrefabs.road2x2Prefab,
                GridCellKeys.ROAD_2x2_ONE_NEIGHBOUR => roadPrefabs.road2x2OneNeighbourPrefab,
                GridCellKeys.ROAD_2x2_NO_NEIGHBOUR => roadPrefabs.road2x2NoNeighbourPrefab,
                GridCellKeys.ROAD_2x2_TURN => roadPrefabs.road2x2TurnPrefab,
                GridCellKeys.ROAD_2x2_CROSSROAD => roadPrefabs.road2x2CrossRoadPrefab,
                GridCellKeys.ROAD_2x2_T_TURN => roadPrefabs.road2x2TTurnPrefab,

                GridCellKeys.SIMPLE_HOUSE_01 => lowDensityHouses[this.random.NextInt(0, lowDensityHouses.Length)].entity,

                GridCellKeys.SIMPLE_SHOP_01 => lowDensityShops[this.random.NextInt(0, lowDensityShops.Length)].entity,

                _ => Entity.Null
            };

            if (entity == Entity.Null)
                return;

            float4x4 matrix = SystemAPI.GetComponentRO<LocalToWorld>(entity).ValueRO.Value;

            // 2. Spawn it
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

            // 3. If it's a house building, create entity for each of its apartment. (a simple house has one apartment)
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
        }

        [BurstCompile]
        private void DestroyDecoration(ref SystemState _, int2 index, ref EntityCommandBuffer entityCmdBuffer)
        {
            foreach ((RefRO<MapDecoration> decoration, Entity e) in SystemAPI.Query<RefRO<MapDecoration>>().WithEntityAccess())
            {
                if (decoration.ValueRO.index.Equals(index))
                    entityCmdBuffer.DestroyEntity(e);
            }
        }
    }

    public struct GridCellComponent : IComponentData
    {
        public int2 index;
    }
}


