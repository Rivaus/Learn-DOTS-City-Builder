using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    /// <summary>
    /// Creates an entity which has all roads prefabs.
    /// </summary>
    public class HouseBuildingPrefabsAuthoring : MonoBehaviour
    {
        public GameObject simpleHouse01 = null;

        private class Baker : Baker<HouseBuildingPrefabsAuthoring>
        {
            public override void Bake(HouseBuildingPrefabsAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new HouseBuildingPrefabs()
                {
                    simpleHouse01 = GetEntity(authoring.simpleHouse01, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct HouseBuildingPrefabs : IComponentData
    {
        public Entity simpleHouse01;
    }
}