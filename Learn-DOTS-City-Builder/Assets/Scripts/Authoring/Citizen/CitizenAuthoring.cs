using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.citizen
{
    public class CitizenAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CitizenAuthoring>
        {
            public override void Bake(CitizenAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }
}

