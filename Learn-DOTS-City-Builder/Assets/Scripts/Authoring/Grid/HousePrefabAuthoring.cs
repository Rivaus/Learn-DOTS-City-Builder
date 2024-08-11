using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    /// <summary>
    /// Creates an entity which has all roads prefabs.
    /// </summary>
    public class HousePrefabAuthoring : MonoBehaviour
    {
        public GameObject simpleHouse01 = null;

        private class Baker : Baker<HousePrefabAuthoring>
        {
            public override void Bake(HousePrefabAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new HousePrefab()
                {
                    simpleHouse01 = GetEntity(authoring.simpleHouse01, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct HousePrefab : IComponentData
    {
        public Entity simpleHouse01;
    }
}