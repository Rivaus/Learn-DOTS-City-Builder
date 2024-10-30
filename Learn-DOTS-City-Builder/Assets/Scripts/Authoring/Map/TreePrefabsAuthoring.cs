using quentin.tran.authoring.grid;
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
                });

                DynamicBuffer<TreeCollection> lowDensityHouses = AddBuffer<TreeCollection>(e);

                for (int i = 0; i < authoring.treePrefabs.Count; i++)
                    lowDensityHouses.Add(new() { entity = GetEntity(authoring.treePrefabs[i], TransformUsageFlags.Renderable) });
            }
        }
    }

    public struct TreePrefabs : IComponentData
    {
        public int firstLayerNbTrees;

        public int nbOfLayers;
    }

    public struct TreeCollection : IBufferElementData
    {
        public Entity entity;
    }
}
