using quentin.tran.authoring.map;
using quentin.tran.common;
using quentin.tran.simulation.component.map;
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
            DynamicBuffer<TreeCollection> treePrefabs = SystemAPI.GetSingletonBuffer<TreeCollection>();

            NativeHashMap<int2, int> countByCell = new(GridProperties.GRID_SIZE * GridProperties.GRID_SIZE, Allocator.Temp);

            int nbTreeSpawned = 0;

            float3 maxPosition = GridProperties.GRID_SIZE * GridProperties.GRID_CELL_SIZE * new float3(1, 0, 1);

            EntityCommandBuffer cmd = new(Allocator.Temp);

            while (nbTreeSpawned < trees.firstLayerNbTrees)
            {
                int2 index = SpawnTree(ref state, float3.zero, maxPosition, treePrefabs[random.NextInt(0, treePrefabs.Length)].entity, ref cmd);

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
                            SpawnTree(ref state, min, max, treePrefabs[random.NextInt(0, treePrefabs.Length)].entity, ref cmd);
                        }

                        countByCell[index] += additional;
                    }
                }
            }

            cmd.Playback(state.EntityManager);
            cmd.Dispose();

            countByCell.Dispose();
        }

        [BurstCompile]
        private int2 SpawnTree(ref SystemState state, float3 minPosition, float3 maxPosition, Entity prefab, ref EntityCommandBuffer cmd)
        {
            float3 position = random.NextFloat3(minPosition, maxPosition);

            Entity tree = cmd.Instantiate(prefab);
            cmd.SetComponent(tree, new LocalTransform()
            {
                Position = position,
                Rotation = quaternion.Euler(-math.PIHALF, random.NextFloat(0, math.PI2), 0),
                Scale = SystemAPI.GetComponent<LocalTransform>(prefab).Scale * 3
            });

            int2 index = new((int)(position.x / GridProperties.GRID_CELL_SIZE), (int)(position.z / GridProperties.GRID_CELL_SIZE));

            cmd.AddComponent(tree, new MapDecoration()
            {
                index = index
            });

            return index;
        }
    }
}
