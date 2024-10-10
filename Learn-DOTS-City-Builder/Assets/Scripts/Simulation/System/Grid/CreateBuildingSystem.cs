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
    partial struct CreateBuildingSystem : ISystem, ISystemStartStop
    {
        private Random random;

        public NativeArray<Entity> simpleHouse01Prefabs;
        public NativeArray<Entity> simpleOffice01Prefabs;

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

            RefRW<JobBuildingPrefabs> simpleOffice01Prefabs = SystemAPI.GetSingletonRW<JobBuildingPrefabs>();
            this.simpleOffice01Prefabs = new(9, Allocator.Persistent);

            this.simpleOffice01Prefabs[0] = simpleOffice01Prefabs.ValueRO.simpleOffice01;
            this.simpleOffice01Prefabs[1] = simpleOffice01Prefabs.ValueRO.simpleOffice02;
            this.simpleOffice01Prefabs[2] = simpleOffice01Prefabs.ValueRO.simpleOffice03;
            this.simpleOffice01Prefabs[3] = simpleOffice01Prefabs.ValueRO.simpleOffice04;
            this.simpleOffice01Prefabs[4] = simpleOffice01Prefabs.ValueRO.simpleOffice05;
            this.simpleOffice01Prefabs[5] = simpleOffice01Prefabs.ValueRO.simpleOffice06;
            this.simpleOffice01Prefabs[6] = simpleOffice01Prefabs.ValueRO.restaurant01;
            this.simpleOffice01Prefabs[7] = simpleOffice01Prefabs.ValueRO.restaurant02;
            this.simpleOffice01Prefabs[8] = simpleOffice01Prefabs.ValueRO.restaurant03;

            var now = System.DateTime.Now;

            this.random = Random.CreateFromIndex((uint)(now.Second + now.Minute + now.Hour));
        }

        public void OnStopRunning(ref SystemState state)
        {
            this.simpleHouse01Prefabs.Dispose();
            this.simpleOffice01Prefabs.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            RoadPrefab roadPrefabs = SystemAPI.GetSingleton<RoadPrefab>();

            EntityCommandBuffer entityCmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            Queue<CreateBuildingEntityCommand> commands = BuilderController.Instance.createBuildingCommands;

            while (commands.Count > 0)
            {
                CreateBuildingEntityCommand cmd = commands.Dequeue();

                Create(ref state, cmd, ref entityCmdBuffer, ref roadPrefabs);
                DestroyDecoration(ref state, cmd.index, ref entityCmdBuffer);
            }

            entityCmdBuffer.Playback(state.EntityManager);
            entityCmdBuffer.Dispose();
        }

        [BurstCompile]
        private void Create(ref SystemState _, CreateBuildingEntityCommand createCmd, ref EntityCommandBuffer entityCmdBuffer, ref RoadPrefab roadPrefabs)
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

                GridCellKeys.SIMPLE_HOUSE_01 => this.simpleHouse01Prefabs[this.random.NextInt(0, this.simpleHouse01Prefabs.Length)],

                GridCellKeys.SIMPLE_JOB_OFFICE_01 => this.simpleOffice01Prefabs[this.random.NextInt(0, this.simpleOffice01Prefabs.Length)],

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


