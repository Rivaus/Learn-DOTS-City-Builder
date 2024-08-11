using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.citizen
{
    public class CitizenSpawnerAuthoring : MonoBehaviour
    {
        public GameObject citizenPrefab;

        private class Baker : Baker<CitizenSpawnerAuthoring>
        {
            public override void Bake(CitizenSpawnerAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new CitizenSpawner() { citizenPrefab = GetEntity(authoring.citizenPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }

    public struct CitizenSpawner : IComponentData
    {
        public Entity citizenPrefab;
    }
}

