using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.map
{
    public class TreePrefabsAuthoring : MonoBehaviour
    {
        public List<GameObject> treePrefabs = new();

        public int firstLayerNbTrees;

        public int nbOfLayers;

        private class Baker : Baker<TreePrefabsAuthoring>
        {
            public override void Bake(TreePrefabsAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);

                AddComponent(e, new TreePrefabs()
                {
                    firstLayerNbTrees = authoring.firstLayerNbTrees,

                    nbOfLayers = authoring.nbOfLayers,

                    tree01_01 = GetEntity(authoring.treePrefabs[0], TransformUsageFlags.Dynamic),
                    tree01_02 = GetEntity(authoring.treePrefabs[1], TransformUsageFlags.Dynamic),
                    tree01_03 = GetEntity(authoring.treePrefabs[2], TransformUsageFlags.Dynamic),
                    tree02_01 = GetEntity(authoring.treePrefabs[3], TransformUsageFlags.Dynamic),
                    tree02_02 = GetEntity(authoring.treePrefabs[4], TransformUsageFlags.Dynamic),
                    tree02_03 = GetEntity(authoring.treePrefabs[5], TransformUsageFlags.Dynamic),
                    tree03_01 = GetEntity(authoring.treePrefabs[6], TransformUsageFlags.Dynamic),
                    tree03_02 = GetEntity(authoring.treePrefabs[7], TransformUsageFlags.Dynamic),
                    tree03_03 = GetEntity(authoring.treePrefabs[8], TransformUsageFlags.Dynamic),
                    tree04_01 = GetEntity(authoring.treePrefabs[9], TransformUsageFlags.Dynamic),
                    tree04_02 = GetEntity(authoring.treePrefabs[10], TransformUsageFlags.Dynamic),
                    tree04_03 = GetEntity(authoring.treePrefabs[11], TransformUsageFlags.Dynamic),
                    tree05_01 = GetEntity(authoring.treePrefabs[12], TransformUsageFlags.Dynamic),
                    tree05_02 = GetEntity(authoring.treePrefabs[13], TransformUsageFlags.Dynamic),
                    tree05_03 = GetEntity(authoring.treePrefabs[14], TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct TreePrefabs : IComponentData
    {
        public int firstLayerNbTrees;

        public int nbOfLayers;

        public Entity tree01_01;

        public Entity tree01_02;

        public Entity tree01_03;

        public Entity tree02_01;

        public Entity tree02_02;

        public Entity tree02_03;

        public Entity tree03_01;

        public Entity tree03_02;

        public Entity tree03_03;

        public Entity tree04_01;

        public Entity tree04_02;

        public Entity tree04_03;

        public Entity tree05_01;

        public Entity tree05_02;

        public Entity tree05_03;
    }
}
