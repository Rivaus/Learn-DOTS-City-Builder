using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    /// <summary>
    /// Creates an entity which has all roads prefabs.
    /// </summary>
    public class RoadPrefabAuthoring : MonoBehaviour
    {
        public GameObject road2x2Prefab = null;

        public GameObject road2x2NoNeighbour = null;

        public GameObject road2x2OneNeighbour = null;

        public GameObject road2x2TurnPrefab = null;

        public GameObject road2x2CrossRoadPrefab = null;

        public GameObject road2x2TTurnPrefab = null;

        private class Baker : Baker<RoadPrefabAuthoring>
        {
            public override void Bake(RoadPrefabAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new RoadPrefab()
                {
                    road2x2Prefab = GetEntity(authoring.road2x2Prefab, TransformUsageFlags.Dynamic),
                    road2x2NoNeighbourPrefab = GetEntity(authoring.road2x2NoNeighbour, TransformUsageFlags.Dynamic),
                    road2x2OneNeighbourPrefab = GetEntity(authoring.road2x2OneNeighbour, TransformUsageFlags.Dynamic),
                    road2x2TurnPrefab = GetEntity(authoring.road2x2TurnPrefab, TransformUsageFlags.Dynamic),
                    road2x2CrossRoadPrefab = GetEntity(authoring.road2x2CrossRoadPrefab, TransformUsageFlags.Dynamic),
                    road2x2TTurnPrefab = GetEntity(authoring.road2x2TTurnPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct RoadPrefab : IComponentData
    {
        public Entity road2x2Prefab;

        public Entity road2x2NoNeighbourPrefab;

        public Entity road2x2OneNeighbourPrefab;

        public Entity road2x2TurnPrefab;

        public Entity road2x2CrossRoadPrefab;

        public Entity road2x2TTurnPrefab;
    }
}

