using quentin.tran.authoring.map;
using quentin.tran.common;
using quentin.tran.simulation.component.map;
using quentin.tran.simulation.system.grid;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace quentin.tran.simulation.map
{
    partial struct GenerateTreesSystem : ISystem
    {
        private Unity.Mathematics.Random random;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TreePrefabs>();

            DateTime now = DateTime.Now;
            this.random = Unity.Mathematics.Random.CreateFromIndex((uint)(now.Second + now.Minute + now.Hour));
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            TreePrefabs trees = SystemAPI.GetSingleton<TreePrefabs>();

            NativeArray<Entity> prefabs = new(new Entity[]
            {
                trees.tree01_01, trees.tree01_02, trees.tree01_03,
                trees.tree02_01,  trees.tree02_02,  trees.tree02_03,
                trees.tree03_01,  trees.tree03_02,  trees.tree03_03,
                trees.tree04_01,  trees.tree04_02,  trees.tree04_03,
                trees.tree05_01,  trees.tree05_02,  trees.tree05_03,
            },
            Allocator.Temp);

            NativeHashMap<int2, int> countByCell = new(GridProperties.GRID_SIZE * GridProperties.GRID_SIZE, Allocator.Temp);

            int nbTreeSpawned = 0;

            float3 maxPosition = GridProperties.GRID_SIZE * GridProperties.GRID_CELL_SIZE * new float3(1, 0, 1);

            while (nbTreeSpawned < trees.firstLayerNbTrees)
            {
                int2 index = SpawnTree(ref state, float3.zero, maxPosition, prefabs[random.NextInt(0, prefabs.Length)]);

                if (countByCell.ContainsKey(index))
                    countByCell[index]++;
                else
                    countByCell.Add(index, 1);

                nbTreeSpawned++;
            }

            using var indexes = countByCell.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < trees.nbOfLayers; i++) // Iterate on nb of layers.
            {
                for (int j = 0; j < indexes.Length; j++)
                {
                    int2 index = indexes[j];

                    if (countByCell[index] > 1)
                    {


                        float3 min = new float3(GridProperties.GRID_CELL_SIZE * (index.x - 2), 0, GridProperties.GRID_CELL_SIZE * (index.y - 2));
                        float3 max = min + 2 * new float3(GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE);

                        int additional = 0;

                        for (int k = 0; k < random.NextInt(0, countByCell[index]); k++)
                        {
                            additional++;
                            SpawnTree(ref state, min, max, prefabs[random.NextInt(0, prefabs.Length)]);
                        }

                        countByCell[index] += additional;
                    }
                }
            }

            countByCell.Dispose();
            prefabs.Dispose();
        }

        [BurstCompile]
        private int2 SpawnTree(ref SystemState state, float3 minPosition, float3 maxPosition, Entity prefab)
        {
            float3 position = random.NextFloat3(minPosition, maxPosition);

            Entity tree = state.EntityManager.Instantiate(prefab);
            state.EntityManager.SetComponentData(tree, new LocalTransform()
            {
                Position = position,
                Rotation = quaternion.Euler(-math.PIHALF, random.NextFloat(0, math.PI2), 0),
                Scale = SystemAPI.GetComponent<LocalTransform>(prefab).Scale * 3
            });

            int2 index = new((int)(position.x / GridProperties.GRID_CELL_SIZE), (int)(position.z / GridProperties.GRID_CELL_SIZE));

            state.EntityManager.AddComponentData(tree, new MapDecoration()
            {
                index = index
            });

            return index;
        }
    }
}
