using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.grid
{
    /// <summary>
    /// Creates an entity which has all roads prefabs.
    /// </summary>
    public class HouseBuildingPrefabsAuthoring : MonoBehaviour
    {
        public List<GameObject> simpleHouse01Prefabs = new();

        private class Baker : Baker<HouseBuildingPrefabsAuthoring>
        {
            public override void Bake(HouseBuildingPrefabsAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);

                AddComponent(e, new HouseBuildingPrefabs()
                {
                    simpleHouse01_01 = GetEntity(authoring.simpleHouse01Prefabs[0], TransformUsageFlags.Dynamic),
                    simpleHouse01_02 = GetEntity(authoring.simpleHouse01Prefabs[1], TransformUsageFlags.Dynamic),
                    simpleHouse01_03 = GetEntity(authoring.simpleHouse01Prefabs[2], TransformUsageFlags.Dynamic),
                    simpleHouse01_04 = GetEntity(authoring.simpleHouse01Prefabs[3], TransformUsageFlags.Dynamic),
                    simpleHouse01_05 = GetEntity(authoring.simpleHouse01Prefabs[4], TransformUsageFlags.Dynamic),
                    simpleHouse01_06 = GetEntity(authoring.simpleHouse01Prefabs[5], TransformUsageFlags.Dynamic),
                    simpleHouse01_07 = GetEntity(authoring.simpleHouse01Prefabs[6], TransformUsageFlags.Dynamic),
                    simpleHouse01_08 = GetEntity(authoring.simpleHouse01Prefabs[7], TransformUsageFlags.Dynamic),
                    simpleHouse01_09 = GetEntity(authoring.simpleHouse01Prefabs[8], TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct HouseBuildingPrefabs : IComponentData
    {
        public Entity simpleHouse01_01;
        public Entity simpleHouse01_02;
        public Entity simpleHouse01_03;
        public Entity simpleHouse01_04;
        public Entity simpleHouse01_05;
        public Entity simpleHouse01_06;
        public Entity simpleHouse01_07;
        public Entity simpleHouse01_08;
        public Entity simpleHouse01_09;
    }
}